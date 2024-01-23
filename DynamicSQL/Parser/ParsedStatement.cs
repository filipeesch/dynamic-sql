namespace DynamicSQL.Parser;

using System.Collections.Generic;
using DynamicSQL.Parser.Expressions;

public class ParsedStatement(IReadOnlyCollection<IParsedExpression> expressions, IReadOnlyList<IParsedExpression> variationExpressions)
{
    public IReadOnlyCollection<IParsedExpression> Expressions { get; } = expressions;

    public IReadOnlyList<IParsedExpression> VariationExpressions { get; } = variationExpressions;
}
