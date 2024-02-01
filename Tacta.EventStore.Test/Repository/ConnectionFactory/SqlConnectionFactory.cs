using System.Data;
using Tacta.Connection;
using System.Data.Common;
using System.Threading.Tasks;
using System;
using System.Threading;

#if USE_SYSTEM_DATA_SQLCLIENT
    using System.Data.SqlClient;
#elif USE_MICROSOFT_DATA_SQLCLIENT
using Microsoft.Data.SqlClient;
#endif

namespace Tacta.EventStore.Test.Repository.ConnectionFactory
{
    public class SqlConnectionFactory : IConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(string connectionString) => _connectionString = connectionString;

        public virtual string ConnectionString() => _connectionString;

        public DbConnection Connection() => new SqlConnection(ConnectionString());

        public async Task ExecuteWithTransactionIfExists(Func<DbConnection, IDbTransaction, Task> func, CancellationToken ct = default)
        {
            await using var connection = Connection();
            await func.Invoke(connection, null);
        }

        public async Task<T> ExecuteWithTransactionIfExists<T>(Func<DbConnection, IDbTransaction, Task<T>> func, CancellationToken ct = default)
        {
            await using var connection = Connection();
            return await func.Invoke(Connection(), null);
        }
    }
}
