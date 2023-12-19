using System.Data.SqlClient;
using DynamicSQL;

DateOnly? birthDate = new DateOnly(1989, 3, 12);
int[] peopleIds = { 1, 2, 3, 4 };
var includeAddresses = true;

var query = new DynamicQuery(
    $"""
     SELECT
         p.Name
         << {includeAddresses} ?, (SELECT a.Name FROM Address a WHERE a.PersonId = p.Id FOR JSON AUTO) : '' >> AS Addresses
         FROM Person p
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
