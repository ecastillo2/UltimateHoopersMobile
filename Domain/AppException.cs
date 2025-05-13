using System;

namespace Domain
{
    /// <summary>
    /// Custom application exception type for handling application-specific errors
    /// </summary>
    public class AppException : Exception
    {
        /// <summary>
        /// Creates a new instance of AppException
        /// </summary>
        public AppException() : base() { }

        /// <summary>
        /// Creates a new instance of AppException with a specified error message
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public AppException(string message) : base(message) { }

        /// <summary>
        /// Creates a new instance of AppException with a specified error message and a reference to the inner exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public AppException(string message, Exception innerException) : base(message, innerException) { }
    }
}