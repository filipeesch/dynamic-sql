namespace DynamicSQL.Parser;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DynamicSQL.Parser.Expressions;

public class ExpressionStack : IEnumerable<IParsedExpression>
{
    private readonly Stack<IParsedExpression> _expressions = new();

    private readonly List<IParsedExpression> _variationExpressions = new();

    public IReadOnlyList<IParsedExpression> VariationExpressions => _variationExpressions;

    public void Push(IParsedExpression expression)
    {
        _expressions.Push(expression);
        expression.TryReduce(this);

        switch (_expressions.Peek())
        {
            case ConditionalExpression exp:
                _variationExpressions.Add(exp);
                break;
            case InArrayExpression exp:
                _variationExpressions.Add(exp);
                break;
        }
    }

    public IParsedExpression? this[int index] => _expressions.ElementAtOrDefault(index);

    public IParsedExpression Pop() => _expressions.Pop();

    public IParsedExpression Peek() => _expressions.Peek();

    public IEnumerator<IParsedExpression> GetEnumerator() => _expressions.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
