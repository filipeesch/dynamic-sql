using DynamicSQL.Compiler;
using Microsoft.Data.SqlClient;

var statement = StatementCompiler.Compile<QueryInput>(
    i =>
        $"""
         SELECT
             p.Name,
             << {i.IncludeAddresses} ? (SELECT a.Name FROM Address a WHERE a.PersonId = p.Id FOR JSON AUTO) : '' >> AS Addresses
             FROM Person p
             WHERE 1=1
                 << {i.BirthDate} ? AND p.BirthDate = {i.BirthDate} >>
                 << {i.PeopleIds} ? AND p.Id IN {i.PeopleIds} >>
         """);

var input = new QueryInput(
    new DateOnly(1989, 3, 12),
    new[] { 1, 2, 3, 4 },
    true);

var command = new SqlCommand();

for (int i = 0; i < 10000; i++)
{
    statement.Render(input, command);
    command.Parameters.Clear();
}

public record QueryInput(DateOnly? BirthDate, IEnumerable<int> PeopleIds, bool IncludeAddresses);
