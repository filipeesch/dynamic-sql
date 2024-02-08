namespace DynamicSQL;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

internal class OutputEnumerator<T>(DbDataReader reader, CancellationToken cancellationToken) : IAsyncEnumerator<T>
{
    private static readonly ConcurrentDictionary<string, Func<DbDataReader, T>> CompiledReaders = new();
    private readonly Func<DbDataReader, T> _readFunc = GetReadFunc(reader);

    public T Current => _readFunc(reader);

    public ValueTask DisposeAsync() => new();

    public async ValueTask<bool> MoveNextAsync() => await reader.ReadAsync(cancellationToken);

    private static Func<DbDataReader, T> GetReadFunc(DbDataReader reader)
    {
        var columns = Enumerable
            .Range(0, reader.FieldCount)
            .Select(i => new Column(i, reader.GetName(i)))
            .ToList();

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

    private static Func<DbDataReader, T> CreateCompiledReadFunc(IReadOnlyList<Column> columns)
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
        IReadOnlyList<Column> columns,
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
                    Column: columns.FirstOrDefault(col => col.Name.Equals(par.Name, StringComparison.InvariantCultureIgnoreCase))
                ))
            .Select(
                parameter => ReaderHelper.CreateDataReaderGetValueExpression(
                    readerParameter,
                    parameter.Parameter.ParameterType,
                    parameter.Column?.Index));

        return Expression.New(ctor, parameters);
    }

    private static Expression CreateDefaultConstructorStrategy(
        IEnumerable<Column> columns,
        Expression readerParameter)
    {
        var expressions = new List<Expression>();
        var resultVariable = Expression.Variable(typeof(T), "result");
        expressions.Add(Expression.Assign(resultVariable, Expression.New(typeof(T))));

        expressions.AddRange(
            columns
                .Select(x => (Property: typeof(T).GetProperty(x.Name), Column: x))
                .Where(x => x.Property is not null)
                .Select(
                    property => Expression.Assign(
                        Expression.Property(resultVariable, property.Property!),
                        ReaderHelper.CreateDataReaderGetValueExpression(
                            readerParameter,
                            property.Property!.PropertyType,
                            property.Column.Index))));

        expressions.Add(resultVariable);

        return Expression.Block([resultVariable], expressions);
    }

    private static string GetCompiledReaderKey(IEnumerable<Column> columns) => string.Join(",", columns.Select(x => x.Name));

    private class Column
    {
        public Column(int index, string name)
        {
            Index = index;
            Name = name;
        }

        public int Index { get; }

        public string Name { get; }
    }
}
