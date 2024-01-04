namespace DynamicSQL.Parser;

using System.Collections.Generic;

public class ConditionalNode : IParsedSqlNode
{
    public ConditionalNode(
        int conditionValueIndex,
        IReadOnlyCollection<IParsedSqlNode> truePartNodes,
        IReadOnlyCollection<IParsedSqlNode> falsePartNodes)
    {
        ConditionValueIndex = conditionValueIndex;
        TruePartNodes = truePartNodes;
        FalsePartNodes = falsePartNodes;
    }

    public int ConditionValueIndex { get; }

    public IReadOnlyCollection<IParsedSqlNode> TruePartNodes { get; }

    public IReadOnlyCollection<IParsedSqlNode> FalsePartNodes { get; }
}
