namespace DynamicSQL.Parser.Expressions;

using System;
using System.Collections.Generic;
using System.Linq;

internal abstract class TokenExpression(int startIndex, int endIndex) : IParsedExpression
{
    private static readonly IReadOnlyCollection<TokenExpressionResolver> TokenExpressionResolvers = new[]
    {
        new TokenExpressionResolver(
            symbol => symbol == TokenSymbols.OpenTag,
            (startIndex, endIndex) => new[] { new OpenTagExpression(startIndex, endIndex) }),
        new TokenExpressionResolver(
            symbol => symbol == TokenSymbols.CloseTag,
            (startIndex, endIndex) => new[] { new CloseTagExpression(startIndex, endIndex) }),
        new TokenExpressionResolver(
            symbol => symbol == TokenSymbols.OpenBraces,
            (startIndex, endIndex) => new[] { new OpenBracesExpression(startIndex, endIndex) }),
        new TokenExpressionResolver(
            symbol => symbol == TokenSymbols.CloseBraces,
            (startIndex, endIndex) => new[] { new CloseBracesExpression(startIndex, endIndex) }),
        new TokenExpressionResolver(
            symbol => symbol == TokenSymbols.QuestionMark,
            (startIndex, endIndex) => new[] { new QuestionMarkExpression(startIndex, endIndex) }),
        new TokenExpressionResolver(
            symbol => symbol == TokenSymbols.Colon,
            (startIndex, endIndex) => new[] { new ColonExpression(startIndex, endIndex) }),
        new TokenExpressionResolver(
            symbol =>
            {
                if (symbol.Length < TokenSymbols.InOperator.Length + TokenSymbols.OpenBraces.Length)
                {
                    return false;
                }

                var symbolSpan = symbol.AsSpan();

                var slice = symbolSpan.Slice(0, TokenSymbols.InOperator.Length);

                if (!slice.Equals(TokenSymbols.InOperator.AsSpan(), StringComparison.Ordinal))
                {
                    return false;
                }

                slice = symbolSpan.Slice(symbol.Length - TokenSymbols.OpenBraces.Length,
                    TokenSymbols.OpenBraces.Length);

                return slice.Equals(TokenSymbols.OpenBraces.AsSpan(), StringComparison.Ordinal);
            },
            (startIndex, endIndex) =>
                new TokenExpression[]
                {
                    new InOperatorExpression(startIndex, endIndex - 1),
                    new OpenBracesExpression(endIndex, endIndex)
                }),
    };

    public int StartIndex { get; } = startIndex;

    public int EndIndex { get; } = endIndex;

    public virtual bool TryReduce(ExpressionStack stack) => false;

    public static IEnumerable<TokenExpression> FromSymbol(string symbol, int startIndex, int endIndex)
    {
        var resolver = TokenExpressionResolvers.FirstOrDefault(resolver => resolver.Matches(symbol));

        if (resolver != null)
        {
            return resolver.GetExpressions(startIndex, endIndex);
        }

        throw new ArgumentOutOfRangeException($"Symbol '{symbol}' is not supported");
    }

    private class TokenExpressionResolver
    {
        public TokenExpressionResolver(
            Func<string, bool> matches,
            Func<int, int, IEnumerable<TokenExpression>> getExpressions)
        {
            this.Matches = matches;
            this.GetExpressions = getExpressions;
        }

        public Func<string, bool> Matches { get; }

        public Func<int, int, IEnumerable<TokenExpression>> GetExpressions { get; }
    };
}