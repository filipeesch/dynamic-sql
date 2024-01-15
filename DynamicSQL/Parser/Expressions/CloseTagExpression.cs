namespace DynamicSQL.Parser.Expressions;

using System;
using System.Collections.Generic;
using System.Linq;

internal class CloseTagExpression(int startIndex, int endIndex) : TokenExpression(startIndex, endIndex)
{
    public override bool TryReduce(ExpressionStack stack)
    {
        var expressions = new List<IParsedExpression>();

        if (stack.Pop() is not CloseTagExpression)
        {
            throw new Exception($"Scope must end with '{TokenSymbols.CloseTag}'");
        }

        while (true)
        {
            var exp = stack.Pop();

            if (exp is OpenTagExpression)
            {
                break;
            }

            expressions.Add(exp);
        }

        if (expressions.Count == 0)
        {
            return false;
        }

        expressions.Reverse();

        var firstExpression = expressions.First();

        if (firstExpression is ConditionalStartParseExpression)
        {
            stack.Push(ConditionalExpression.Reduce(expressions));
            return true;
        }

        throw new Exception("Only conditional scopes are supported");
    }
}
