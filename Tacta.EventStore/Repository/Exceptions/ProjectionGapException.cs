using System;

namespace Tacta.EventStore.Repository.Exceptions
{
    /// <summary>
    /// Represents an exception that is thrown when a gap is detected in the projection process,
    /// indicating that some events have not been projected and consistency may be compromised.
    /// </summary>
    public sealed class ProjectionGapException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectionGapException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ProjectionGapException(string message) : base(message)
        {
        }
    }
}
