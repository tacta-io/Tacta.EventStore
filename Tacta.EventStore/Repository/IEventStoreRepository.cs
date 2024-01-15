using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tacta.EventStore.Repository
{
    public interface IEventStoreRepository
    {
        Task SaveAsync<T>(AggregateRecord aggregateRecord, IReadOnlyCollection<EventRecord<T>> eventRecords,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<EventStoreRecord<T>>> GetAsync<T>(string aggregateId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<EventStoreRecord<T>>> GetFromSequenceAsync<T>(int sequence, int? take = null, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<EventStoreRecord<T>>> GetUntilAsync<T>(string aggregateId, Guid eventId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<EventStoreRecord<T>>> GetUntilAsync<T>(string aggregateId, int sequence, CancellationToken cancellationToken = default);
        Task<int> GetLatestSequence(CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<EventStoreRecord<T>>> GetAsync<T>(string query, object param, CancellationToken cancellationToken = default);
    }
}