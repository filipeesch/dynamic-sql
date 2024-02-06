using DynamicSQL.Compiler;
using DynamicSQL.PerformanceTests;
using Microsoft.Data.Sqlite;

var input = new DynamicQueryInput(
    true,
    true,
    new[] { 10, 11, 12, 13, 14, 16, 17, 18 },
    1000);

var DynamicQuery = StatementCompiler.Compile<DynamicQueryInput>(
    (i, c) => $"""
               SELECT
                 Id
                 << {i.IncludeName} ?, Name >>
                 << {i.IncludeBirthDate} ?, BirthDate >>
               FROM Person
               WHERE 1=1
                 << {i.Ids} ? AND Id IN {i.Ids} >>
                 << {i.Count} ? LIMIT {i.Count} >>
               """);

var connection = new SqliteConnection("Data Source=:memory:");

using var command = connection.CreateCommand();

connection.Open();
using var transaction = connection.BeginTransaction();

command.Transaction = transaction;
command.CommandText =
    """
    CREATE TABLE Person (
        Id INT PRIMARY KEY,
        Name VARCHAR(200),
        BirthDate DATE
    );
    """;

command.ExecuteNonQuery();

command.CommandText = "INSERT INTO Person VALUES(@Id, @Name, @BirthDate)";
command.Parameters.Add(new SqliteParameter("Id", SqliteType.Integer));
command.Parameters.Add(new SqliteParameter("Name", SqliteType.Text));
command.Parameters.Add(new SqliteParameter("BirthDate", SqliteType.Integer));

for (var i = 1; i <= 100_000; i++)
{
    command.Parameters["Id"].Value = i;
    command.Parameters["Name"].Value = $"Name_{i}";
    command.Parameters["BirthDate"].Value = DateTime.UtcNow.AddDays(-Random.Shared.Next(365, 365 * 100));
    command.ExecuteNonQuery();
}

transaction.Commit();

var result = await DynamicQuery.QueryListAsync<QueryResult>(connection, input, predictedListSize: input.Count);

namespace DynamicSQL.PerformanceTests
{
    public record DynamicQueryInput(
        bool IncludeName,
        bool IncludeBirthDate,
        IEnumerable<int>? Ids,
        int? Count);


    public class QueryResult
    {
        public int Id { get; init; }

        public string Name { get; init; }

        public DateTime BirtDate { get; init; }
    }
}
