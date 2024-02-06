using DynamicSQL.Compiler;
using DynamicSQL.Sample;

var input = new DynamicQueryInput(
    true,
    true,
    new[] { 10, 11, 12, 13, 14, 16, 17, 18 },
    100);

// This object should be static or singleton in real applications
var query = StatementCompiler.Compile<DynamicQueryInput>(
    (i,c)=> $"""
          SELECT
            Id
            << {i.IncludeName} ?, Name >>
            << {i.IncludeBirthDate} ?, BirthDate >>
          FROM Person
          WHERE 1=1
            << {i.Ids} ? AND Id IN {i.Ids} >>
            << {i.Count} ? LIMIT {i.Count} >>
          """);

var connection = Helper.CreateSchemaAndData();

Console.WriteLine("Id\t\t|\t\tName\t\t|\t\tBirth Date");

await foreach (var item in query.QueryAsyncEnumerable<QueryResult>(connection, input))
{
    Console.WriteLine($"{item.Id}\t\t|\t\t{item.Name}\t\t|\t\t{item.BirthDate}");
}
