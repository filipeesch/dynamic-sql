namespace DynamicSQL.Parser;

public class CodeNode(string code) : IParsedStatementNode
{
    public string Code { get; } = code;
}
