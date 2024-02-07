# Conditional Rendering

Conditional rendering in DynamicSQL allows you to include or exclude segments of your SQL query based on dynamic conditions. This feature simplifies the creation of flexible and dynamic SQL queries, enhancing code readability and maintainability.

## Overview

The syntax for conditional rendering in DynamicSQL uses `<< >>` tags combined with a ternary-like condition: `<< condition ? truePart : falsePart >>`. The `falsePart` is optional, enabling more concise expressions when no alternative action is required.

In the context of evaluating conditions:
- A `null` value, a `false` boolean, or an empty list or enumeration are considered **false**.
- Any non-null value, a `true` boolean, or a non-empty list or enumeration are considered **true**.

Given the `QueryInput` object below, let's explore some samples:

```csharp
public record QueryInput(
    bool SelectName,
    bool? IsActive,
    IEnumerable<int> DepartmentIds,
    DateTime? StartDate,
    DateTime? EndDate,
    bool? HasPayments);
```

## Example 1

A basic example to illustrate conditional rendering:

```csharp
var statement = StatementCompiler.Compile<QueryInput>(
    (i, c) =>
        $"""
         SELECT
            Id
            << {i.SelectName} ? , Name >>
         FROM Employees
         WHERE 1=1
             << {i.IsActive} ? AND IsActive = {i.IsActive} >>
         """);
```

Here, if `SelectName` is `true`, the `Name` column will be included in the SELECT clause. For `IsActive`, if the value is not `null`, the generated SQL query will include the condition to apply the `IsActive` filter. If `IsActive` is `null`, that condition is excluded, showcasing how null values are considered false in conditional rendering.

## Example 2

A more complex scenario combining multiple conditions:

```csharp
var statement = StatementCompiler.Compile<QueryInput>(
    (i, c) =>
        $"""
         SELECT Id, Name, Department, HireDate
         FROM Employees
         WHERE 1=1
             << {i.IsActive} ? AND IsActive = {i.IsActive} >>
             << {i.DepartmentIds} ? AND DepartmentId IN {i.DepartmentId} >>
             << {i.StartDate} ? AND HireDate >= {i.StartDate} >>
             << {i.EndDate} ? AND HireDate <= {i.EndDate} >>
         """);
```

This query dynamically adapts based on `QueryInput` properties, demonstrating DynamicSQL's capability to handle multiple dynamic conditions to construct specific queries.

## Nested Conditions

DynamicSQL supports nested conditions for constructing sophisticated query logic.

### Example:

```csharp
var statement = StatementCompiler.Compile<QueryInput>(
    (i, c) =>
        $"""
         SELECT Id, Name
         FROM Employees e
         WHERE 1=1
             << {i.HasPayments} ? 
                << 
                    {i.HasPayments == true}
                    ? EXISTS(SELECT * FROM Payments p WHERE p.EmployeeId = e.Id)
                    : NOT EXISTS(SELECT * FROM Payments p WHERE p.EmployeeId = e.Id)
                >>        
             >>
         """);

var input = new QueryInput
{
    HasPayments = true
};
```

In this nested condition example, the query incorporates a subquery to check for the existence or absence of related payments for each employee. 
