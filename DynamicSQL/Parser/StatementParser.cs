namespace DynamicSQL.Parser;

using System.Linq;
using DynamicSQL.Extensions;
using DynamicSQL.Parser.Expressions;

public class StatementParser
{
    public ParsedStatement Parse(string template)
    {
        var stack = new ExpressionStack();

        ParseInternal(template, stack);

        return new ParsedStatement(
            stack
                .Reverse()
                .ToList());
    }

    private void ParseInternal(string template, ExpressionStack stack)
    {
        var index = 0;

        foreach (var token in template.Tokenize())
        {
            TryStackTextExpression(template, stack, index, token.StartIndex - index);

            index = token.EndIndex + 1;
            stack.Push(token);
        }

        TryStackTextExpression(template, stack, index, template.Length - index);
    }

    private static void TryStackTextExpression(string template, ExpressionStack stack, int index, int length)
    {
        var text = template
            .Substring(index, length)
            .Trim(' ', '\n', '\r');

        if (!string.IsNullOrEmpty(text))
        {
            stack.Push(new TextExpression(text));
        }
    }
}
