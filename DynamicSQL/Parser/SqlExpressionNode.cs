namespace DynamicSQL.Parser;

public class SqlExpressionNode(string expression) : IParsedStatementNode
{
    public string Expression { get; } = expression;
}
