namespace DynamicSQL.Parser.Expressions;

internal class InArrayExpression(int parameterIndex) : IParsedExpression
{
    public int ParameterIndex { get; } = parameterIndex;

    public bool TryReduce(ExpressionStack stack) => false;
}
