using System;

namespace Tacta.EventStore.Repository.Exceptions
{
    public sealed class InvalidSequenceException : Exception
    {
        public InvalidSequenceException(string message) : base(message)
        {
            
        }
    }
}
