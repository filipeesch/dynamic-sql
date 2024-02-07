# Automatic Parameters

DynamicSQL enhances security and simplifies query construction by automatically converting placeholders in interpolated strings into SQL parameters. This key feature prevents SQL injection vulnerabilities and supports efficient database operations.

## Overview

DynamicSQL transforms placeholders within string interpolation (`{variable}`) into parameterized SQL commands. This process, known as automatic parameterization, seamlessly handles both scalar values and lists. The latter is particularly useful when working with the `IN` SQL operator, ensuring queries remain both secure and easy to read.

### Example

Consider a simple example that demonstrates the automatic conversion of a scalar value into a parameter:

```csharp
var statement = StatementCompiler.Compile<QueryInput>(
    (i, c) =>
        $"""
        SELECT Name, Age
        FROM Employees
        WHERE Id = {i.EmployeeId}
        """);
```

The resulting statement sent to the database would resemble:

```sql
SELECT Name, Age
FROM Employees
WHERE Id = @p0
```

This illustrates how `i.EmployeeId` is automatically parameterized as `@p0`, enhancing query security by preventing direct value insertion.

## The `IN` Operator with Lists

DynamicSQL greatly simplifies the inclusion of lists in the `IN` SQL operator by converting them into multiple parameters, thus maintaining SQL injection protection.

### Example: List Parameterization

```csharp
var statement = StatementCompiler.Compile<QueryInput>(
    (i, c) =>
        $"""
        SELECT Name, Department
        FROM Employees
        WHERE DepartmentId IN {i.DepartmentIds}
        """);

var input = new QueryInput { DepartmentIds = new[] { 1, 2, 3 } };
```

The statement dynamically prepared for the database:

```sql
SELECT Name, Department
FROM Employees
WHERE DepartmentId IN(@p0_0, @p0_1, @p0_2)
```

In this scenario, `IN {i.DepartmentIds}` is automatically expanded into a series of parameters (`@p0_0, @p0_1, @p0_2`), with each element of the `DepartmentIds` list safely represented in the query's `IN` clause. This special handling by DynamicSQL eliminates the manual enumeration of list items, ensuring both query efficiency and security.
