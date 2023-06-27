using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Tacta.EventStore.Domain;
using Tacta.EventStore.Projector;
using Tacta.EventStore.Repository;
using Tacta.EventStore.Test.Projector.DomainEvents;
using Tacta.EventStore.Test.Projector.Projections;
using Xunit;

namespace Tacta.EventStore.Test.Projector
{
    public class ProjectionTest
    {
        private readonly Mock<IProjectionRepository> _projectionRepository;
        private readonly Mock<IEventStoreRepository> _eventStoreRepository;

        public ProjectionTest()
        {
            _projectionRepository = new Mock<IProjectionRepository>();
            _eventStoreRepository = new Mock<IEventStoreRepository>();
        }

        [Fact]
        public async Task OnUserRegistered()
        {
            // Given
            var userRegistered = new UserRegistered("userId", "John Doe", false);
            userRegistered.WithVersionAndSequence(1, 120);

            _eventStoreRepository.Setup(x => x.GetFromSequenceAsync<DomainEvent>(0, 100))
                .ReturnsAsync(new List<EventStoreRecord<DomainEvent>>
                {
                    new EventStoreRecord<DomainEvent>
                    {
                        AggregateId = userRegistered.AggregateId,
                        CreatedAt = userRegistered.CreatedAt,
                        Event = userRegistered,
                        Id = Guid.NewGuid(),
                        Sequence = userRegistered.Sequence,
                        Version = userRegistered.Version
                    }
                });

            // When
            var userProjection = new UserProjection(_projectionRepository.Object);
            var processor = new ProjectionProcessor(new List<IProjection> {userProjection}, _eventStoreRepository.Object);
            await processor.Process();

            // Then
            Assert.Equal(120, userProjection.GetSequence());
        }
        
        [Fact]
        public async Task NoProjectionsAdded_ReturnsZero()
        {
            // Given
            var userRegistered = new UserRegistered("userId", "John Doe", false);
            userRegistered.WithVersionAndSequence(1, 120);

            _eventStoreRepository.Setup(x => x.GetFromSequenceAsync<DomainEvent>(0, 100))
                .ReturnsAsync(new List<EventStoreRecord<DomainEvent>>
                {
                    new EventStoreRecord<DomainEvent>
                    {
                        AggregateId = userRegistered.AggregateId,
                        CreatedAt = userRegistered.CreatedAt,
                        Event = userRegistered,
                        Id = Guid.NewGuid(),
                        Sequence = userRegistered.Sequence,
                        Version = userRegistered.Version
                    }
                });

            // When
            var userProjection = new UserProjection(_projectionRepository.Object);
            var processor = new ProjectionProcessor(new List<IProjection>(), _eventStoreRepository.Object);
            await processor.Process();

            // Then
            Assert.Equal(0, userProjection.GetSequence());
        }


        [Fact]
        public async Task OnUserBanned()
        {
            // Given
            var userRegistered = new UserRegistered("userId", "John Doe", false);
            userRegistered.WithVersionAndSequence(1, 120);

            var userBanned = new UserBanned("userId");
            userRegistered.WithVersionAndSequence(2, 345);

            _eventStoreRepository.Setup(x => x.GetFromSequenceAsync<DomainEvent>(0, 100))
                .ReturnsAsync(new List<EventStoreRecord<DomainEvent>>
                {
                    new EventStoreRecord<DomainEvent>
                    {
                        AggregateId = userRegistered.AggregateId,
                        CreatedAt = userRegistered.CreatedAt,
                        Event = userRegistered,
                        Id = Guid.NewGuid(),
                        Sequence = userRegistered.Sequence,
                        Version = userRegistered.Version
                    },
                    new EventStoreRecord<DomainEvent>
                    {
                        AggregateId = userBanned.AggregateId,
                        CreatedAt = userBanned.CreatedAt,
                        Event = userBanned,
                        Id = Guid.NewGuid(),
                        Sequence = userBanned.Sequence,
                        Version = userBanned.Version
                    }
                });

            // When
            var userProjection = new UserProjection(_projectionRepository.Object);
            var processor = new ProjectionProcessor(new List<IProjection> { userProjection }, _eventStoreRepository.Object);
            await processor.Process();

            // Then
            Assert.Equal(345, userProjection.GetSequence());
        }
    }
}