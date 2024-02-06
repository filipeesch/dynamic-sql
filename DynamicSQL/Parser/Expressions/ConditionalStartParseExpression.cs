namespace DynamicSQL.Parser.Expressions;

internal class ConditionalStartParseExpression(int conditionValueInterpolationIndex) : IParsedExpression
{
    public int ConditionValueInterpolationIndex { get; } = conditionValueInterpolationIndex;

    public bool TryReduce(ExpressionStack stack) => false;
}
