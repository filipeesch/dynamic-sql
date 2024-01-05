namespace DynamicSQL.Parser;

using System.Collections.Generic;

public class ConditionalNode : IParsedStatementNode
{
    public ConditionalNode(
        int conditionValueIndex,
        IReadOnlyCollection<IParsedStatementNode> truePartNodes,
        IReadOnlyCollection<IParsedStatementNode> falsePartNodes)
    {
        ConditionValueIndex = conditionValueIndex;
        TruePartNodes = truePartNodes;
        FalsePartNodes = falsePartNodes;
    }

    public int ConditionValueIndex { get; }

    public IReadOnlyCollection<IParsedStatementNode> TruePartNodes { get; }

    public IReadOnlyCollection<IParsedStatementNode> FalsePartNodes { get; }
}
