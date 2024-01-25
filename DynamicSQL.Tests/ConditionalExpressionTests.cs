namespace DynamicSQL.Tests;

using System.Data.Common;
using System.Threading.Tasks;
using DynamicSQL.Compiler;
using Xunit;

public class ConditionalExpressionTests
{
    private readonly DbConnection _connection = Helper.CreateSchemaAndData();

    private record QueryInput(
        bool IncludeName,
        int? Id,
        bool? HasAddress);

    private static readonly Statement<QueryInput> Statement = StatementCompiler.Compile<QueryInput>(
        i => $"""
              SELECT
                Id
                << {i.IncludeName} ?,Name >>
              FROM Person p
              WHERE 1=1
                << {i.Id} ? AND Id = {i.Id} >>
                <<
                    {i.HasAddress.HasValue} ?
                    <<
                        {i.HasAddress == true}
                        ? AND EXISTS(SELECT * FROM Address WHERE PersonId = p.Id)
                        : AND NOT EXISTS(SELECT * FROM Address WHERE PersonId = p.Id)
                    >>
                >>
              """);

    [Fact]
    public async Task QueryList_NameNotIncluded_ReturnWithNameAsNull()
    {
        // Arrange
        var input = new QueryInput(false, null, null);

        // Act
        var result = await Statement.QueryListAsync<PersonRecord>(_connection, input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.Count);

        for (var i = 0; i < result.Count; i++)
        {
            var id = i + 1;
            Assert.Equal(id, result[i].Id);
            Assert.Null(result[i].Name);
        }
    }

    [Fact]
    public async Task QuerySingle_ByIdWithName_Return()
    {
        // Arrange
        var input = new QueryInput(true, 10, null);

        // Act
        var result = await Statement.QuerySingleAsync<PersonRecord>(_connection, input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(input.Id, result!.Id);
        Assert.Equal($"Person_{input.Id}", result!.Name);
    }

    [Fact]
    public async Task QueryList_WithAddress_ReturnList()
    {
        // Arrange
        var input = new QueryInput(false, null, true);

        // Act
        var result = await Statement.QueryListAsync<PersonRecord>(_connection, input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(70, result.Count);
    }

    [Fact]
    public async Task QueryList_WithoutAddress_ReturnList()
    {
        // Arrange
        var input = new QueryInput(false, null, false);

        // Act
        var result = await Statement.QueryListAsync<PersonRecord>(_connection, input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(30, result.Count);
    }
}
