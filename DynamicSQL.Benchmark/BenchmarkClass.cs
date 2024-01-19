namespace DynamicSQL.Benchmark;

using BenchmarkDotNet.Attributes;
using Dapper;
using DynamicSQL.Compiler;
using Microsoft.Data.Sqlite;

[SimpleJob]
[MemoryDiagnoser]
public class BenchmarkClass
{
    private static readonly CompiledStatement<QueryInput> Statement = StatementCompiler.Compile<QueryInput>(
        i =>
            $"""
             WITH RECURSIVE RandomData AS (
                 SELECT RANDOM() % 100 as Id
                 UNION ALL
                 SELECT RANDOM() % 100
                 FROM RandomData
                 LIMIT 10
             )
             SELECT Id, {i.Name} AS Name, {i.Age} AS Age FROM RandomData;
             """);

    public record QueryInput(string Name, int Age);

    public class QueryResult
    {
        public int Id { get; init; }

        public string Name { get; init; }

        public int Age { get; init; }
    }

    [Benchmark]
    public async Task DynamicSQL()
    {
        var input = new QueryInput("Name_Value", 39);

        await using var connection = new SqliteConnection("Data Source=:memory:");

        var result = await Statement.QueryListAsync<QueryResult>(connection, input);
    }

    [Benchmark]
    public async Task Dapper()
    {
        var input = new QueryInput("Name_Value", 39);

        await using var connection = new SqliteConnection("Data Source=:memory:");

        var result = await connection.QueryAsync<QueryResult>(
            """
            WITH RECURSIVE RandomData AS (
                 SELECT RANDOM() % 100 as Id
                 UNION ALL
                 SELECT RANDOM() % 100
                 FROM RandomData
                 LIMIT 10
             )
             SELECT Id, @Name AS Name, @Age AS Age FROM RandomData;
            """,
            input);
    }
}
