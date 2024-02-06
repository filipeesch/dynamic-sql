namespace DynamicSQL.Parser.Expressions;

internal class InArrayExpression(int interpolationIndex) : IParsedExpression
{
    public int InterpolationIndex { get; } = interpolationIndex;

    public bool TryReduce(ExpressionStack stack) => false;
}
