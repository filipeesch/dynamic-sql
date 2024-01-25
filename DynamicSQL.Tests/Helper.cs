namespace DynamicSQL.Tests;

using System;
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

            CREATE TABLE Address (
                Id INT PRIMARY KEY,
                PersonId INT,
                Name VARCHAR(200)
            );
            """;

        command.ExecuteNonQuery();

        command.CommandText = "INSERT INTO Person VALUES(@Id, @Name, @BirthDate)";
        command.Parameters.Add(new SqliteParameter("Id", SqliteType.Integer));
        command.Parameters.Add(new SqliteParameter("Name", SqliteType.Text));
        command.Parameters.Add(new SqliteParameter("BirthDate", SqliteType.Integer));

        using var addressCommand = connection.CreateCommand();

        addressCommand.CommandText = "INSERT INTO Address VALUES(@Id, @PersonId, @Name)";
        addressCommand.Parameters.Add(new SqliteParameter("Id", SqliteType.Integer));
        addressCommand.Parameters.Add(new SqliteParameter("PersonId", SqliteType.Integer));
        addressCommand.Parameters.Add(new SqliteParameter("Name", SqliteType.Text));

        for (var p = 1; p <= 100; p++)
        {
            command.Parameters["Id"].Value = p;
            command.Parameters["Name"].Value = $"Person_{p}";
            command.Parameters["BirthDate"].Value = new DateTime(1900 + p, 1, 1);
            command.ExecuteNonQuery();

            // Addresses for the first 70 items only
            if (p > 70)
            {
                continue;
            }

            for (var a = 0; a < 3; a++)
            {
                addressCommand.Parameters["Id"].Value = 10 * p + a;
                addressCommand.Parameters["PersonId"].Value = p;
                addressCommand.Parameters["Name"].Value = $"Address_{a}";
                addressCommand.ExecuteNonQuery();
            }
        }

        transaction.Commit();

        return connection;
    }
}
