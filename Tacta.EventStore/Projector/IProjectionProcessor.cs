using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tacta.EventStore.Domain;

namespace Tacta.EventStore.Projector
{
    public interface IProjectionProcessor
    {
        Task<int> Process(int take = 100, bool processParallel = false, bool auditEnabled = false);
        Task<string> Status(string service, int refreshRate = 5);
        Task Rebuild(IEnumerable<Type> projectionTypes = null);
    }
}
