using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            var userRegistered = new UserRegistered("userId", "John Doe", false, 120);

            _eventStoreRepository.Setup(x => x.GetFromSequenceAsync<DomainEvent>(0, 100, CancellationToken.None))
                .ReturnsAsync(new List<EventStoreRecord<DomainEvent>>
                {
                    new EventStoreRecord<DomainEvent>
                    {
                        AggregateId = userRegistered.AggregateId,
                        CreatedAt = userRegistered.CreatedAt,
                        Event = userRegistered,
                        Id = Guid.NewGuid(),
                        Sequence = 120,
                        Version = 1
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
            var userRegistered = new UserRegistered("userId", "John Doe", false, 120);

            _eventStoreRepository.Setup(x => x.GetFromSequenceAsync<DomainEvent>(0, 100, CancellationToken.None))
                .ReturnsAsync(new List<EventStoreRecord<DomainEvent>>
                {
                    new EventStoreRecord<DomainEvent>
                    {
                        AggregateId = userRegistered.AggregateId,
                        CreatedAt = userRegistered.CreatedAt,
                        Event = userRegistered,
                        Id = Guid.NewGuid(),
                        // Sequence = userRegistered.Sequence,
                        // Version = userRegistered.Version
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
            var userRegistered = new UserRegistered("userId", "John Doe", false, 120);

            var userBanned = new UserBanned("userId", 345);

            _eventStoreRepository.Setup(x => x.GetFromSequenceAsync<DomainEvent>(0, 100, CancellationToken.None))
                .ReturnsAsync(new List<EventStoreRecord<DomainEvent>>
                {
                    new EventStoreRecord<DomainEvent>
                    {
                        AggregateId = userRegistered.AggregateId,
                        CreatedAt = userRegistered.CreatedAt,
                        Event = userRegistered,
                        Id = Guid.NewGuid(),
                        Sequence = 120,
                        Version = 1
                    },
                    new EventStoreRecord<DomainEvent>
                    {
                        AggregateId = userBanned.AggregateId,
                        CreatedAt = userBanned.CreatedAt,
                        Event = userBanned,
                        Id = Guid.NewGuid(),
                        Sequence = 345,
                        Version = 2
                    }
                });

            // When
            var userProjection = new UserProjection(_projectionRepository.Object);
            var processor = new ProjectionProcessor(new List<IProjection> { userProjection }, _eventStoreRepository.Object);
            await processor.Process();

            // Then
            Assert.Equal(345, userProjection.GetSequence());
        }
        
        [Fact]
        public async Task OnRebuildWithoutProcess()
        {
            // Given
            _projectionRepository.Setup(p => p.GetSequenceAsync()).ReturnsAsync(() =>
            {
                if (_projectionRepository.Invocations.Any(i =>
                        i.Method.Name == nameof(_projectionRepository.Object.DeleteAllAsync)))
                {
                    return 0;
                }

                return 345;
            });

            var userProjection = new UserProjection(_projectionRepository.Object);
            var processor = new ProjectionProcessor(new List<IProjection> { userProjection }, _eventStoreRepository.Object);
            
            // When
            await processor.Rebuild();

            // Then
            Assert.Equal(0, userProjection.GetSequence());
        }

        [Fact]
        public async Task OnRebuildAndProcess()
        {
            // Given
            var userRegistered = new UserRegistered("userId", "John Doe", false, 120);

            var userBanned = new UserBanned("userId", 345);

            _eventStoreRepository.Setup(x => x.GetFromSequenceAsync<DomainEvent>(0, 100, CancellationToken.None))
                .ReturnsAsync(new List<EventStoreRecord<DomainEvent>>
                {
                    new EventStoreRecord<DomainEvent>
                    {
                        AggregateId = userRegistered.AggregateId,
                        CreatedAt = userRegistered.CreatedAt,
                        Event = userRegistered,
                        Id = Guid.NewGuid(),
                        // Sequence = userRegistered.Sequence,
                        // Version = userRegistered.Version
                    },
                    new EventStoreRecord<DomainEvent>
                    {
                        AggregateId = userBanned.AggregateId,
                        CreatedAt = userBanned.CreatedAt,
                        Event = userBanned,
                        Id = Guid.NewGuid(),
                        // Sequence = userBanned.Sequence,
                        // Version = userBanned.Version
                    }
                });

            var userProjection = new UserProjection(_projectionRepository.Object);
            var processor = new ProjectionProcessor(new List<IProjection> { userProjection }, _eventStoreRepository.Object);

            _projectionRepository.Setup(p => p.GetSequenceAsync()).ReturnsAsync(() =>
            {
                if (_projectionRepository.Invocations.Any(
                        i => i.Method.Name == nameof(_projectionRepository.Object.DeleteAllAsync)))
                    return 0;
                
                return userProjection.GetSequence(); 
            });

            // When
            await processor.Process();
            await processor.Rebuild();

            // Then
            Assert.Equal(0, userProjection.GetSequence());
        }

        [Fact]
        public async Task OnNormalRebuildFlow()
        {
            // Given
            var userRegistered = new UserRegistered("userId", "John Doe", false, 120);

            var userBanned = new UserBanned("userId", 345);

            _eventStoreRepository.Setup(x => x.GetFromSequenceAsync<DomainEvent>(0, 100, CancellationToken.None))
                .ReturnsAsync(new List<EventStoreRecord<DomainEvent>>
                {
                    new EventStoreRecord<DomainEvent>
                    {
                        AggregateId = userRegistered.AggregateId,
                        CreatedAt = userRegistered.CreatedAt,
                        Event = userRegistered,
                        Id = Guid.NewGuid(),
                        Sequence = 120,
                        Version = 1
                    },
                    new EventStoreRecord<DomainEvent>
                    {
                        AggregateId = userBanned.AggregateId,
                        CreatedAt = userBanned.CreatedAt,
                        Event = userBanned,
                        Id = Guid.NewGuid(),
                        Sequence = 345,
                        Version = 2
                    }
                });

            var userProjection = new UserProjection(_projectionRepository.Object);
            var processor = new ProjectionProcessor(new List<IProjection> { userProjection }, _eventStoreRepository.Object);
            
            _projectionRepository.Setup(p => p.GetSequenceAsync()).ReturnsAsync(() =>
            {
                if (_projectionRepository.Invocations.Any(
                        i => i.Method.Name == nameof(_projectionRepository.Object.DeleteAllAsync)))
                    return 0;
                
                return userProjection.GetSequence(); 
            });
            
            // When
            await processor.Process();
            await processor.Rebuild();
            await processor.Process();

            // Then
            Assert.Equal(345, userProjection.GetSequence());
        }
    }
}