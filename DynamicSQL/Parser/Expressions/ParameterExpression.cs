namespace DynamicSQL.Parser.Expressions;

using System;

internal class ParameterExpression(int parameterIndex) : IParsedExpression
{
    public int ParameterIndex { get; } = parameterIndex;

    public bool TryReduce(ExpressionStack stack)
    {
        if (stack[0] is not ParameterExpression)
        {
            throw new Exception("Invalid braces closing");
        }

        if (stack[1] is not InOperatorExpression)
        {
            return false;
        }

        var parameter = (ParameterExpression)stack.Pop();
        _ = (InOperatorExpression)stack.Pop();

        stack.Push(new InArrayExpression(parameter.ParameterIndex));
        return true;
    }
}
