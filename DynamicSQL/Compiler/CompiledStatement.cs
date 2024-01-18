namespace DynamicSQL.Compiler;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DynamicSQL.Extensions;

public class CompiledStatement<TInput>
{
    private readonly Action<IStatementProcessor> _renderMethod;
    private readonly Func<TInput, object[]> _getValuesMethod;
    private readonly int _predictedStatementLength;

    internal CompiledStatement(
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
        var builder = new StringBuilder(_predictedStatementLength);
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
