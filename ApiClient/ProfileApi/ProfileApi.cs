using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SocialMedia.Domain;
using System.Net.Http.Headers;
using System.Text;

namespace ApiClient
{
    public static class ProfileApi
    {
        static WebApi _api = new WebApi();

        /// <summary>
        /// GetProfiles
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<List<Profile>> GetProfiles(string token)
        {
            WebApi _api = new WebApi();
            List<Profile> modelList = new List<Profile>();

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/Profile/GetProfiles/");
                    var responseString = await response.Content.ReadAsStringAsync();


                    if (response.IsSuccessStatusCode)
                    {
                        modelList = JsonConvert.DeserializeObject<List<Profile>>(responseString);

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
        /// Get Profile ById
        /// </summary>
        /// <param name="profileId">The ID of the profile to fetch.</param>
        /// <param name="token">The authorization token for the API request.</param>
        /// <returns>Returns a Profile object or error message.</returns>
        public static async Task<Profile> GetProfileById(string profileId, string token)
        {
            Profile obj = null;
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
                    // Make the GET request to the API
                    var response = await client.GetAsync("api/Profile/GetProfileById" + urlParameters);
                    var responseString = await response.Content.ReadAsStringAsync();

                    // Check if the API call was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Deserialize the response into the Profile object
                        obj = JsonConvert.DeserializeObject<Profile>(responseString);
                        return obj;  // Return the Profile object
                    }
                    else
                    {
                        // Log or handle the case where the API fails
                        Console.WriteLine($"Error: {responseString}");
                        return null; // Or handle as needed
                    }
                }
                catch (Exception ex)
                {
                    // Handle the exception (log it or return a message)
                    Console.WriteLine(ex.ToString());
                    return null; // Or return a default Profile if needed
                }
            }
        }



        /// <summary>
        /// Get FollowingProfilesByProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<List<Profile>> GetFollowingProfilesByProfileId(string profileId, string token)
        {
            WebApi _api = new WebApi();
            List<Profile> modelList = new List<Profile>();
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
                    var response = await client.GetAsync("api/Profile/GetFollowingProfilesByProfileId" + urlParameters);
                    var responseString = await response.Content.ReadAsStringAsync();


                    if (response.IsSuccessStatusCode)
                    {
                        modelList = JsonConvert.DeserializeObject<List<Profile>>(responseString);

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
        /// GetFollowerProfilesByProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<List<Profile>> GetFollowerProfilesByProfileId(string profileId, string token)
        {
            WebApi _api = new WebApi();
            List<Profile> modelList = new List<Profile>();
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
                    var response = await client.GetAsync("api/Profile/GetFollowerProfilesByProfileId" + urlParameters);
                    var responseString = await response.Content.ReadAsStringAsync();


                    if (response.IsSuccessStatusCode)
                    {
                        modelList = JsonConvert.DeserializeObject<List<Profile>>(responseString);

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
        /// Update Profile
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateProfile(Profile obj, string token)
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
                    var response = await client.PostAsync("api/Profile/UpdateProfile", content);
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
        /// Update Profile UserName
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateProfileUserName(Profile obj, string token)
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
                    var response = await client.PostAsync("api/Profile/UpdateProfileUserName", content);
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
        /// Is EmailAvailable
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static async Task<bool> IsUserNameAvailable(string userName)
        {
            string isAvailable = string.Empty;

            WebApi _api = new WebApi();
            User _user = new User();
            string urlParameters = "?userName=" + userName;

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer ");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/Profile/IsUserNameAvailable" + urlParameters);
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
        /// Is EmailAvailable
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static async Task<List<Game>> GetProfileGameHistory(string profileId, string token)
        {
            string isAvailable = string.Empty;

            WebApi _api = new WebApi();
            List<Game> modelList = new List<Game>();
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
                    var response = await client.GetAsync("api/Profile/GetProfileGameHistory" + urlParameters);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        modelList = JsonConvert.DeserializeObject<List<Game>>(responseString);

                    }
                }

                catch (Exception ex)
                {
                    var x = ex;

                }

                return modelList;
            }

        }


        /// <summary>
        /// Is EmailAvailable
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static async Task<bool> IsEmailAvailable(string email)
        {
            string isAvailable = string.Empty;

            WebApi _api = new WebApi();
            User _user = new User();
            string urlParameters = "?email=" + email;

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer ");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/Profile/IsEmailAvailable" + urlParameters);
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
        /// GetFollowerProfilesByProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task UpdateWinnerPoints(string profileId, string token)
        {
            WebApi _api = new WebApi();
            
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
                    var response = await client.GetAsync("api/Profile/UpdateWinnerPoints" + urlParameters);
                    var responseString = await response.Content.ReadAsStringAsync();


                    if (response.IsSuccessStatusCode)
                    {
                      //  modelList = JsonConvert.DeserializeObject<List<Profile>>(responseString);

                    }
                }

                catch (Exception ex)
                {
                    var x = ex;
                }

            }

         

        }


        /// <summary>
        /// GetFollowerProfilesByProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task UpdateLastRunDate(string profileId,string lastRunDate, string token)
        {
            WebApi _api = new WebApi();

            string urlParameters = "?profileId=" + profileId;
            string urlParameterTwo = "&lastRunDate=" + lastRunDate;

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/Profile/UpdateLastRunDate" + urlParameters+ urlParameterTwo);
                    var responseString = await response.Content.ReadAsStringAsync();


                    if (response.IsSuccessStatusCode)
                    {
                        //  modelList = JsonConvert.DeserializeObject<List<Profile>>(responseString);

                    }
                }

                catch (Exception ex)
                {
                    var x = ex;
                }

            }



        }


        /// <summary>
        /// GetFollowerProfilesByProfileId
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task UpdateSetProfileWithBestRecord(string profileId, string token)
        {
            WebApi _api = new WebApi();

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
                    var response = await client.GetAsync("api/Profile/UpdateSetProfileWithBestRecord" + urlParameters);
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
        /// UpdateSetProfileWithBestRecordToFalse
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task UpdateSetProfileWithBestRecordToFalse(string profileId, string token)
        {
            WebApi _api = new WebApi();

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
                    var response = await client.GetAsync("api/Profile/UpdateSetProfileWithBestRecordToFalse" + urlParameters);
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
