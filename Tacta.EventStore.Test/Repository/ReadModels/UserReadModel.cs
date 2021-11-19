using System;

namespace Tacta.EventStore.Test.Repository.ReadModels
{
    public class UserReadModel
    {
        public Guid Id { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Sequence { get; set; }
        public Guid EventId { get; set; }
        public string Name { get; set; }
    }
}