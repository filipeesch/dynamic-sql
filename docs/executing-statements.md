# Executing Statements and Parsing Results

DynamicSQL provides convenient methods for executing SQL statements and parsing query results directly into .NET classes or records. This feature supports both classes and records, including those with constructors that have parameters. The parse function, compiled at runtime during the first execution, is cached for future use, ensuring efficient operation. For properties or constructor parameters not returned by the query, the default type value is used for initialization.

## How It Works

DynamicSQL offers three main methods for executing queries and parsing results:

- `QueryListAsync<TOutput>`: Retrieves a list of items.
- `QuerySingleAsync<TOutput>`: Retrieves a single item.
- `QueryAsyncEnumerable<TOutput>`: Streams results asynchronously, useful for large datasets.

### Example 1: QueryListAsync

Retrieve a list of employees within a specific ID range:

```csharp
var statement = StatementCompiler.Compile<IdRangeInput>(
    (i, c) => $"SELECT Id, Name FROM Employee WHERE Id >= {i.StartId} AND Id <= {i.EndId}");
var input = new IdRangeInput(1, 5);

var employees = await statement.QueryListAsync<EmployeeRecord>(_connection, input);
```

### Example 2: QuerySingleAsync

Retrieve a single employee record:

```csharp
var statement = StatementCompiler.Compile<int>(
    (i, c) => $"SELECT Id, Name, Status FROM Employee WHERE Id = {i}");
var id = 10;

var employee = await statement.QuerySingleAsync<EmployeeRecord>(_connection, id);
```

### Example 3: QueryAsyncEnumerable (Streaming)

Stream employee records asynchronously:

```csharp
var statement = StatementCompiler.Compile<IdRangeInput>(
    (i, c) => $"SELECT Id, Name FROM Employee WHERE Id >= {i.StartId} AND Id <= {i.EndId}");
var input = new IdRangeInput(1, 5);

await foreach (var employee in statement.QueryAsyncEnumerable<EmployeeRecord>(_connection, input))
{
    // Process each employee
}
```
