namespace DynamicSQL.Compiler;

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DynamicSQL.Extensions;
using DynamicSQL.Parser.Expressions;

public class CompiledStatement<TInput>
{
    private readonly Action<IStatementProcessor> _renderMethod;
    private readonly Func<TInput, object[]> _getValuesMethod;
    private readonly int _predictedStatementLength;
    private readonly IReadOnlyList<IParsedExpression> _variationExpressions;

    private readonly ConcurrentDictionary<object[], CachedStatementVariation> _variationsCache;

    internal CompiledStatement(
        Action<IStatementProcessor> renderMethod,
        Func<TInput, object[]> getValuesMethod,
        int predictedStatementLength,
        IReadOnlyList<IParsedExpression> variationExpressions)
    {
        _renderMethod = renderMethod;
        _getValuesMethod = getValuesMethod;
        _predictedStatementLength = predictedStatementLength;
        _variationExpressions = variationExpressions;
        _variationsCache = new(new StatementVariationsEqualityComparer(variationExpressions));
    }

    public void Render(TInput input, DbCommand command)
    {
        var values = _getValuesMethod(input);

        if (_variationsCache.TryGetValue(values, out var cached))
        {
        }

        var builder = new PooledStringBuilder(_predictedStatementLength);
        var processor = new StatementProcessor(builder, command, values);

        _renderMethod(processor);

        _variationsCache.TryAdd(values,)

        command

        command.CommandText = builder.ToString();
    }

    public Task<List<TOutput>> QueryListAsync<TOutput>(
        DbConnection connection,
        TInput input,
        CancellationToken cancellationToken = default,
        int? predictedListSize = null)
    {
        return QueryAsyncEnumerable<TOutput>(connection, input, cancellationToken).ToListAsync(cancellationToken, predictedListSize);
    }

    public Task<List<TOutput>> QueryListAsync<TOutput>(
        DbCommand command,
        TInput input,
        CancellationToken cancellationToken = default,
        int? predictedListSize = null)
    {
        return QueryAsyncEnumerable<TOutput>(command, input, cancellationToken).ToListAsync(cancellationToken, predictedListSize);
    }

    public async IAsyncEnumerable<TOutput> QueryAsyncEnumerable<TOutput>(
        DbConnection connection,
        TInput input,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var command = connection.CreateCommand();

        Render(input, command);

        await connection.OpenAsync(cancellationToken);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var readFunc = OutputReader<TOutput>.GetReaderFunc(reader);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return readFunc(reader);
        }
    }

    public async IAsyncEnumerable<TOutput> QueryAsyncEnumerable<TOutput>(
        DbCommand command,
        TInput input,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Render(input, command);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var readFunc = OutputReader<TOutput>.GetReaderFunc(reader);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return readFunc(reader);
        }
    }
}

internal class CachedStatementVariation
{
    public CachedStatementVariation(string commandText, IReadOnlyList<Parameter> parameters)
    {
        CommandText = commandText;
        Parameters = parameters;
    }

    public string CommandText { get; }

    public IReadOnlyList<Parameter> Parameters { get; }

    public class Parameter
    {
        public Parameter(string name, int valueIndex)
        {
            Name = name;
            ValueIndex = valueIndex;
        }

        public string Name { get; }

        public int ValueIndex { get; }
    }
}

internal class StatementVariationsEqualityComparer(IReadOnlyList<IParsedExpression> variationExpressions) : IEqualityComparer<object[]>
{
    public bool Equals(object[] x, object[] y)
    {
        foreach (var expression in variationExpressions)
        {
            switch (expression)
            {
                case InArrayExpression inArrayExpression
                    when Count(x[inArrayExpression.ParameterIndex]) != Count(y[inArrayExpression.ParameterIndex]):
                case ConditionalExpression conditionalExpression
                    when StatementProcessor.ConditionValueTest(x[conditionalExpression.ConditionValueIndex]) !=
                         StatementProcessor.ConditionValueTest(y[conditionalExpression.ConditionValueIndex]):
                    return false;
            }
        }

        return true;
    }

    public int GetHashCode(object[] values)
    {
        const int prime = 397; // A prime number used for hashing
        var hash = 1;

        foreach (var expression in variationExpressions)
        {
            var itemHash = expression switch
            {
                InArrayExpression inArrayExpression => Count(values[inArrayExpression.ParameterIndex]).GetHashCode(),
                ConditionalExpression conditionalExpression => StatementProcessor
                    .ConditionValueTest(values[conditionalExpression.ConditionValueIndex])
                    .GetHashCode(),
                _ => throw new ArgumentException("Unsupported Expression type")
            };

            hash = (hash * prime) ^ itemHash;
        }

        return hash;
    }

    private static int Count(object values)
    {
        return values switch
        {
            ICollection coll => coll.Count,
            IEnumerable<object> enumerable => enumerable.Count(),
            _ => throw new ArgumentException("The Count method only supports collections")
        };
    }
}
