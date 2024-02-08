namespace DynamicSQL.Benchmark;

using System.Data.Common;
using System.Text;
using BenchmarkDotNet.Attributes;
using Dapper;
using DynamicSQL.Compiler;

//[ShortRunJob]
[MemoryDiagnoser]
public class BenchmarkClass
{
    private static readonly Statement<int> SelectAll = StatementCompiler.Compile<int>(
        (i, c) => $"SELECT Id, Name, BirthDate FROM Person");

    private static readonly Statement<SelectAllQueryInput> SelectAllWithLimit = StatementCompiler.Compile<SelectAllQueryInput>(
        (i, c) => $"SELECT Id, Name, BirthDate FROM Person LIMIT {i.Count}");

    private static readonly Statement<QuerySingleInput> QuerySingle = StatementCompiler.Compile<QuerySingleInput>(
        (i, c) => $"SELECT Id, Name, BirthDate FROM Person WHERE Id = {i.Id}");

    private static readonly Statement<DynamicQueryInput> DynamicQuery = StatementCompiler.Compile<DynamicQueryInput>(
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

    private readonly DbConnection _connection = Helper.CreateSchemaAndData();


    [Benchmark]
    public async Task DynamicSQL_QueryList_1_Item()
    {
        var input = new SelectAllQueryInput(1);

        var result = await SelectAllWithLimit.QueryListAsync<QueryResult>(_connection, input);
    }

    [Benchmark]
    public async Task DynamicSQL_QueryList_1_Item_Predicted()
    {
        var input = new SelectAllQueryInput(1);

        var result = await SelectAllWithLimit.QueryListAsync<QueryResult>(_connection, input, predictedListSize: input.Count);
    }

    [Benchmark]
    public async Task Dapper_QueryList_1_Item()
    {
        var input = new SelectAllQueryInput(1);

        var result = await _connection.QueryAsync<QueryResult>(
            "SELECT Id, Name, BirthDate FROM Person LIMIT @Count",
            input);
    }

    [Benchmark]
    public async Task DynamicSQL_QueryList_10_Items()
    {
        var input = new SelectAllQueryInput(10);

        var result = await SelectAllWithLimit.QueryListAsync<QueryResult>(_connection, input);
    }

    [Benchmark]
    public async Task DynamicSQL_QueryList_10_Items_Predicted()
    {
        var input = new SelectAllQueryInput(10);

        var result = await SelectAllWithLimit.QueryListAsync<QueryResult>(_connection, input, predictedListSize: input.Count);
    }

    [Benchmark]
    public async Task Dapper_QueryList_10_Items()
    {
        var input = new SelectAllQueryInput(10);

        var result = await _connection.QueryAsync<QueryResult>(
            "SELECT Id, Name, BirthDate FROM Person LIMIT @Count",
            input);
    }

    [Benchmark]
    public async Task DynamicSQL_QueryList_100_Items()
    {
        var input = new SelectAllQueryInput(100);

        var result = await SelectAllWithLimit.QueryListAsync<QueryResult>(_connection, input);
    }

    [Benchmark]
    public async Task DynamicSQL_QueryList_100_Items_Predicted()
    {
        var input = new SelectAllQueryInput(100);

        var result = await SelectAllWithLimit.QueryListAsync<QueryResult>(_connection, input, predictedListSize: input.Count);
    }

    [Benchmark]
    public async Task Dapper_QueryList_100_Items()
    {
        var input = new SelectAllQueryInput(100);

        var result = await _connection.QueryAsync<QueryResult>(
            "SELECT Id, Name, BirthDate FROM Person LIMIT @Count",
            input);
    }

    [Benchmark]
    public async Task DynamicSQL_QueryList_1000_Items()
    {
        var input = new SelectAllQueryInput(1000);

        var result = await SelectAllWithLimit.QueryListAsync<QueryResult>(_connection, input);
    }

    [Benchmark]
    public async Task DynamicSQL_QueryList_1000_Items_Predicted()
    {
        var input = new SelectAllQueryInput(1000);

        var result = await SelectAllWithLimit.QueryListAsync<QueryResult>(_connection, input, predictedListSize: input.Count);
    }

    [Benchmark]
    public async Task Dapper_QueryList_1000_Items()
    {
        var input = new SelectAllQueryInput(1000);

        var result = await _connection.QueryAsync<QueryResult>(
            "SELECT Id, Name, BirthDate FROM Person LIMIT @Count",
            input);
    }

    [Benchmark]
    public async Task DynamicSQL_QuerySingle()
    {
        var input = new QuerySingleInput(16);

        var result = await QuerySingle.QuerySingleAsync<QueryResult>(_connection, input);
    }

    [Benchmark]
    public async Task Dapper_QuerySingle()
    {
        var input = new QuerySingleInput(16);

        var result = await _connection.QuerySingleAsync<QueryResult>(
            "SELECT Id, Name, BirthDate FROM Person WHERE Id = @Id",
            input);
    }

    [Benchmark]
    public async Task DynamicSQL_DynamicQuery()
    {
        var input = new DynamicQueryInput(
            true,
            true,
            new[] { 10, 11, 12, 13, 14, 16, 17, 18 },
            1000);

        var result = await DynamicQuery.QueryListAsync<QueryResult>(_connection, input);
    }

    [Benchmark]
    public async Task Dapper_DynamicQuery()
    {
        var query = new StringBuilder("SELECT Id");
        var parameters = new DynamicParameters();

        var i = new DynamicQueryInput(
            true,
            true,
            new[] { 10, 11, 12, 13, 14, 16, 17, 18 },
            1000);

        if (i.IncludeName)
            query.Append(", Name");

        if (i.IncludeBirthDate)
            query.Append(", BirthDate");

        query.Append(" FROM Person WHERE 1=1 ");

        if (i.Ids != null && i.Ids.Any())
        {
            query.Append(" AND Id IN @Ids ");
            parameters.Add("Ids", i.Ids);
        }

        if (i.Count.HasValue)
        {
            parameters.Add("Count", i.Count);
            query.Append(" LIMIT @Count");
        }

        var result = await _connection.QueryAsync<QueryResult>(query.ToString(), parameters);
    }
}
