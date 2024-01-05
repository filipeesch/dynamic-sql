using Xunit;

namespace DynamicSQL.Tests;

using System;
using System.Data.SqlClient;
using DynamicSQL.Compiler;
using DynamicSQL.Parser;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var parser = new StatementParser();

        var parsed = parser.Parse(
            """
            SELECT
                p.Name
                << {0} ?, (SELECT a.Name FROM Address a WHERE a.PersonId = p.Id FOR JSON AUTO) : '' >> AS Addresses
                FROM Person p
                WHERE 1=1
                    << {1} ? AND p.BirthDate >= {4} << {5} ? Test1 AND << {6} ? Test2 >>>>>>
                    << {2} ? AND p.Id IN {3} >>
            """);
    }

    [Fact]
    public void Test2()
    {
        var statement = StatementCompiler.Compile<DateTime>(
            input =>
                $"""
                 SELECT
                     p.Name
                     << {input.Day} ?, (SELECT a.Name FROM Address a WHERE a.PersonId = p.Id FOR JSON AUTO) : '' >> AS Addresses
                     FROM Person p
                     WHERE
                         p.Id = {input.Day}
                         << {input.Month} ? AND p.BirthDate >= {4} << {5} ? Test1 AND << {6} ? Test2 >>>>>>
                         << {2} ? AND p.Id IN {3} >>
                         AND p.ParentId IN {new[] { input.Year, input.Month, input.Day }}
                 """);

        var command = new SqlCommand();

        statement.Render(DateTime.Now, command);
    }
}
