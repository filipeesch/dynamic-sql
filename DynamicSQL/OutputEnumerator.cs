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
    private readonly OutputReader<T> _outputReader = new(reader);

    public T Current => _outputReader.Read();

    public ValueTask DisposeAsync() => new();

    public async ValueTask<bool> MoveNextAsync() => await _outputReader.MoveNextAsync(cancellationToken);
}
