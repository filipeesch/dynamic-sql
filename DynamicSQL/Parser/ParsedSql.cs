namespace DynamicSQL.Parser;

using System.Collections.Generic;

public class ParsedSql
{
    public ParsedSql(IReadOnlyCollection<IParsedSqlNode> nodes)
    {
        Nodes = nodes;
    }

    public IReadOnlyCollection<IParsedSqlNode> Nodes { get; }
}
