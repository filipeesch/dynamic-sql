namespace DynamicSQL.Compiler;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

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
        var builder = new CommandTextBuilder(_predictedStatementLength * 2);

        var processor = new StatementProcessor<TInput>(
            builder,
            new StatementParameters(command),
            input,
            _getValuesMethod(input));

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

    public async Task<TOutput?> QuerySingleAsync<TOutput>(
        DbConnection connection,
        TInput input,
        CancellationToken cancellationToken = default)
    {
        return await QueryAsyncEnumerable<TOutput>(connection, input, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);
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

            await foreach (var item in reader.ToAsyncEnumerable<TOutput>().WithCancellation(cancellationToken))
            {
                yield return item;
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
