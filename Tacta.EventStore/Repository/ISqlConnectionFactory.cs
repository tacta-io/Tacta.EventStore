using System.Data.SqlClient;

namespace Tacta.EventStore.Repository
{
    public interface ISqlConnectionFactory
    {
        SqlConnection SqlConnection();
    }
}
