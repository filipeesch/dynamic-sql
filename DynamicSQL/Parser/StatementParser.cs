namespace DynamicSQL.Parser;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class StatementParser
{
    public ParsedStatement Parse(string template)
    {
        var nodes = ParsePart(template);

        return new ParsedStatement(nodes);
    }

    private static IReadOnlyCollection<IParsedStatementNode> ParsePart(string part)
    {
        var matches = RegexDefinitions.TemplateTag.Matches(part);

        var lastIndex = 0;

        var nodes = new List<IParsedStatementNode>();

        foreach (Match match in matches)
        {
            if (lastIndex < match.Index)
            {
                nodes.AddRange(CreateOffTagsNodes(part.Substring(lastIndex, match.Index - lastIndex)));
            }

            var group = match.Groups[1].Value;

            var conditionalOperatorMatch = RegexDefinitions.ConditionalOperator.Match(group);

            if (conditionalOperatorMatch.Success)
            {
                nodes.Add(CreateConditionalNode(conditionalOperatorMatch));
            }

            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < part.Length)
        {
            nodes.AddRange(CreateOffTagsNodes(part.Substring(lastIndex, part.Length - lastIndex)));
        }

        return nodes;
    }

    private static IEnumerable<IParsedStatementNode> CreateOffTagsNodes(string part)
    {
        var matches = RegexDefinitions.Parameters.Matches(part);

        var lastIndex = 0;

        var nodes = new List<IParsedStatementNode>();

        foreach (Match match in matches)
        {
            if (lastIndex < match.Index)
            {
                nodes.Add(new CodeNode(part.Substring(lastIndex, match.Index - lastIndex)));
            }

            nodes.Add(
                new ParameterNode(
                    match.Groups[1].Value,
                    int.Parse(match.Groups[2].Value)));

            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < part.Length)
        {
            nodes.Add(new CodeNode(part.Substring(lastIndex, part.Length - lastIndex)));
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
            Array.Empty<IParsedStatementNode>() :
            ParsePart(falsePart);

        return new ConditionalNode(conditionValueIndex, truePartNodes, falsePartNodes);
    }
}
