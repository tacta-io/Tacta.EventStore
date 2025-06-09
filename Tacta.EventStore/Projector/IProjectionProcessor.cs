using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tacta.EventStore.Projector.Models;

namespace Tacta.EventStore.Projector
{
    public interface IProjectionProcessor
    {
        Task<ProcessData> Process(int take = 100, bool processParallel = false, bool auditEnabled = false, bool pesimisticProcessing = false);
        Task<string> Status(string service, int refreshRate = 5);
        Task Rebuild(IEnumerable<Type> projectionTypes = null);
    }
}
