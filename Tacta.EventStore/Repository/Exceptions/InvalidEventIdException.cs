using System;

namespace Tacta.EventStore.Repository.Exceptions
{
    public sealed class InvalidEventIdException : Exception
    {
        public InvalidEventIdException(string message) : base(message)
        {
            
        }
    }
}
