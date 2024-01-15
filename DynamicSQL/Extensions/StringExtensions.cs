namespace DynamicSQL.Extensions;

using System.Collections.Generic;
using DynamicSQL.Parser;
using DynamicSQL.Parser.Expressions;

internal static class StringExtensions
{
    public static IEnumerable<TokenExpression> Tokenize(this string value)
    {
        return new TokenizerEnumerable(value);
    }
}
