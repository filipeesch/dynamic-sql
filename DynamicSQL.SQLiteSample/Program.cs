using DynamicSQL.Compiler;
using Microsoft.Data.Sqlite;

var statement = StatementCompiler.Compile<QueryInput>(
    i =>
        $"""
         SELECT
            {i.Id} AS Id,
            {i.Name} AS Name,
            {i.Age} AS Age
         """);

var input = new QueryInput(Guid.NewGuid(), "Name_Value", 39);

await using var connection = new SqliteConnection("Data Source=:memory:");

await foreach (var item in statement.QueryAsyncEnumerable<QueryResult>(connection, input))
{
    Console.WriteLine($"{item.Id}\t\t{item.Name}\t\t{item.Age}");
}

Console.ReadLine();

public record QueryInput(Guid Id, string Name, int Age);

public record QueryResult(Guid Id, string Name, int Age);
