namespace DynamicSQL.Compiler;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using DynamicSQL.Parser;

public class CodeCompiler(ParsedStatement parsed)
{
    private static readonly MethodInfo RenderCodeNodeMethod = typeof(StatementRenderer)
        .GetMethod(nameof(StatementRenderer.RenderCodeNode))!;

    private static readonly MethodInfo RenderParameterNodeMethod = typeof(StatementRenderer)
        .GetMethod(nameof(StatementRenderer.RenderParameterNode))!;

    public Action<StatementRenderer> Compile()
    {
        var expressions = new List<Expression>();

        var renderer = Expression.Parameter(typeof(StatementRenderer), "renderer");

        GenerateNodesCode(parsed.Nodes, expressions, renderer);

        var lambda = Expression.Lambda<Action<StatementRenderer>>(Expression.Block(expressions), renderer);

        return lambda.Compile();
    }

    private void GenerateNodesCode(
        IReadOnlyCollection<IParsedStatementNode> nodes,
        ICollection<Expression> expressions,
        ParameterExpression renderer)
    {
        foreach (var node in nodes)
        {
            expressions.Add(
                node switch
                {
                    CodeNode codeNode => GenerateCodeNode(codeNode, renderer),
                    ParameterNode parameterNode =>
                        GenerateParameterNode(parameterNode, renderer),
                    ConditionalNode conditionalNode =>
                        GenerateConditionalNode(conditionalNode, renderer),
                    _ => throw new InvalidOperationException()
                });
        }
    }

    private Expression GenerateConditionalNode(ConditionalNode conditionalNode, Expression renderer)
    {
        //var @if = Expression.IfThen()
        return Expression.Empty();
    }

    private Expression GenerateParameterNode(ParameterNode parameterNode, Expression renderer) =>
        Expression.Call(renderer, RenderParameterNodeMethod, Expression.Constant(parameterNode));

    private Expression GenerateCodeNode(CodeNode codeNode, Expression renderer) =>
        Expression.Call(renderer, RenderCodeNodeMethod, Expression.Constant(codeNode));
}
