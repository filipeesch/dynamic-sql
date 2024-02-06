namespace DynamicSQL.Compiler;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DynamicSQL.Parser;
using DynamicSQL.Parser.Expressions;
using ConditionalExpression = DynamicSQL.Parser.Expressions.ConditionalExpression;

internal class CodeCompiler(ParsedStatement parsed)
{
    private static readonly MethodInfo RenderTextMethod = typeof(IStatementProcessor)
        .GetMethod(nameof(IStatementProcessor.RenderText))!;

    private static readonly MethodInfo RenderInterpolationExpressionMethod = typeof(IStatementProcessor)
        .GetMethod(nameof(IStatementProcessor.RenderInterpolationExpression))!;

    private static readonly MethodInfo RenderInArrayExpressionMethod = typeof(IStatementProcessor)
        .GetMethod(nameof(IStatementProcessor.RenderInArrayExpression))!;

    private static readonly MethodInfo ConditionValueTestMethod = typeof(IStatementProcessor)
        .GetMethod(nameof(IStatementProcessor.ConditionValueTest))!;

    public Action<IStatementProcessor> Compile()
    {
        var processor = Expression.Parameter(typeof(IStatementProcessor), "processor");

        var expression = GenerateNodesCode(parsed.Expressions, processor);

        var lambda = Expression.Lambda<Action<IStatementProcessor>>(expression, processor);

        return lambda.Compile();
    }

    private Expression GenerateNodesCode(
        IEnumerable<IParsedExpression> expressions,
        Expression processor)
    {
        return Expression.Block(
            expressions.Select(
                node =>
                    node switch
                    {
                        TextExpression exp => CreateRenderTextCall(exp.Text, processor),
                        InOperatorExpression => CreateRenderTextCall("IN", processor),
                        InterpolationExpression exp => CreateRenderInterpolationExpressionCall(exp, processor),
                        InArrayExpression exp => CreateRenderInArrayExpressionCall(exp, processor),
                        ConditionalExpression exp => CreateConditionalExpressionCall(exp, processor),
                        _ => throw new InvalidOperationException()
                    }));
    }

    private Expression CreateConditionalExpressionCall(ConditionalExpression conditionalExpression, Expression processor)
    {
        var testMethodCall = Expression.Call(
            processor,
            ConditionValueTestMethod,
            Expression.Constant(conditionalExpression.ConditionValueIndex));

        var trueExpression = GenerateNodesCode(conditionalExpression.TruePartNodes, processor);

        var falseExpression = conditionalExpression.FalsePartNodes.Count == 0 ?
            null :
            GenerateNodesCode(conditionalExpression.FalsePartNodes, processor);

        return falseExpression is null ?
            Expression.IfThen(testMethodCall, trueExpression) :
            Expression.IfThenElse(testMethodCall, trueExpression, falseExpression);
    }

    private Expression CreateRenderInterpolationExpressionCall(InterpolationExpression parameter, Expression processor) =>
        Expression.Call(processor, RenderInterpolationExpressionMethod, Expression.Constant(parameter));

    private Expression CreateRenderInArrayExpressionCall(InArrayExpression inArrayExpression, Expression processor) =>
        Expression.Call(processor, RenderInArrayExpressionMethod, Expression.Constant(inArrayExpression));

    private Expression CreateRenderTextCall(string text, Expression processor) =>
        Expression.Call(processor, RenderTextMethod, Expression.Constant(text));
}
