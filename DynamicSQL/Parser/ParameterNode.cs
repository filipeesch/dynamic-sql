namespace DynamicSQL.Parser;

public class ParameterNode(string op, int parameterValueIndex) : IParsedStatementNode
{
    public string Operator { get; } = op;

    public int ParameterValueIndex { get; } = parameterValueIndex;
}
