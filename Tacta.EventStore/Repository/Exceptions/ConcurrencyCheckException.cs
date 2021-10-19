using System;

namespace Tacta.EventStore.Repository.Exceptions
{
    public sealed class ConcurrencyCheckException : Exception
    {
        public ConcurrencyCheckException(string message) : base(message)
        {
            
        }
    }
}
