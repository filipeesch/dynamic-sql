namespace DynamicSQL.Tests;

using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using DynamicSQL.Compiler;
using Xunit;

public class QueryListTests
{
    private readonly DbConnection _connection = Helper.CreateSchemaAndData();

    private record IdRangeInput(int StartId, int EndId);

    [Fact]
    public async Task IdRange_ReturnList()
    {
        // Arrange
        var statement = StatementCompiler.Compile<IdRangeInput>(
            i => $"SELECT Id, Name FROM Person WHERE Id >= {i.StartId} AND Id <= {i.EndId}");
        var input = new IdRangeInput(30, 39);

        // Act
        var result = await statement.QueryListAsync<PersonRecord>(_connection, input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Count);

        for (var i = 0; i <= 9; i++)
        {
            var id = i + 30;
            Assert.Equal(id, result[i].Id);
            Assert.Equal($"Person_{id}", result[i].Name);
        }
    }

    [Fact]
    public async Task InOperator_ReturnList()
    {
        // Arrange
        var statement = StatementCompiler.Compile<IEnumerable<int>>(
            i => $"SELECT Id, Name FROM Person WHERE Id IN {i}");
        var input = new[] { 30, 31, 32, 33, 34 };

        // Act
        var result = await statement.QueryListAsync<PersonRecord>(_connection, input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);

        for (var i = 0; i <= 4; i++)
        {
            var id = i + 30;
            Assert.Equal(id, result[i].Id);
            Assert.Equal($"Person_{id}", result[i].Name);
        }
    }
}
