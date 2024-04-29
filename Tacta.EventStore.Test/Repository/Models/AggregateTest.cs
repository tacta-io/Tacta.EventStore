using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using Tacta.EventStore.Domain;
using Tacta.EventStore.Repository.Models;
using Tacta.EventStore.Test.Domain.AggregateRoots;
using Tacta.EventStore.Test.Domain.DomainEvents;
using Tacta.EventStore.Test.Domain.Identities;
using Xunit;

namespace Tacta.EventStore.Test.Repository.Models
{
    public class AggregateTest
    {
        [Fact]
        public void AggregateWithoutDomainEvents_ShouldHaveCorrectValues()
        {
            // Given
            var backlogItemId = new BacklogItemId();
            var backlogItemCreatedEvent = new BacklogItemCreated("summary", backlogItemId);
            var subTaskAddedEvent = new SubTaskAdded(new TaskId(), "title");
            var domainEvents = new List<IDomainEvent> { backlogItemCreatedEvent, subTaskAddedEvent };
            var aggregateRoot = new BacklogItem(domainEvents);
            
            // When
            var aggregate = new Aggregate(aggregateRoot);
            
            // Then
            aggregate.AggregateRecord.Should().NotBeNull();
            aggregate.AggregateRecord.Id.Should().Be(backlogItemId.ToString());
            aggregate.AggregateRecord.Name.Should().Be("BacklogItem");
            aggregate.AggregateRecord.Version.Should().Be(2);

            aggregate.EventRecords.Should().NotBeNull();
            aggregate.EventRecords.Should().HaveCount(0);
        }
        
        [Fact]
        public void AggregateWithDomainEvents_ShouldHaveCorrectValues()
        {
            // Given
            var backlogItemId = new BacklogItemId();
            var aggregateRoot = BacklogItem.FromSummary(backlogItemId, "summary");
            aggregateRoot.AddTask("title");
            
            // When
            var aggregate = new Aggregate(aggregateRoot);
            
            // Then
            aggregate.AggregateRecord.Should().NotBeNull();
            aggregate.AggregateRecord.Id.Should().Be(backlogItemId.ToString());
            aggregate.AggregateRecord.Name.Should().Be("BacklogItem");
            aggregate.AggregateRecord.Version.Should().Be(0);

            aggregate.EventRecords.Should().NotBeNull();
            aggregate.EventRecords.Should().HaveCount(2);
            aggregate.EventRecords.Should().ContainSingle(eventRecord => eventRecord.Event is BacklogItemCreated);
            aggregate.EventRecords.Should().ContainSingle(eventRecord => eventRecord.Event is SubTaskAdded);
        }

        [Fact]
        public void ToStoredEvents_ShouldReturnCorrectValues()
        {
            // Given
            var backlogItemId = new BacklogItemId();
            var aggregateRoot = BacklogItem.FromSummary(backlogItemId, "summary");
            aggregateRoot.AddTask("title");
            var aggregate = new Aggregate(aggregateRoot);
            
            // When
            var storedEvents = aggregate.ToStoredEvents(new JsonSerializerSettings()).ToList();
            
            // Then
            storedEvents.Should().HaveCount(2);
            storedEvents.Should().ContainSingle(storedEvent =>
                storedEvent.AggregateId == backlogItemId.ToString() &&
                storedEvent.Aggregate == nameof(BacklogItem) &&
                storedEvent.Version == 1 &&
                storedEvent.Name == nameof(BacklogItemCreated));
            storedEvents.Should().ContainSingle(storedEvent =>
                storedEvent.AggregateId == backlogItemId.ToString() &&
                storedEvent.Aggregate == nameof(BacklogItem) &&
                storedEvent.Version == 2 &&
                storedEvent.Name == nameof(SubTaskAdded));
        }
    }
}