namespace DynamicSQL.Parser.Expressions;

using System.Collections.Generic;

internal class ConditionalExpression : IParsedExpression
{
    public ConditionalExpression(
        int conditionValueIndex,
        IReadOnlyCollection<IParsedExpression> truePartNodes,
        IReadOnlyCollection<IParsedExpression> falsePartNodes)
    {
        ConditionValueIndex = conditionValueIndex;
        TruePartNodes = truePartNodes;
        FalsePartNodes = falsePartNodes;
    }

    public int ConditionValueIndex { get; }

    public IReadOnlyCollection<IParsedExpression> TruePartNodes { get; }

    public IReadOnlyCollection<IParsedExpression> FalsePartNodes { get; }

    public static ConditionalExpression Reduce(IEnumerable<IParsedExpression> expressions)
    {
        using var enumerator = expressions.GetEnumerator();

        enumerator.MoveNext();
        var startExpression = (ConditionalStartParseExpression)enumerator.Current!;

        var trueExpressions = GetConditionalSubExpressions(enumerator);
        var falseExpressions = GetConditionalSubExpressions(enumerator);

        return new ConditionalExpression(
            startExpression.ConditionValueInterpolationIndex,
            trueExpressions,
            falseExpressions);
    }

    private static IReadOnlyList<IParsedExpression> GetConditionalSubExpressions(IEnumerator<IParsedExpression> enumerator)
    {
        var nodes = new List<IParsedExpression>();

        while (enumerator.MoveNext())
        {
            if (enumerator.Current is ColonExpression)
            {
                break;
            }

            nodes.Add(enumerator.Current);
        }

        return nodes;
    }

    public bool TryReduce(ExpressionStack stack) => false;
}
