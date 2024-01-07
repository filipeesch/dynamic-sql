namespace DynamicSQL.Benchmark;

using BenchmarkDotNet.Attributes;
using DynamicSQL.Compiler;
using Microsoft.Data.SqlClient;

[SimpleJob]
[MemoryDiagnoser]
public class BenchmarkClass
{
    QueryInput input = new QueryInput(
        new DateOnly(1989, 3, 12),
        new[] { 1, 2, 3, 4 },
        true);

    CompiledStatement<QueryInput> compiledStatement = StatementCompiler.Compile<QueryInput>(
        i =>
            $"""
             SELECT
                 p.Name
                 << {i.IncludeAddresses} ?, (SELECT a.Name FROM Address a WHERE a.PersonId = p.Id FOR JSON AUTO) : '' >> AS Addresses
                 FROM Person p
                 WHERE 1=1
                     << {i.BirthDate} ? AND p.BirthDate = {i.BirthDate} <<  >>>>
                     << {i.PeopleIds} ? AND p.Id IN {i.PeopleIds} >>
             """);

    [Benchmark]
    public void InterpreterImp()
    {
        var query = new DynamicQuery(
            $"""
             SELECT
                 p.Name
                 << {input.IncludeAddresses} ?, (SELECT a.Name FROM Address a WHERE a.PersonId = p.Id FOR JSON AUTO) : '' >> AS Addresses
                 FROM Person p
                 WHERE 1=1
                     << {input.BirthDate} ? AND p.BirthDate = {input.BirthDate} <<  >>>>
                     << {input.PeopleIds} ? AND p.Id IN {input.PeopleIds} >>
             """);

        var command = new SqlCommand();
        query.RenderOn(command);
    }

    [Benchmark]
    public void CompiledImp()
    {
        var command = new SqlCommand();
        compiledStatement.Render(input, command);
    }

    record QueryInput(DateOnly BirthDate, int[] PeopleIds, bool IncludeAddresses);
}
