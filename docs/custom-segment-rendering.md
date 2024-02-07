# Custom Segment Rendering

The Custom Segment Rendering feature allows the client app to dynamically inject SQL segments and parameters into queries, providing a powerful tool for constructing dynamic SQL statements tailored to specific application requirements.

## How It Works

To leverage the segment rendering feature, you'll utilize the `SegmentRenderer` method within the `StatementCompositor`. This method enables you to pass a delegate function responsible for injecting custom SQL code and parameters into your query. The custom segment is then seamlessly integrated into the SQL statement wherever the string interpolation placeholder is positioned.

### Example

```csharp
var statement = StatementCompiler.Compile<IEnumerable<int>>(
    (i, c) => $"""
        SELECT 
            Id,
            Name
        FROM Person
        WHERE Id IN({c.SegmentRenderer(RenderIdsParameters)})""");
```

In this example, the `RenderIdsParameters` function is provided by the client application and serves as the injection point for parameter names and values.

```csharp
private static void RenderIdsParameters(SegmentRendererContext<IEnumerable<int>> context)
{
    var count = 0;

    foreach (var i in context.Input)
    {
        if (count > 0)
        {
            context.CommandTextBuilder.Append(',');
        }

        // Create a new parameter for each ID and add it to the command
        var p = context.Parameters.Add($"Id{count}", i);

        // Append the parameter's placeholder to the command text
        context.CommandTextBuilder
            .Append('@')
            .Append(p.ParameterName);

        count++;
    }
}
```

### Combining Conditional and Custom Segment Rendering

This example demonstrates how to conditionally apply a custom-rendered SQL segment within a query:

```csharp
var statement = StatementCompiler.Compile<QueryInput>(
    (input, compositor) => $"""
        SELECT 
            Id,
            Name,
            Status
        FROM Employee
        WHERE 1=1
            << {input.DepartmentIds} ? AND DepartmentId IN({compositor.SegmentRenderer(RenderDepartmentParameters)}) >>
        """);

private static void RenderDepartmentParameters(SegmentRendererContext<QueryInput> context)
{
    var count = 0;

    foreach (var i in context.Input.DepartmentIds)
    {
        if (count > 0)
        {
            context.CommandTextBuilder.Append(',');
        }

        // Create a new parameter for each ID and add it to the command
        var p = context.Parameters.Add($"DepartmentId{count}", i);

        // Append the parameter's placeholder to the command text
        context.CommandTextBuilder
            .Append('@')
            .Append(p.ParameterName);

        count++;
    }
}

public record QueryInput(bool? IsActive, IEnumerable<int> DepartmentIds);
```
