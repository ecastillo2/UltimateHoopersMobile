using Newtonsoft.Json;
using SocialMedia.Domain;
using System.Net.Http.Headers;
using System.Text;

namespace ApiClient
{
    public static class ProductApi
    {
        static WebApi _api = new WebApi();

        /// <summary>
        /// Fetches the list of products from the API.
        /// </summary>
        /// <param name="token">The authentication token used for API authorization.</param>
        /// <returns>A list of products or an empty list if an error occurs.</returns>
        public static async Task<List<Product>> GetProducts(string token)
        {
            WebApi _api = new WebApi(); // Initialize the API instance.
            List<Product> modelList = new List<Product>(); // Initialize an empty list for products.

            var clientBaseAddress = _api.Intial(); // Get the base address for the API.

            using (var client = new HttpClient())
            {
                // Set up the HTTP client with the necessary headers.
                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token); // Add the Bearer token for authentication.
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); // Accept JSON response.

                try
                {
                    // Make the GET request to fetch products.
                    var response = await client.GetAsync("api/Product/GetProducts/");

                    // Read the response content as a string.
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        // If the request is successful, deserialize the response into a list of products.
                        modelList = JsonConvert.DeserializeObject<List<Product>>(responseString);
                    }
                    else
                    {
                        // If the response status code indicates failure, log the response code and message.
                        // You can replace this with logging or other error handling mechanisms.
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception for debugging purposes.
                    // You can replace this with your preferred logging mechanism.
                    Console.WriteLine($"Exception: {ex.Message} - {ex.StackTrace}");
                }
            }

            // Return the list of products (empty if no products were fetched or if an error occurred).
            return modelList;
        }

        /// <summary>
        /// Create SavedPost
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<bool> CreateProduct(Product obj, string token)
        {

            var userJsonString = JsonConvert.SerializeObject(obj);
            var clientBaseAddress = _api.Intial();

            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer" + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpContent content = new StringContent(userJsonString, Encoding.UTF8, "application/json");
               
                try
                {
                    var response = await client.PostAsync("api/Product/CreateProduct/", content);

                }

                catch (Exception ex)
                {
                    var x = ex;

                }

            }
            return true;
        }

        /// <summary>
        /// Get SavedPost By Id
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<Product> GetProductById(string productId, string token)
        {

            Product obj = new Product();
            string urlParameters = "?productId=" + productId;

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer "+ token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
               
                try
                {
                    var response = await client.GetAsync("api/Product/GetProductById" + urlParameters);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {

                        obj = JsonConvert.DeserializeObject<Product>(responseString);


                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());

                }

            }
            return obj;
        }



        /// <summary>
        /// Update SavedPost
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateProduct(Product obj, string token)
        {
            
            var userJsonString = JsonConvert.SerializeObject(obj);

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer "+ token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpContent content = new StringContent(userJsonString, Encoding.UTF8, "application/json");
                try
                {
                    var response = await client.PostAsync("api/Product/UpdateProduct", content);
                    var responseString = await response.Content.ReadAsStringAsync();

                }

                catch (Exception ex)
                {
                    var x = ex;

                }

                return true;
            }

        }


        /// <summary>
        /// Delete SavedPost
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task DeleteProduct(string productId, string token)
        {
            WebApi _api = new WebApi();
            
            string urlParameters = "?productId=" + productId;

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.DeleteAsync("api/Product/DeleteProduct" + urlParameters);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {

                    }
                }

                catch (Exception ex)
                {
                    var x = ex;

                }

            }
          
        }

       
    }
}
