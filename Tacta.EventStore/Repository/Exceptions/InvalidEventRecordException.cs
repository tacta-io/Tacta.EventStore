using System;

namespace Tacta.EventStore.Repository.Exceptions
{
    public sealed class InvalidEventRecordException : Exception
    {
        public InvalidEventRecordException(string message) : base(message)
        {
            
        }
    }
}
