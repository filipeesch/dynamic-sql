namespace DynamicSQL.Parser.Expressions;

internal class ConditionalStartParseExpression(int conditionValueParameterIndex) : IParsedExpression
{
    public int ConditionValueParameterIndex { get; } = conditionValueParameterIndex;

    public bool TryReduce(ExpressionStack stack) => false;
}
