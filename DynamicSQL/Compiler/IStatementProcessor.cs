namespace DynamicSQL.Compiler;

using DynamicSQL.Parser.Expressions;

internal interface IStatementProcessor
{
    void RenderText(string text);

    void RenderInterpolationExpression(InterpolationExpression expression);

    void RenderInArrayExpression(InArrayExpression expression);

    bool ConditionValueTest(int conditionValueIndex);
}
