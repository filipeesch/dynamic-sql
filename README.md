## Introduction
SQL is a powerful and flexible language for querying and updating data. However, generating dynamic SQL can be cumbersome, involving complex logic, string concatenation, and creating `DbParameter` instances. While there are libraries to facilitate SQL creation using C#, they often fall short in handling complex queries, leading to convoluted C# code. This project introduces a new approach to simplify, empower, and maintain dynamic SQL generation.

## Usage
The library leverages `<< >>` tags combined with string interpolation and custom operators to construct the final SQL string. The interpolated string is interpreted and compiled to native code, to provide the best performance in runtime.

### Conditional Rendering
The syntax `<<condition ? truePart : falsePart>>` acts as an IF statement for conditional rendering. The false part is optional. Conditions can be boolean, nullable, or list types, with 'true' interpreted as a boolean true, a non-null nullable type, or a non-empty list. Nested conditions are supported.

### Automatic Parameters
String interpolation placeholders are automatically converted into SQL parameters, avoiding any kind of SQL injection. Lists used with the `IN` SQL operator are transformed into multiple SQL parameters.

#### Example:
```csharp
private static readonly CompiledStatement<QueryInput> Statement = StatementCompiler.Compile<QueryInput>(
    i =>
        $"""
         SELECT
             p.Name
             << {i.IncludeAddresses} ?, (SELECT a.Name FROM Address a WHERE a.PersonId = p.Id FOR JSON AUTO) : '' >> AS Addresses
             FROM Person p
             WHERE 1=1
                 << {i.BirthDate} ? AND p.BirthDate = {i.BirthDate} >>
                 << {i.PeopleIds} ? AND p.Id IN {i.PeopleIds} >>
         """);

public record QueryResult(string Name, string Addresses);

var input = new QueryInput(
    null,
    new[] { 1, 2, 3, 4 },
    true);

await using var connection = new SqlConnection(...);

var result = await Statement.QueryListAsync<QueryResult>(connection, input);
```

#### Resulting SQL Statement:
```sql
SELECT
    p.Name
    , (SELECT a.Name FROM Address a WHERE a.PersonId = p.Id FOR JSON AUTO) AS Addresses
FROM Person p
WHERE 1=1
    AND p.Id IN (@p4_0, @p4_1, @p4_2, @p4_3)
```

## Future Enhancements
- Loop rendering support
- Better syntax error messages 
- Adding support for table-valued parameters.
- Extending compatibility with different SQL databases like Postgres, MySQL, and Oracle.
