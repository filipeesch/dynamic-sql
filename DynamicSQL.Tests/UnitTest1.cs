using Xunit;

namespace DynamicSQL.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var parser = new QueryParser();

        var parsed = parser.Parse(
            """
            SELECT
                p.Name
                << {0} ?, (SELECT a.Name FROM Address a WHERE a.PersonId = p.Id FOR JSON AUTO) : '' >> AS Addresses
                FROM Person p
                WHERE 1=1
                    << {1} ? AND p.BirthDate >= {4} << {5} ? Teste1 AND << {6} ? Teste2 >>>>>>
                    << {2} ? AND p.Id IN {3} >>
            """);
    }
}
