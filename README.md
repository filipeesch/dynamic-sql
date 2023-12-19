## Introduction
SQL is a powerful and flexible language for querying and updating data. However, generating dynamic SQL can be cumbersome, involving complex logic, string concatenation, and creating `DbParameter` instances. While there are libraries to facilitate SQL creation using C#, they often fall short in handling complex queries, leading to convoluted C# code. This project introduces a new approach to simplify, empower, and maintain dynamic SQL generation.

## Quick Start
The library leverages `<< >>` tags combined with string interpolation and custom operators to construct the final SQL string. Here's a quick guide on how to use it:

### Ternary Operator
The syntax `<<condition ? truePart : falsePart>>` acts as an IF statement for conditional rendering. The false part is optional. Conditions can be boolean, nullable, or list types, with 'true' interpreted as a boolean true, a non-null nullable type, or a non-empty list.

### Automatic Parameters
String interpolation placeholders are automatically converted into SQL parameters. Lists used with the `IN` SQL operator are transformed into multiple SQL parameters.

#### Example Usage:
```csharp
DateOnly? birthDate = null;
int[] peopleIds = { 1, 2, 3, 4 };
var includeAddresses = true;

var query = new DynamicQuery(
    $"""
     SELECT
         p.Name,
         << {includeAddresses} ? (SELECT a.Name FROM Address a WHERE a.PersonId = p.Id FOR JSON AUTO) : '' >> AS Addresses
     FROM Person p
     WHERE 1=1
         << {birthDate} ? AND p.BirthDate = {birthDate} >>
         << {peopleIds} ? AND p.Id IN {peopleIds} >>
     """);

var command = new SqlCommand();
query.RenderOn(command);
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
- Adding support for table-valued parameters.
- Extending compatibility with different SQL databases like Postgres, MySQL, and Oracle.
