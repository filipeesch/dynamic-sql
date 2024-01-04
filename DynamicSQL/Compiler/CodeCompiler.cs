namespace DynamicSQL.Compiler;

using System;
using DynamicSQL.Parser;

public class CodeCompiler<TInput>
{
    private readonly ParsedSql parsedSql;

    public CodeCompiler(ParsedSql parsedSql)
    {
        this.parsedSql = parsedSql;
    }

    public CompiledSql<TInput> Compile()
    {
        throw new NotImplementedException();
    }
}
