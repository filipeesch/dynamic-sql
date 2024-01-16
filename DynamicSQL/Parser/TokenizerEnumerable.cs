namespace DynamicSQL.Parser;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DynamicSQL.Parser.Expressions;

internal class TokenizerEnumerable : IEnumerable<TokenExpression>
{
    private static readonly Regex TokensRegex = new(@"<<|>>|\{|\}|\?|:|\s+IN\s*", RegexOptions.Singleline);
    private readonly IReadOnlyList<TokenExpression> _tokens;

    public TokenizerEnumerable(string template)
    {
        _tokens = TokensRegex
            .Matches(template)
            .Cast<Match>()
            .Select(match => TokenExpression.FromSymbol(match.Value.Trim(), match.Index, match.Index + match.Length - 1))
            .ToList();
    }

    public IEnumerator<TokenExpression> GetEnumerator() => _tokens.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
