namespace DynamicSQL.Extensions;

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DynamicSQL.Parser.Expressions;

internal static class StringExtensions
{
    private static readonly Regex TokensRegex = new(@"<<|>>|\{|\}|\?|:|\s+IN\s+", RegexOptions.Singleline);

    public static IEnumerable<TokenExpression> Tokenize(this string value)
    {
        return TokensRegex
            .Matches(value)
            .Cast<Match>()
            .Select(match => TokenExpression.FromSymbol(match.Value.Trim(), match.Index, match.Index + match.Length - 1));
    }
}
