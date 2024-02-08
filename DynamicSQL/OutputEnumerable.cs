namespace DynamicSQL;

using System.Collections.Generic;
using System.Data.Common;
using System.Threading;

internal class OutputEnumerable<T>(DbDataReader reader) : IAsyncEnumerable<T>
{
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
        new OutputEnumerator<T>(reader, cancellationToken);
}
