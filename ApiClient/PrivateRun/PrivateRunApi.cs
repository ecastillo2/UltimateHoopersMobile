using Domain;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace ApiClient
{
    public static class PrivateRunApi
    {
        static WebApi _api = new WebApi();

        /// <summary>
        /// Get SavedPosts
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<List<PrivateRun>> GetPrivateRuns(string token)
        {
            WebApi _api = new WebApi();
            List<PrivateRun> modelList = new List<PrivateRun>();

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/PrivateRun/GetPrivateRuns/");
                    var responseString = await response.Content.ReadAsStringAsync();


                    if (response.IsSuccessStatusCode)
                    {
                        modelList = JsonConvert.DeserializeObject<List<PrivateRun>>(responseString);

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
        /// Creates a private run with the provided data.
        /// </summary>
        /// <param name="obj">The private run object to be created.</param>
        /// <param name="token">The authentication token for the API request.</param>
        /// <returns>Returns a JsonResult indicating success or failure.</returns>
        [HttpPost]
        public static async Task<JsonResult> CreatePrivateRun(PrivateRun obj, string token)
        {
            var userJsonString = JsonConvert.SerializeObject(obj);
            var clientBaseAddress = _api.Intial();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);  // Fixed the missing space after 'Bearer'
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpContent content = new StringContent(userJsonString, Encoding.UTF8, "application/json");

                try
                {
                    // Perform the POST request
                    var response = await client.PostAsync("api/PrivateRun/CreatePrivateRun/", content);

                    // Check if the response status code indicates success
                    if (response.IsSuccessStatusCode)
                    {
                        // Optionally, you can deserialize the response if you expect any data in return
                        var responseString = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<PrivateRun>(responseString);

                        // Return success as a JSON response
                        return new JsonResult(new { success = true, message = "Private run created successfully", data = result });
                    }
                    else
                    {
                        // Log or handle the error response as needed
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        return new JsonResult(new { success = false, message = $"Error creating private run: {response.StatusCode} - {errorResponse}" });
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception (you can replace this with a more sophisticated logging mechanism)
                    return new JsonResult(new { success = false, message = $"Exception occurred: {ex.Message}" });
                }
            }
        }



        /// <summary>
        /// Get SavedPost By Id
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<List<Profile>> GetProfilesByPrivateRunId(string PrivateRunId, string token)
        {

            List<Profile> modelList = new List<Profile>();
            string urlParameters = "?privateRunId=" + PrivateRunId;

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/PrivateRun/GetProfilesByPrivateRunId" + urlParameters);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {

                        modelList = JsonConvert.DeserializeObject<List<Profile>>(responseString);


                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());

                }

            }
            return modelList;
        }


        /// <summary>
        /// Get SavedPost By Id
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<PrivateRun> GetPrivateRunById(string privateRunId, string token)
        {

            PrivateRun obj = new PrivateRun();
            string urlParameters = "?privateRunId=" + privateRunId;

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer "+ token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
               
                try
                {
                    var response = await client.GetAsync("api/PrivateRun/GetPrivateRunById" + urlParameters);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {

                        obj = JsonConvert.DeserializeObject<PrivateRun>(responseString);


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
        /// Get SavedPost By Id
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<List<PrivateRun>> GetProfileInvitesByProfileId(string profileId, string token)
        {

            List<PrivateRun> modelList = new List<PrivateRun>();
            string urlParameters = "?profileId=" + profileId;
            

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/PrivateRun/GetProfileInvitesByProfileId" + urlParameters);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {

                        modelList = JsonConvert.DeserializeObject<List<PrivateRun>>(responseString);


                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());

                }

            }
            return modelList;
        }

        /// <summary>
        /// Get SavedPost By Id
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<List<PrivateRun>> GetPrivateRunsByProfileId(string ProfileId, string token)
        {

            List<PrivateRun> modelList = new List<PrivateRun>();
            string urlParameters = "?profileId=" + ProfileId;

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/PrivateRun/GetPrivateRunsByProfileId" + urlParameters);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {

                        modelList = JsonConvert.DeserializeObject<List<PrivateRun>>(responseString);


                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());

                }

            }
            return modelList;
        }


        



        /// <summary>
        /// Update SavedPost
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<bool> UpdatePrivateRun(PrivateRun obj, string token)
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
                    var response = await client.PostAsync("api/PrivateRun/UpdatePrivateRun", content);
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
        public static async Task RemovePrivateRun(string privateRunId, string token)
        {
            WebApi _api = new WebApi();
            
            string urlParameters = "?privateRunId=" + privateRunId;

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/PrivateRun/RemovePrivateRun" + urlParameters);
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
