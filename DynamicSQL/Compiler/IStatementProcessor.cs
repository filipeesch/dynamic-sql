namespace DynamicSQL.Compiler;

using DynamicSQL.Parser;

public interface IStatementProcessor
{
    void RenderCodeNode(SqlExpressionNode node);

    void RenderParameterNode(ParameterNode node);

    bool ConditionValueTest(int conditionValueIndex);
}
