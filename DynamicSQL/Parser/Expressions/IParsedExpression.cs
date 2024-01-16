namespace DynamicSQL.Parser.Expressions;

public interface IParsedExpression
{
    bool TryReduce(ExpressionStack stack);
}
