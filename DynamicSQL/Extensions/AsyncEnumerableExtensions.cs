namespace DynamicSQL;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public static class AsyncEnumerableExtensions
{
    public static async Task<List<T>> ToListAsync<T>(
        this IAsyncEnumerable<T> enumerable,
        CancellationToken cancellationToken = default,
        int? predictedListSize = null)
    {
        var result = predictedListSize is null ?
            new List<T>() :
            new List<T>(predictedListSize.Value);

        await foreach (var item in enumerable.WithCancellation(cancellationToken))
        {
            result.Add(item);
        }

        return result;
    }
}
