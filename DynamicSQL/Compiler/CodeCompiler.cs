namespace DynamicSQL.Compiler;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DynamicSQL.Parser;
using DynamicSQL.Parser.Expressions;
using ConditionalExpression = DynamicSQL.Parser.Expressions.ConditionalExpression;
using ParameterExpression = DynamicSQL.Parser.Expressions.ParameterExpression;

internal class CodeCompiler(ParsedStatement parsed)
{
    private static readonly MethodInfo RenderTextExpressionMethod = typeof(IStatementProcessor)
        .GetMethod(nameof(IStatementProcessor.RenderTextExpression))!;

    private static readonly MethodInfo RenderParameterExpressionMethod = typeof(IStatementProcessor)
        .GetMethod(nameof(IStatementProcessor.RenderParameterExpression))!;

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
                        TextExpression textExpression => CreateTextExpressionCall(textExpression, processor),
                        ParameterExpression parameterExpression => CreateParameterExpressionCall(parameterExpression, processor),
                        InArrayExpression inArrayExpression => CreateInArrayExpressionCall(inArrayExpression, processor),
                        ConditionalExpression conditionalExpression => CreateConditionalExpressionCall(conditionalExpression, processor),
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

    private Expression CreateParameterExpressionCall(ParameterExpression parameter, Expression processor) =>
        Expression.Call(processor, RenderParameterExpressionMethod, Expression.Constant(parameter));

    private Expression CreateInArrayExpressionCall(InArrayExpression inArrayExpression, Expression processor) =>
        Expression.Call(processor, RenderInArrayExpressionMethod, Expression.Constant(inArrayExpression));

    private Expression CreateTextExpressionCall(TextExpression textExpression, Expression processor) =>
        Expression.Call(processor, RenderTextExpressionMethod, Expression.Constant(textExpression));
}
