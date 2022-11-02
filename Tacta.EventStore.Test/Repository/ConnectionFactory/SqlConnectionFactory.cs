using Microsoft.Data.SqlClient;
using Tacta.EventStore.Repository;

namespace Tacta.EventStore.Test.Repository.ConnectionFactory
{
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(string connectionString) => _connectionString = connectionString;

        public virtual string ConnectionString() => _connectionString;

        public SqlConnection SqlConnection() => new SqlConnection(ConnectionString());
    }
}
