using System;
using System.Net;

namespace ApiClient.Core
{
    /// <summary>
    /// Exception thrown when an API request fails
    /// </summary>
    public class ApiException : Exception
    {
        /// <summary>
        /// Gets the HTTP status code of the failed request
        /// </summary>
        public HttpStatusCode? StatusCode { get; }

        /// <summary>
        /// Gets the raw response content of the failed request
        /// </summary>
        public string ResponseContent { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiException"/> class
        /// </summary>
        /// <param name="message">The error message</param>
        public ApiException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiException"/> class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public ApiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiException"/> class
        /// </summary>
        /// <param name="statusCode">The HTTP status code</param>
        /// <param name="responseContent">The response content</param>
        public ApiException(HttpStatusCode statusCode, string responseContent)
            : base($"API request failed with status code {statusCode}: {responseContent}")
        {
            StatusCode = statusCode;
            ResponseContent = responseContent;
        }
    }
}