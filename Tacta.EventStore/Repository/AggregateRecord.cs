using Tacta.EventStore.Repository.Exceptions;

namespace Tacta.EventStore.Repository
{
    public sealed class AggregateRecord
    {
        public string Id { get; }
        public string Name { get; }
        public int Version { get; }

        public AggregateRecord(string id, string name, int version)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new InvalidAggregateIdException("Id cannot be null or white space");

            if (string.IsNullOrWhiteSpace(name)) throw new InvalidAggregateRecordException("Name cannot be null or white space");

            if (version < 0) throw new InvalidAggregateRecordException("Version cannot be less then zero");

            (Id, Name, Version) = (id, name, version);
        }
    }
}
