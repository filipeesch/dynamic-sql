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

    public static async Task<T?> FirstOrDefaultAsync<T>(
        this IAsyncEnumerable<T> enumerable,
        CancellationToken cancellationToken = default)
    {
        await using var enumerator = enumerable.GetAsyncEnumerator(cancellationToken);

        if (await enumerator.MoveNextAsync())
        {
            return enumerator.Current;
        }

        return default;
    }
}
