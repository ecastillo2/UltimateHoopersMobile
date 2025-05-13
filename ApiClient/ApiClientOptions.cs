namespace ApiClient.Configuration
{
    /// <summary>
    /// Configuration options for the API client
    /// </summary>
    public class ApiClientOptions
    {
        /// <summary>
        /// The configuration section name for API client options
        /// </summary>
        public const string SectionName = "ApiClient";

        /// <summary>
        /// Gets or sets the base URL for the API
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the timeout in seconds for API requests
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;
    }
}