using System;

namespace Tacta.EventStore.Repository.Exceptions
{
    public sealed class InvalidAggregateRecordException : Exception
    {
        public InvalidAggregateRecordException(string message) : base(message)
        {
            
        }
    }
}
