namespace DynamicSQL.Compiler;

using DynamicSQL.Parser.Expressions;

internal interface IStatementProcessor
{
    void RenderTextExpression(TextExpression expression);

    void RenderParameterExpression(ParameterExpression expression);

    void RenderInArrayExpression(InArrayExpression expression);

    bool ConditionValueTest(int conditionValueIndex);
}
