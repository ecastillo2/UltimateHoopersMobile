using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Api.Client.Helper
{
    public class WebApiService
    {
        private readonly HttpClient _httpClient;

        public WebApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            // Get the base address from configuration
            var baseAddress = configuration["ApiSettings:BaseAddress"];

            // Ensure the base address is valid
            if (!Uri.IsWellFormedUriString(baseAddress, UriKind.Absolute))
            {
                throw new UriFormatException($"The base address '{baseAddress}' is not a valid URI.");
            }

            // Set the BaseAddress for HttpClient
            _httpClient.BaseAddress = new Uri(baseAddress);
        }

        public HttpClient GetClient()
        {
            return _httpClient;
        }
}

      
    
}
