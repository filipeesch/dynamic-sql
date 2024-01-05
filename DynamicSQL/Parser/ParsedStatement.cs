namespace DynamicSQL.Parser;

using System.Collections.Generic;

public class ParsedStatement(IReadOnlyCollection<IParsedStatementNode> nodes)
{
    public IReadOnlyCollection<IParsedStatementNode> Nodes { get; } = nodes;
}
