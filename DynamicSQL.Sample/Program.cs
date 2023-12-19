using System.Data.SqlClient;
using DynamicSQL;

DateOnly? birthDate = new DateOnly(1989, 3, 12);
int[] peopleIds = { 1, 2, 3, 4 };
var includeAddresses = false;

var query = new DynamicQuery(
    $"""
     SELECT
         p.Name,
         << {includeAddresses} ? a.Name : '' >> AS AddressName
         FROM Person p
             << {includeAddresses} ? LEFT JOIN Address a ON a.PersonId = p.Id >>
         WHERE 1=1
             << {birthDate} ? AND p.BirthDate = {birthDate} >>
             << {peopleIds} ? AND p.Id IN {peopleIds} >>
     """);

var command = new SqlCommand();
query.RenderOn(command);

Console.WriteLine("Parameters: ");

foreach (SqlParameter parameter in command.Parameters)
{
    Console.WriteLine($" {parameter.ParameterName}: {parameter.Value}");
}

Console.WriteLine("\nSQL: ");
Console.WriteLine(command.CommandText);

Console.ReadLine();
