namespace DynamicSQL.Parser.Expressions;

using System;

internal class CloseBracesExpression(int startIndex, int endIndex) : TokenExpression(startIndex, endIndex)
{
    public override bool TryReduce(ExpressionStack stack)
    {
        if (stack.Pop() is not CloseBracesExpression)
        {
            throw new Exception("Invalid braces closing");
        }

        if (stack.Pop() is not TextExpression textExpression)
        {
            throw new Exception("Invalid expression between braces");
        }

        if (!int.TryParse(textExpression.Text, out var index))
        {
            throw new Exception("Invalid parameter index value");
        }

        if (stack.Pop() is not OpenBracesExpression)
        {
            throw new Exception("Braces closing without opening");
        }

        stack.Push(new InterpolationExpression(index));

        return true;
    }
}
