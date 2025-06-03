using System;
using System.Threading.Tasks;

namespace Tacta.EventStore.Repository
{
    public interface IAuditRepository
    {
        Task SaveAsync(long sequence, DateTime appliedAt);
    }
}
