## Introduction
SQL is a powerful and flexible language for querying and updating data. However, generating dynamic SQL can be cumbersome, involving complex logic, string concatenation, and creating `DbParameter` instances. While there are libraries to facilitate SQL creation using C#, they often fall short in handling complex queries, leading to convoluted C# code. This project introduces a new approach to simplify and maintain dynamic SQL generation.

## Usage
The library leverages `<< >>` tags combined with string interpolation and custom operators to construct the final SQL command. The interpolated string is interpreted and compiled to native code, to provide the best performance in runtime.

## Key Features

- **Simplified SQL Generation**: Streamlines the creation of complex SQL queries using minimal code, which helps reduce the likelihood of errors and improves code maintainability.

- **Conditional Rendering**: Employs intuitive syntax (`<<condition ? truePart : falsePart>>`) for conditional inclusion of SQL segments. This allows queries to dynamically adapt based on runtime conditions.

- **Automatic Parameters**: Automatically converts string interpolation placeholders into SQL parameters, mitigating SQL injection risks. This feature is particularly useful for list types with the `IN` SQL operator, as it transforms them into multiple parameters seamlessly.

- **Nested Conditions**: Facilitates the construction of sophisticated SQL queries with support for nested conditional statements, enabling complex logic to be expressed cleanly and concisely within SQL strings.

- **Segment Rendering**: Provides the ability to dynamically render specific SQL segments with custom logic, offering flexibility in query construction. This allows for the injection of custom SQL snippets and parameters into queries based on application requirements.

- **Performance Optimized**: The statements and parsers are compiled into native code, which is then cached for future executions. No reflection is used in the hot path. This ensures high performance for query executions.

- **Type-Safe Parsing**: Parses query results directly into .NET classes or records

## Documentation
See the [documentation page](https://filipeesch.github.io/dynamic-sql/) for more details.

## Sample Usage
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
