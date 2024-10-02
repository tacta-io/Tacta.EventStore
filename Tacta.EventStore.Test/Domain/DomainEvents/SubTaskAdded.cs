using System;
using Newtonsoft.Json;
using Tacta.EventStore.Domain;
using Tacta.EventStore.Test.Domain.Identities;

namespace Tacta.EventStore.Test.Domain.DomainEvents
{
    public class SubTaskAdded : DomainEvent
    {
        public string Title { get; }

        public TaskId TaskId { get; }

        public SubTaskAdded(TaskId taskId, string title) : base(taskId.ToString())
        {
            CreatedAt = DateTime.Now;
            TaskId = taskId;
            Title = title;
        }
        
        [JsonConstructor]
        public SubTaskAdded(Guid id, string aggregateId, DateTime createdAt, TaskId taskId, string title) 
            : base(id, aggregateId, createdAt)
        {
            TaskId = taskId;
            Title = title;
        }
    }
}
