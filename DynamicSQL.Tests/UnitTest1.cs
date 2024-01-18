using System;
using DynamicSQL.Compiler;
using DynamicSQL.Parser;
using Microsoft.Data.SqlClient;
using Xunit;

namespace DynamicSQL.Tests;

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
            FROM Person p
            WHERE
                <<
                   {2}
                   ? AND p.BirthDate >= {3}
                       <<
                           {4}
                           ? Test1_True
                               <<
                                   {5}
                                   ? Test11_True
                                   : Test11_False
                               >>
                           : Test1_False
                               <<
                                   {6}
                                   ? Test2_True
                                   : Test2_False
                               >>
                       >>
                >>
                << {7} ? AND p.Id = {8} >>
                AND p.ParentId IN {9}
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
                        INNER JOIN Test t
                     WHERE
                         p.Id = {input.Day}
                         <<
                            {input.Month}
                            ? AND p.BirthDate >= {4}
                                <<
                                    {true}
                                    ? Test1_True
                                        <<
                                            {false}
                                            ? Test11_True
                                            : Test11_False
                                        >>
                                    : Test1_False
                                        <<
                                            {false}
                                            ? Test2_True
                                            : Test2_False
                                        >>
                                >>
                         >>
                         << {2} ? AND p.Id = {3} >>
                         AND p.ParentId IN {new[] { input.Year, input.Month, input.Day }}
                 """);

        var command = new SqlCommand();

        statement.Render(DateTime.Now, command);
    }
}
