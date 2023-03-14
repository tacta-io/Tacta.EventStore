using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tacta.EventStore.Repository
{
    public interface IEventStoreRepository
    {
        Task SaveAsync<T>(AggregateRecord aggregateRecord, IReadOnlyCollection<EventRecord<T>> eventRecords);
        Task<IReadOnlyCollection<EventStoreRecord<T>>> GetAsync<T>(string aggregateId);
        Task<IReadOnlyCollection<EventStoreRecord<T>>> GetFromSequenceAsync<T>(int sequence, int? take = null);
        Task<IReadOnlyCollection<EventStoreRecord<T>>> GetUntilAsync<T>(string aggregateId, Guid eventId);
        Task<IReadOnlyCollection<EventStoreRecord<T>>> GetUntilAsync<T>(string aggregateId, int sequence);
        Task<int> GetLatestSequence();
        Task<IReadOnlyCollection<EventStoreRecord<T>>> GetAsync<T>(string query, object param);
        Task<IReadOnlyCollection<EventStoreRecord<T>>> LoadAsync<T>(int offset, int rows, IEnumerable<string> eventsInterestedIn);
    }
}
