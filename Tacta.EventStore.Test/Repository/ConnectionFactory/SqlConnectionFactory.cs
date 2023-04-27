using System.Data;
using Tacta.EventStore.Repository;

#if USE_SYSTEM_DATA_SQLCLIENT
    using System.Data.SqlClient;
#elif USE_MICROSOFT_DATA_SQLCLIENT
    using Microsoft.Data.SqlClient;
#endif

namespace Tacta.EventStore.Test.Repository.ConnectionFactory
{
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(string connectionString) => _connectionString = connectionString;

        public virtual string ConnectionString() => _connectionString;

        public IDbConnection SqlConnection() => new SqlConnection(ConnectionString());
    }
}
