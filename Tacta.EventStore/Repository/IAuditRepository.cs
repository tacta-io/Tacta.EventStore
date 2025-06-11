using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tacta.EventStore.Repository
{
    public interface IAuditRepository
    {
        Task SaveAsync(long sequence, DateTime appliedAt);

        Task<List<long>> DetectProjectionsGap(long sequenceStart,long sequenceEnd);
    }
}
