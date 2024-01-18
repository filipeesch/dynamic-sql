namespace DynamicSQL;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

public static class OutputReader<T>
{
    private static readonly ConcurrentDictionary<string, Func<DbDataReader, T>> CompiledReaders = new();


    public static Func<DbDataReader, T> GetReaderFunc(DbDataReader reader)
    {
        var columns = reader.GetColumnSchema();
        return GetReadFunc(columns);
    }

    private static Func<DbDataReader, T> GetReadFunc(IReadOnlyCollection<DbColumn> columns)
    {
        var readerKey = GetCompiledReaderKey(columns);

        if (CompiledReaders.TryGetValue(readerKey, out var readFunc))
        {
            return readFunc;
        }

        lock (CompiledReaders)
        {
            readFunc = CompiledReaders.GetOrAdd(readerKey, _ => CreateCompiledReadFunc(columns));
        }

        return readFunc;
    }

    private static Func<DbDataReader, T> CreateCompiledReadFunc(IEnumerable<DbColumn> columns)
    {
        var readerParameter = Expression.Parameter(typeof(DbDataReader), "reader");

        var bodyExpression = typeof(T).GetConstructor(Type.EmptyTypes) != null
            ? CreateDefaultConstructorStrategy(columns, readerParameter)
            : CreateParameterConstructorStrategy(columns, readerParameter);

        return Expression
            .Lambda<Func<DbDataReader, T>>(bodyExpression, readerParameter)
            .Compile();
    }

    private static Expression CreateParameterConstructorStrategy(
        IEnumerable<DbColumn> columns,
        Expression readerParameter)
    {
        var constructors = typeof(T).GetConstructors();

        if (!constructors.Any())
        {
            throw new Exception($"No public constructor was found in the class '{typeof(T).FullName}'");
        }

        var ctor = constructors[0];

        var parameters = ctor
            .GetParameters()
            .Select(
                par =>
                (
                    Parameter: par,
                    Column: columns.FirstOrDefault(col => col.ColumnName.Equals(par.Name, StringComparison.InvariantCultureIgnoreCase))
                ))
            .Select(
                parameter => ReaderHelper.CreateDataReaderGetValueExpression(
                    readerParameter,
                    parameter.Parameter.ParameterType,
                    parameter.Column?.ColumnOrdinal));

        return Expression.New(ctor, parameters);
    }

    private static Expression CreateDefaultConstructorStrategy(
        IEnumerable<DbColumn> columns,
        Expression readerParameter)
    {
        var expressions = new List<Expression>();
        var resultVariable = Expression.Variable(typeof(T), "result");
        expressions.Add(Expression.Assign(resultVariable, Expression.New(typeof(T))));

        expressions.AddRange(
            columns
                .Select(x => (Property: typeof(T).GetProperty(x.ColumnName), Column: x))
                .Where(x => x.Property is not null)
                .Select(
                    property => Expression.Assign(
                        Expression.Property(resultVariable, property.Property!),
                        ReaderHelper.CreateDataReaderGetValueExpression(
                            readerParameter,
                            property.Property!.PropertyType,
                            property.Column.ColumnOrdinal))));

        expressions.Add(resultVariable);

        return Expression.Block([resultVariable], expressions);
    }

    private static string GetCompiledReaderKey(IEnumerable<DbColumn> columns) => string.Join(",", columns.Select(x => x.ColumnName));
}
