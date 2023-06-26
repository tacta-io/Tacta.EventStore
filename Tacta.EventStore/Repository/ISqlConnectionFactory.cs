using System.Data;

namespace Tacta.EventStore.Repository
{
    public interface ISqlConnectionFactory
    {
        IDbConnection SqlConnection();
        IDbTransaction Transaction { get; }
    }
}
