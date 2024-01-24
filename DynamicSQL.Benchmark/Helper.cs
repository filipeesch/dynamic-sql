namespace DynamicSQL.Benchmark;

using System.Data.Common;
using Microsoft.Data.Sqlite;

public static class Helper
{
    public static DbConnection CreateSchemaAndData()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var command = connection.CreateCommand();

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

        return connection;
    }
}
