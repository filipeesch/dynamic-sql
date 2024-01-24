namespace DynamicSQL.Compiler;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DynamicSQL.Extensions;

public class Statement<TInput>
{
    private readonly Action<IStatementProcessor> _renderMethod;
    private readonly Func<TInput, object[]> _getValuesMethod;
    private readonly int _predictedStatementLength;

    internal Statement(
        Action<IStatementProcessor> renderMethod,
        Func<TInput, object[]> getValuesMethod,
        int predictedStatementLength)
    {
        _renderMethod = renderMethod;
        _getValuesMethod = getValuesMethod;
        _predictedStatementLength = predictedStatementLength;
    }

    public void Render(TInput input, DbCommand command)
    {
        var builder = new PooledStringBuilder(_predictedStatementLength * 2);
        var processor = new StatementProcessor(builder, command, _getValuesMethod(input));

        _renderMethod(processor);

        command.CommandText = builder.ToString();
    }

    public Task<List<TOutput>> QueryListAsync<TOutput>(
        DbConnection connection,
        TInput input,
        CancellationToken cancellationToken = default,
        int? predictedListSize = null)
    {
        return QueryAsyncEnumerable<TOutput>(connection, input, cancellationToken)
            .ToListAsync(cancellationToken, predictedListSize);
    }

    public async IAsyncEnumerable<TOutput> QueryAsyncEnumerable<TOutput>(
        DbConnection connection,
        TInput input,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var command = connection.CreateCommand();

        Render(input, command);

        var wasClosed = await connection.TryOpenConnectionAsync(cancellationToken);

        try
        {
            using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleResult, cancellationToken);

            var readFunc = OutputReader<TOutput>.GetReaderFunc(reader);

            while (await reader.ReadAsync(cancellationToken))
            {
                yield return readFunc(reader);
            }
        }
        finally
        {
            if (wasClosed)
            {
                connection.Close();
            }
        }
    }

    public async Task<TOutput?> QuerySingleAsync<TOutput>(
        DbConnection connection,
        TInput input,
        CancellationToken cancellationToken = default)
    {
        using var command = connection.CreateCommand();

        Render(input, command);

        var wasClosed = await connection.TryOpenConnectionAsync(cancellationToken);

        try
        {
            using var reader = await command.ExecuteReaderAsync(
                CommandBehavior.SingleResult | CommandBehavior.SingleRow,
                cancellationToken);

            var readFunc = OutputReader<TOutput>.GetReaderFunc(reader);

            return await reader.ReadAsync(cancellationToken) ?
                readFunc(reader) :
                default;
        }
        finally
        {
            if (wasClosed)
            {
                connection.Close();
            }
        }
    }

    public async Task<TOutput> QueryScalar<TOutput>(
        DbCommand command,
        TInput input,
        CancellationToken cancellationToken = default)
    {
        Render(input, command);

        return (TOutput)await command.ExecuteScalarAsync(cancellationToken);
    }

    public async Task<int> ExecuteAsync(
        DbCommand command,
        TInput input,
        CancellationToken cancellationToken = default)
    {
        Render(input, command);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
