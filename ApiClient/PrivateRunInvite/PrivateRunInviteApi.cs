using Domain;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace ApiClient
{
    public static class PrivateRunInviteApi
    {
        static WebApi _api = new WebApi();

        /// <summary>
        /// Get SavedPosts
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<List<PrivateRunInvite>> GetPrivateRunInvites(string token)
        {
            WebApi _api = new WebApi();
            List<PrivateRunInvite> modelList = new List<PrivateRunInvite>();

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/PrivateRunInvite/GetPrivateRunInvites/");
                    var responseString = await response.Content.ReadAsStringAsync();


                    if (response.IsSuccessStatusCode)
                    {
                        modelList = JsonConvert.DeserializeObject<List<PrivateRunInvite>>(responseString);

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
        /// Create SavedPost
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<bool> CreatePrivateRunInvite(PrivateRunInvite obj, string token)
        {

            var userJsonString = JsonConvert.SerializeObject(obj);
            var clientBaseAddress = _api.Intial();

            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpContent content = new StringContent(userJsonString, Encoding.UTF8, "application/json");
               
                try
                {
                    var response = await client.PostAsync("api/PrivateRunInvite/CreatePrivateRunInvite/", content);

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
        public static async Task<PrivateRunInvite> GetPrivateRunInviteById(string privateRunInviteId, string token)
        {

            PrivateRunInvite obj = new PrivateRunInvite();
            string urlParameters = "?privateRunInviteId=" + privateRunInviteId;

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer "+ token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
               
                try
                {
                    var response = await client.GetAsync("api/PrivateRunInvite/GetPrivateRunInviteById" + urlParameters);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {

                        obj = JsonConvert.DeserializeObject<PrivateRunInvite>(responseString);


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
        /// Updates the invite status for a player in a private run.
        /// </summary>
        /// <param name="ProfileId">The profile ID of the player whose invite status is being updated.</param>
        /// <param name="PrivateRunId">The ID of the private run.</param>
        /// <param name="AcceptedInvite">The updated status of the invite (Accepted, Declined, Undecided, etc.).</param>
        /// <param name="token">The authentication token for the request.</param>
        /// <returns>A JsonResult indicating success or failure of the operation.</returns>
        public static async Task<JsonResult> UpdatePlayerPrivateRunInvite(string ProfileId, string PrivateRunInviteId, string AcceptedInvite, string token)
        {
            PrivateRunInvite obj = new PrivateRunInvite();
            string urlParameters = "?profileId=" + ProfileId;
            string urlParameters2 = "&privateRunInviteId=" + PrivateRunInviteId;
            string urlParameters3 = "&acceptedInvite=" + AcceptedInvite;

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    // Construct the full URL with query parameters
                    var fullUrl = "api/PrivateRunInvite/UpdatePlayerPrivateRunInvite" + urlParameters + urlParameters2 + urlParameters3;

                    // Send GET request to update the invite status
                    var response = await client.GetAsync(fullUrl);
                    var responseString = await response.Content.ReadAsStringAsync();

                    // Check if the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Deserialize the response JSON into an object (if needed)
                        obj = JsonConvert.DeserializeObject<PrivateRunInvite>(responseString);

                        // Return a JsonResult with success
                        return new JsonResult(new { success = true, message = "Invite updated successfully" });
                    }
                    else
                    {
                        // Log the error message if the response is not successful
                        Console.WriteLine($"Error: {response.StatusCode}, {responseString}");

                        // Return a JsonResult with failure status
                        return new JsonResult(new { success = false, message = $"Error: {response.StatusCode} - {responseString}" });
                    }
                }
                catch (Exception ex)
                {
                    // Log any exceptions that occur during the HTTP request
                    Console.WriteLine($"Exception occurred: {ex.Message}");

                    // Return a JsonResult with failure status
                    return new JsonResult(new { success = false, message = "Exception occurred: " + ex.Message });
                }
            }
        }



        /// <summary>
        /// Get SavedPost By Id
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<List<PrivateRunInvite>> GetPrivateRunInvitesByProfileId(string ProfileId, string token)
        {

            List<PrivateRunInvite> modelList = new List<PrivateRunInvite>();
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
                    var response = await client.GetAsync("api/PrivateRunInvite/GetPrivateRunInvitesByProfileId" + urlParameters);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {

                        modelList = JsonConvert.DeserializeObject<List<PrivateRunInvite>>(responseString);


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
        public static async Task<bool> UpdatePrivateRunInvite(PrivateRunInvite obj, string token)
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
                    var response = await client.PostAsync("api/PrivateRunInvite/UpdatePrivateRunInvite", content);
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
        public static async Task DeletePrivateRunInvite(string privateRunInviteId, string token)
        {
            WebApi _api = new WebApi();
            
            string urlParameters = "?privateRunInviteId=" + privateRunInviteId;

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.DeleteAsync("api/PrivateRunInvite/DeletePrivateRunInvite" + urlParameters);
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

        /// <summary>
        /// Get SavedPost By Id
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task RemoveProfileFromPrivateRun(string ProfileId, string PrivateRunId,  string token)
        {

            PrivateRunInvite obj = new PrivateRunInvite();
            string urlParameters = "?profileId=" + ProfileId;
            string urlParameters2 = "&privateRunId=" + PrivateRunId;
          

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/PrivateRunInvite/RemoveProfileFromPrivateRun" + urlParameters + urlParameters2 );
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {

                        obj = JsonConvert.DeserializeObject<PrivateRunInvite>(responseString);


                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());

                }

            }
            ;
        }


        /// <summary>
        /// Get SavedPost By Id
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task ClearPrivateRunInviteByPrivateRun(string PrivateRunId,  string token)
        {

            PrivateRunInvite obj = new PrivateRunInvite();
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
                    var response = await client.GetAsync("api/PrivateRunInvite/ClearPrivateRunInviteByPrivateRun" + urlParameters );
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {

                        obj = JsonConvert.DeserializeObject<PrivateRunInvite>(responseString);


                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());

                }

            }
            ;
        }


        /// <summary>
        /// Is EmailAvailable
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static async Task<bool> IsProfileIdIdAlreadyInvitedToRunInPrivateRunInvites(string profileId, string privateRunId, string token)
        {
            string isAvailable = string.Empty;

            WebApi _api = new WebApi();
            User _user = new User();
            string urlParameters = "?profileId=" + profileId;
            string urlParametersTwo = "&privateRunId=" + privateRunId;

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/PrivateRunInvite/IsProfileIdIdAlreadyInvitedToRunInPrivateRunInvites" + urlParameters + urlParametersTwo);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {

                        isAvailable = responseString.ToString();


                    }
                }

                catch (Exception ex)
                {
                    var x = ex;

                }

                return Convert.ToBoolean(isAvailable);
            }

        }


        /// <summary>
        /// Get SavedPost By Id
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<bool> GetPrivateRunsByProfileId(string ProfileId,string PrivateRun, string token)
        {
            string isAvailable = string.Empty;
           
            string urlParameters = "?profileId=" + ProfileId;
            string urlParametersTwo = "?privateRunId=" + PrivateRun;

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/PrivateRunInvite/GetPrivateRunsByProfileId" + urlParameters + urlParametersTwo);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {

                        isAvailable = responseString.ToString();


                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());

                }

            }
            return Convert.ToBoolean(isAvailable);
        }
    }
}
