namespace DynamicSQL;

using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

public static class AdoNetExtensions
{
    public static async Task<bool> TryOpenConnectionAsync(this DbConnection connection, CancellationToken cancellationToken)
    {
        if (connection.State == ConnectionState.Open)
        {
            return false;
        }

        await connection.OpenAsync(cancellationToken);
        return true;
    }
}
