using System;

namespace Tacta.EventStore.Repository.Exceptions
{
    public sealed class InvalidAggregateIdException : Exception
    {
        public InvalidAggregateIdException(string message) : base(message)
        {
            
        }
    }
}
