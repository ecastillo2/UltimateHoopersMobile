using ApiClient;
using ApiClient.Interfaces;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SocialMedia.Api.Client
{
    public class PostApi : IPostApi
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PostApi> _logger;

        public PostApi(HttpClient httpClient, ILogger<PostApi> logger,IConfiguration configuration)
        {
            _logger = logger;

            _httpClient = httpClient;

            // Create an instance of WebApiService
            var webApiService = new WebApiService(httpClient, configuration);

            // Set the BaseAddress from WebApiService's HttpClient
            _httpClient.BaseAddress = webApiService.GetClient().BaseAddress;
        }

        /// <summary>
        /// Get Posts
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<Post>> GetPosts(string timeZone, string token)
        {

            List<Post> modelList = new List<Post>();

            try
            {
                string urlParameters = "?timeZone=" + timeZone;

                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.GetAsync("api/Post/GetPosts" + urlParameters);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    modelList = JsonConvert.DeserializeObject<List<Post>>(responseString);
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine(ex.Message);
                _logger.LogError($"Error occurred in Index method: {ex.Message}", ex);
            }

            return modelList;
        }


        public async Task<List<Post>> GetBlogs(string timeZone, string token)
        {
            List<Post> modelList = new List<Post>();

            try
            {
                string urlParameters = "?timeZone=" + timeZone;

                _httpClient.DefaultRequestHeaders.Accept.Clear();

                // Remove existing Authorization header if it already exists
                if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                {
                    _httpClient.DefaultRequestHeaders.Remove("Authorization");
                }

                _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.GetAsync("api/Post/GetBlogs" + urlParameters);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    modelList = JsonConvert.DeserializeObject<List<Post>>(responseString);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _logger.LogError($"Error occurred in GetBlogs method: {ex.Message}", ex);
            }

            return modelList;
        }

        /// <summary>
        /// Get Posts
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<Post>> GetNews(string timeZone, string token)
        {

            List<Post> modelList = new List<Post>();

            try
            {
                string urlParameters = "?timeZone=" + timeZone;

                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.GetAsync("api/Post/GetNews" + urlParameters);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    modelList = JsonConvert.DeserializeObject<List<Post>>(responseString);
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine(ex.Message);
                _logger.LogError($"Error occurred in Index method: {ex.Message}", ex);
            }

            return modelList;
        }


        /// <summary>
        /// Get Posts
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<List<Post>> GetPublicPosts(string token)
        {
           
            List<Post> modelList = new List<Post>();
            

           
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/Post/GetPublicPosts");
                    var responseString = await response.Content.ReadAsStringAsync();


                    if (response.IsSuccessStatusCode)
                    {
                        modelList = JsonConvert.DeserializeObject<List<Post>>(responseString);

                    }
                }

                catch (Exception ex)
                {
                    var x = ex;
                }

            }

            return modelList;

        }

        /// <summary>
        /// Get Posts
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<List<Post>> GetPostsByProfileId(string ProfileId, string timeZone, string token)
        {
           
            List<Post> obj = new List<Post>();
            string urlParameters = "?profileId=" + ProfileId;
            string urlParameters2 = "&timeZone=" + timeZone;

            
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();            
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/Post/GetPostsByProfileId" + urlParameters + urlParameters2);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {

                        obj = JsonConvert.DeserializeObject<List<Post>>(responseString);


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
        /// Get Posts
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<List<Post>> GetSavedPostsByProfileId(string ProfileId, string timeZone, string token)
        {
            WebApi _api = new WebApi();
            List<Post> obj = new List<Post>();
            string urlParameters = "?profileId=" + ProfileId;
            string urlParameters2 = "&timeZone=" + timeZone;

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/Post/GetSavedPostsByProfileId" + urlParameters + urlParameters2);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {

                        obj = JsonConvert.DeserializeObject<List<Post>>(responseString);


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
        /// Get Posts
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<Post>> GetPostsMentionProfileId(string ProfileId, string timeZone, string token)
        {
          
            List<Post> obj = new List<Post>();
            

           
            
                string urlParameters = "?profileId=" + ProfileId;
                string urlParameters2 = "&timeZone=" + timeZone;

                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await _httpClient.GetAsync("api/Post/GetPostsMentionProfileId" + urlParameters + urlParameters2);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {

                        obj = JsonConvert.DeserializeObject<List<Post>>(responseString);


                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    _logger.LogError($"Error occurred in Index method: {ex.Message}", ex);
                }

            
            return obj;

        }

        /// <summary>
        /// Get Posts
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<List<Post>> GetPostsWithTagByTagId(string TagId, string timeZone, string token)
        {
            WebApi _api = new WebApi();
            List<Post> obj = new List<Post>();
            string urlParameters = "?tagId=" + TagId;
            string urlParameters2 = "&timeZone=" + timeZone;

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/Post/GetPostsWithTagByTagId" + urlParameters + urlParameters2);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {

                        obj = JsonConvert.DeserializeObject<List<Post>>(responseString);


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
        /// Create Post
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public  async Task<bool> CreatePost(Post obj, string token)
        {


            

                var userJsonString = JsonConvert.SerializeObject(obj);

                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpContent content = new StringContent(userJsonString, Encoding.UTF8, "application/json");
               
                try
                {
                    var response = await _httpClient.PostAsync("api/Post/CreatePost/", content);

                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    _logger.LogError($"Error occurred in Index method: {ex.Message}", ex);
                }

            
            return true;
        }

        /// <summary>
        /// Get Post By Id
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public  async Task<Post> GetPostById(string postId, string timeZone, string token)
        {

            Post obj = new Post();
            string urlParameters = "?postId=" + postId;
            string urlParameters2 = "&timeZone=" + timeZone;

           
            

                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await _httpClient.GetAsync("api/Post/GetPostById" + urlParameters + urlParameters2);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {

                        obj = JsonConvert.DeserializeObject<Post>(responseString);


                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    _logger.LogError($"Error occurred in Index method: {ex.Message}", ex);
                }

            
            return obj;
        }



        /// <summary>
        /// Update Post
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public  async Task<bool> UpdatePost(Post obj, string token)
        {
            
            var userJsonString = JsonConvert.SerializeObject(obj);

         
            

                
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpContent content = new StringContent(userJsonString, Encoding.UTF8, "application/json");
                try
                {
                    var response = await _httpClient.PostAsync("api/Post/UpdatePost", content);
                    var responseString = await response.Content.ReadAsStringAsync();

                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    _logger.LogError($"Error occurred in Index method: {ex.Message}", ex);
                }

                return true;
            

        }


        /// <summary>
        /// Deletes a post and returns a JSON result indicating success or failure.
        /// </summary>
        /// <param name="postId">The ID of the post to delete.</param>
        /// <param name="token">The authorization token.</param>
        /// <returns>A JSON response indicating success or failure.</returns>
        public static async Task<JsonResult> DeletePost(string postId, string token)
        {
         

            string urlParameters = "?postId=" + postId;
          

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    // Send the DELETE request
                    var response = await client.DeleteAsync("api/Post/DeletePost" + urlParameters);

                    // Check if the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Return success response
                        return new JsonResult(new { success = true, message = "Post deleted successfully." });
                    }
                    else
                    {
                        // Return failure response with error message
                        string responseString = await response.Content.ReadAsStringAsync();
                        return new JsonResult(new { success = false, message = "Failed to delete the post. " + responseString });
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception and return failure response
                    Console.WriteLine($"Error deleting post: {ex.Message}");
                    return new JsonResult(new { success = false, message = "An error occurred while deleting the post." });
                }
            }
        }



        /// <summary>
        /// Delete Post
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task UpdatePostStatus(string postId, string status, string token)
        {
         

            string urlParameters = "?postId=" + postId;
            string urlParameters2 = "&status=" + status;

            
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/Post/UpdatePostStatus" + urlParameters + urlParameters2);
                    var responseString = await response.Content.ReadAsStringAsync();
                    string responseUri = response.RequestMessage.RequestUri.ToString();

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
