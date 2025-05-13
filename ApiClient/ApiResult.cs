namespace ApiClient
{
    /// <summary>
    /// Represents the result of an API operation
    /// </summary>
    public class ApiResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the message associated with the result
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets additional data associated with the result
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Creates a successful result
        /// </summary>
        /// <param name="message">Optional success message</param>
        /// <param name="data">Optional data</param>
        /// <returns>A successful result</returns>
        public static ApiResult Succeeded(string message = null, object data = null)
        {
            return new ApiResult
            {
                Success = true,
                Message = message ?? "Operation completed successfully",
                Data = data
            };
        }

        /// <summary>
        /// Creates a failed result
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="data">Optional data</param>
        /// <returns>A failed result</returns>
        public static ApiResult Failed(string message, object data = null)
        {
            return new ApiResult
            {
                Success = false,
                Message = message,
                Data = data
            };
        }
    }
}