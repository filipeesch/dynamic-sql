namespace DynamicSQL;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class QueryParser
{
    private static readonly Regex TemplateTagRegex = new(
        """
        <<                    # Match the opening '<<'
        (                     # Start of capturing group for the entire content
            (?>
                [^<>]+        # Match any characters except '<' or '>'
                |
                <(?!<)        # Match a single '<' not followed by another '<'
                |
                >(?!>)        # Match a single '>' not followed by another '>'
                |
                <<(?<Depth>)  # Match '<<', increment the 'Depth' counter
                |
                >>(?<-Depth>) # Match '>>', decrement the 'Depth' counter
            )*
            (?(Depth)(?!))    # If 'Depth' is not zero, fail the match
        )
        >>                    # Match the closing '>>'
        """,
        RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);

    private static readonly Regex ConditionalOperatorRegex = new(
        @"\s*{(\d+)}\s*\?\s*([^:]+)\s*(?::(.+))?",
        RegexOptions.Singleline);

    private static readonly Regex ParameterComparisonRegex = new(
        @"(?:\s*(=|<>|>|<|>=|<=|\w+)\s+)?{(\d+)}",
        RegexOptions.Singleline);

    public ParsedQuery Parse(string template)
    {
        var nodes = ParsePart(template);

        return new ParsedQuery(nodes);
    }

    private static IReadOnlyCollection<IParsedQueryNode> ParsePart(string part)
    {
        var matches = TemplateTagRegex.Matches(part);

        var lastIndex = 0;

        var nodes = new List<IParsedQueryNode>();

        foreach (Match match in matches)
        {
            if (lastIndex < match.Index)
            {
                nodes.Add(new RawNode(part.Substring(lastIndex, match.Index - lastIndex)));
            }

            var group = match.Groups[1].Value;

            var conditionalOperatorMatch = ConditionalOperatorRegex.Match(group);

            if (conditionalOperatorMatch.Success)
            {
                nodes.Add(CreateConditionalNode(conditionalOperatorMatch));
            }

            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < part.Length)
        {
            nodes.Add(new RawNode(part.Substring(lastIndex, part.Length - lastIndex)));
        }

        return nodes;
    }

    private static ConditionalNode CreateConditionalNode(Match match)
    {
        var conditionValueIndex = int.Parse(match.Groups[1].Value);
        var truePart = match.Groups[2].Value;
        var falsePart = match.Groups[3].Value;

        var truePartNodes = ParsePart(truePart);
        var falsePartNodes = falsePart == string.Empty ?
            Array.Empty<IParsedQueryNode>() :
            ParsePart(falsePart);

        return new ConditionalNode(conditionValueIndex, truePartNodes, falsePartNodes);
    }
}

public class ParsedQuery
{
    public ParsedQuery(IReadOnlyCollection<IParsedQueryNode> nodes)
    {
        Nodes = nodes;
    }

    public IReadOnlyCollection<IParsedQueryNode> Nodes { get; }
}

public interface IParsedQueryNode
{
}

public class ConditionalNode : IParsedQueryNode
{
    public ConditionalNode(
        int conditionValueIndex,
        IReadOnlyCollection<IParsedQueryNode> truePartNodes,
        IReadOnlyCollection<IParsedQueryNode> falsePartNodes)
    {
        ConditionValueIndex = conditionValueIndex;
        TruePartNodes = truePartNodes;
        FalsePartNodes = falsePartNodes;
    }

    public int ConditionValueIndex { get; }

    public IReadOnlyCollection<IParsedQueryNode> TruePartNodes { get; }

    public IReadOnlyCollection<IParsedQueryNode> FalsePartNodes { get; }
}

public class RawNode : IParsedQueryNode
{
    public string Part { get; }

    public RawNode(string part)
    {
        Part = part;
    }
}
