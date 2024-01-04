namespace DynamicSQL.Parser;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SqlParser
{
    public ParsedSql Parse(string template)
    {
        var nodes = ParsePart(template);

        return new ParsedSql(nodes);
    }

    private static IReadOnlyCollection<IParsedSqlNode> ParsePart(string part)
    {
        var matches = RegexDefinitions.TemplateTag.Matches(part);

        var lastIndex = 0;

        var nodes = new List<IParsedSqlNode>();

        foreach (Match match in matches)
        {
            if (lastIndex < match.Index)
            {
                nodes.Add(CodeNode.Create(part.Substring(lastIndex, match.Index - lastIndex)));
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
            nodes.Add(CodeNode.Create(part.Substring(lastIndex, part.Length - lastIndex)));
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
            Array.Empty<IParsedSqlNode>() :
            ParsePart(falsePart);

        return new ConditionalNode(conditionValueIndex, truePartNodes, falsePartNodes);
    }
}
