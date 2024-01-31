namespace DynamicSQL.Compiler;

using DynamicSQL.Parser.Expressions;

internal interface IStatementProcessor
{
    void RenderText(string text);

    void RenderParameterExpression(ParameterExpression expression);

    void RenderInArrayExpression(InArrayExpression expression);

    bool ConditionValueTest(int conditionValueIndex);
}
