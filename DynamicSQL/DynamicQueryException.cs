using System;

namespace DynamicSQL;

public class DynamicQueryException(string message, string queryPart) : Exception(message)
{
    public string QueryPart { get; } = queryPart;
}
