namespace DynamicSQL.Parser;

using System;
using System.Collections.Generic;

public class StatementParserV2
{
    private static readonly string[] Tokens =
    [
        TokenType.OpenTag,
        TokenType.CloseTag,
        TokenType.OpenBraces,
        TokenType.CloseBraces,
        TokenType.QuestionMark,
        TokenType.Colon,
    ];

    public void Parse(string template)
    {
        var stack = new Stack<IParseExpression>();
        ParseScope(template.AsMemory(), stack);
    }

    private void ParseScope(ReadOnlyMemory<char> scope, Stack<IParseExpression> stack)
    {
        var index = 0;
        var nodes = new List<IParsedStatementNode>();

        while (true)
        {
            var token = NextToken(scope, index);

            var text = scope
                .Slice(index, token.Index - index)
                .ToString()
                .Trim(' ', '\n', '\r');

            if (text != string.Empty)
            {
                stack.Push(new TextExpression(text));
            }

            if (token.Type == TokenType.End)
            {
                break;
            }

            ProcessToken(scope, token, stack);

            index = token.Index + token.Type.Length;
        }
    }

    private void ProcessToken(ReadOnlyMemory<char> scope, Token token, Stack<IParseExpression> stack)
    {
        switch (token.Type)
        {
            case TokenType.OpenTag:
                stack.Push(token);
                ParseScope(scope.Slice(token.Index + TokenType.OpenTag.Length), stack);
                break;

            case TokenType.CloseTag:
                // end scope
                break;

            case TokenType.OpenBraces:
                stack.Push(token);
                break;

            case TokenType.CloseBraces:
                stack.Push(token);
                ReduceToParameterExpression(scope, stack);
                break;

            case TokenType.QuestionMark:
                stack.Push(token);
                ReduceToConditionalStartExpression(scope, stack);
                break;

            case TokenType.Colon:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ReduceToConditionalStartExpression(ReadOnlyMemory<char> scope, Stack<IParseExpression> stack)
    {
        if (stack.Pop() is not Token { Type: TokenType.QuestionMark })
        {
            throw new Exception($"Conditional Start should ends with '{TokenType.QuestionMark}'");
        }

        if (stack.Pop() is not ParameterParseExpression parameter)
        {
            throw new Exception("Conditional expression must have a parameter expression");
        }

        if (stack.Peek() is not Token { Type: TokenType.OpenTag })
        {
            throw new Exception($"Conditional expression must start with an '{TokenType.OpenTag}'");
        }

        stack.Push(new ConditionalStartParseExpression(parameter.ParameterIndex));
    }

    private void ReduceToParameterExpression(ReadOnlyMemory<char> scope, Stack<IParseExpression> stack)
    {
        if (stack.Pop() is not Token { Type: TokenType.CloseBraces })
        {
            throw new Exception("Invalid braces closing");
        }

        if (stack.Pop() is not TextExpression textExpression)
        {
            throw new Exception("Invalid expression between braces");
        }

        if (!int.TryParse(textExpression.Text, out var index))
        {
            throw new Exception("Invalid parameter index value");
        }

        if (stack.Pop() is not Token { Type: TokenType.OpenBraces })
        {
            throw new Exception("Braces closing without opening");
        }

        stack.Push(new ParameterParseExpression(index));
    }

    private Token NextToken(ReadOnlyMemory<char> scope, int index)
    {
        for (var i = index + 1; i < scope.Length; i++)
        {
            foreach (var token in Tokens)
            {
                var c = scope.Slice(i, token.Length).Span;

                if (c.SequenceCompareTo(token.AsSpan()) == 0)
                {
                    return new Token(token, i);
                }
            }
        }

        return new Token(TokenType.End, scope.Length);
    }

    // private Token PreviousToken(Memory<char> scope, int index)
    // {
    //     var span = template.AsSpan();
    //
    //     for (var i = index - 1; i >= 0; i--)
    //     {
    //         foreach (var token in Tokens)
    //         {
    //             var tokenIndex = i - token.Length;
    //
    //             var c = span.Slice(tokenIndex, token.Length);
    //
    //             if (c.CompareTo(token.AsSpan(), StringComparison.InvariantCulture) == 0)
    //             {
    //                 return new Token(token, i);
    //             }
    //         }
    //     }
    //
    //     return Token.Empty;
    // }
}

internal interface IParseExpression
{
}

internal class TextExpression(string text) : IParseExpression
{
    public string Text { get; } = text;
}

internal class ParameterParseExpression(int parameterIndex) : IParseExpression
{
    public int ParameterIndex { get; } = parameterIndex;
}

internal class ConditionalStartParseExpression(int conditionValueParameterIndex) : IParseExpression
{
    public int ConditionValueParameterIndex { get; } = conditionValueParameterIndex;
}

internal class Token(string type, int index) : IParseExpression
{
    public string Type { get; } = type.Trim();

    public int Index { get; } = index;
}

internal static class TokenType
{
    public const string End = "";
    public const string OpenTag = "<<";
    public const string CloseTag = ">>";
    public const string OpenBraces = "{";
    public const string CloseBraces = "}";
    public const string QuestionMark = "?";
    public const string Colon = ":";
}
