using System;
using DynamicSQL.Compiler;
using Xunit;
using System.Data.Common;
using System.Threading.Tasks;

namespace DynamicSQL.Tests;

public class QuerySingleTests
{
    private readonly DbConnection _connection = Helper.CreateSchemaAndData();

    [Fact]
    public async Task ExistingId_ReturnPersonRecord()
    {
        // Arrange
        var statement = StatementCompiler.Compile<int>((i, c) => $"SELECT Id, Name, BirthDate FROM Person WHERE Id = {i}");
        const int id = 10;

        // Act
        var result = await statement.QuerySingleAsync<PersonRecord>(_connection, id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
        Assert.Equal("Person_10", result!.Name);
        Assert.Equal(new DateTime(1910, 1, 1), result!.BirthDate);
    }

    [Fact]
    public async Task ExistingId_ReturnPersonClass()
    {
        // Arrange
        var statement = StatementCompiler.Compile<int>((i, c) => $"SELECT Id, Name, BirthDate FROM Person WHERE Id = {i}");
        const int id = 10;

        // Act
        var result = await statement.QuerySingleAsync<PersonClass>(_connection, id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
        Assert.Equal("Person_10", result!.Name);
        Assert.Equal(new DateTime(1910, 1, 1), result!.BirthDate);
    }

    [Fact]
    public async Task NonExistingId_ReturnNull()
    {
        // Arrange
        var statement = StatementCompiler.Compile<int>((i, c) => $"SELECT Id, Name, BirthDate FROM Person WHERE Id = {i}");
        const int id = -10;

        // Act
        var result = await statement.QuerySingleAsync<PersonRecord>(_connection, id);

        // Assert
        Assert.Null(result);
    }
}
