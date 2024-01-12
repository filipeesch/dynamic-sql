namespace DynamicSQL.Compiler;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DynamicSQL.Parser;

public class CodeCompiler(ParsedStatement parsed)
{
    private static readonly MethodInfo RenderCodeNodeMethod = typeof(IStatementProcessor)
        .GetMethod(nameof(IStatementProcessor.RenderCodeNode))!;

    private static readonly MethodInfo RenderParameterNodeMethod = typeof(IStatementProcessor)
        .GetMethod(nameof(IStatementProcessor.RenderParameterNode))!;

    private static readonly MethodInfo ConditionValueTestMethod = typeof(IStatementProcessor)
        .GetMethod(nameof(IStatementProcessor.ConditionValueTest))!;

    public Action<IStatementProcessor> Compile()
    {
        var processor = Expression.Parameter(typeof(IStatementProcessor), "processor");

        var expression = GenerateNodesCode(parsed.Nodes, processor);

        var lambda = Expression.Lambda<Action<IStatementProcessor>>(expression, processor);

        return lambda.Compile();
    }

    private Expression GenerateNodesCode(
        IEnumerable<IParsedStatementNode> nodes,
        Expression processor)
    {
        return Expression.Block(
            nodes.Select(
                node =>
                    node switch
                    {
                        SqlExpressionNode codeNode => GenerateCodeNode(codeNode, processor),
                        ParameterNode parameterNode =>
                            GenerateParameterNode(parameterNode, processor),
                        ConditionalNode conditionalNode =>
                            GenerateConditionalNode(conditionalNode, processor),
                        _ => throw new InvalidOperationException()
                    }));
    }

    private Expression GenerateConditionalNode(ConditionalNode conditionalNode, Expression processor)
    {
        var testMethodCall = Expression.Call(
            processor,
            ConditionValueTestMethod,
            Expression.Constant(conditionalNode.ConditionValueIndex));

        var trueExpression = GenerateNodesCode(conditionalNode.TruePartNodes, processor);

        var falseExpression = conditionalNode.FalsePartNodes.Count == 0 ?
            null :
            GenerateNodesCode(conditionalNode.FalsePartNodes, processor);

        return falseExpression is null ?
            Expression.IfThen(testMethodCall, trueExpression) :
            Expression.IfThenElse(testMethodCall, trueExpression, falseExpression);
    }

    private Expression GenerateParameterNode(ParameterNode parameterNode, Expression processor) =>
        Expression.Call(processor, RenderParameterNodeMethod, Expression.Constant(parameterNode));

    private Expression GenerateCodeNode(SqlExpressionNode codeNode, Expression processor) =>
        Expression.Call(processor, RenderCodeNodeMethod, Expression.Constant(codeNode));
}
