namespace DynamicSQL.Parser.Expressions;

using System;

internal class QuestionMarkExpression(int startIndex, int endIndex) : TokenExpression(startIndex, endIndex)
{
    public override bool TryReduce(ExpressionStack stack)
    {
        if (stack.Pop() is not QuestionMarkExpression)
        {
            throw new Exception($"Conditional Start should ends with '{TokenSymbols.QuestionMark}'");
        }

        if (stack.Pop() is not InterpolationExpression parameter)
        {
            throw new Exception("Conditional expression must have a parameter expression");
        }

        if (stack.Peek() is not OpenTagExpression)
        {
            throw new Exception($"Conditional expression must start with an '{TokenSymbols.OpenTag}'");
        }

        stack.Push(new ConditionalStartParseExpression(parameter.Index));

        return true;
    }
}
