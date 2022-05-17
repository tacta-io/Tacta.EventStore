using System;

namespace Tacta.EventStore.Test.Repository.ReadModels
{
    public class UserReadModelTestBuilder
    {
        private Guid _id;
        private Guid _eventId;
        private string _name;
        private int _sequence;
        private DateTime _updatedAt;

        public static UserReadModelTestBuilder Default() => new UserReadModelTestBuilder()
            .WithEventId(Guid.NewGuid())
            .WithId(Guid.NewGuid())
            .WithName("John Doe")
            .WithSequence(45)
            .WithUpdatedAt(DateTime.Now);

        public UserReadModelTestBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public UserReadModelTestBuilder WithEventId(Guid eventId)
        {
            _eventId = eventId;
            return this;
        }

        public UserReadModelTestBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public UserReadModelTestBuilder WithSequence(int sequence)
        {
            _sequence = sequence;
            return this;
        }

        public UserReadModelTestBuilder WithUpdatedAt(DateTime updatedAt)
        {
            _updatedAt = updatedAt;
            return this;
        }

        public UserReadModel Build()
        {
            return new UserReadModel
            {
                Id = _id,
                EventId = _eventId,
                Name = _name,
                Sequence = _sequence,
                UpdatedAt = _updatedAt
            };
        }
    }
}
