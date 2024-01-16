namespace DynamicSQL.Parser;

using System.Collections.Generic;
using DynamicSQL.Parser.Expressions;

public class ParsedStatement(IReadOnlyCollection<IParsedExpression> expressions)
{
    public IReadOnlyCollection<IParsedExpression> Expressions { get; } = expressions;
}
