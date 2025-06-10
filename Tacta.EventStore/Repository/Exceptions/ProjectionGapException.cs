using System;

namespace Tacta.EventStore.Repository.Exceptions
{
    public sealed class ProjectionGapException : Exception
    {
        public ProjectionGapException(string message) : base(message)
        {
        }
    }
}
