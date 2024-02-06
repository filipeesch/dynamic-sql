namespace DynamicSQL.Parser.Expressions;

using System;

internal class InterpolationExpression(int index) : IParsedExpression
{
    public int Index { get; } = index;

    public bool TryReduce(ExpressionStack stack)
    {
        if (stack[0] is not InterpolationExpression)
        {
            throw new Exception("Invalid braces closing");
        }

        if (stack[1] is not InOperatorExpression)
        {
            return false;
        }

        var interpolation = (InterpolationExpression)stack.Pop();
        _ = (InOperatorExpression)stack.Pop();

        stack.Push(new InArrayExpression(interpolation.Index));
        return true;
    }
}
