using Domain;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace ApiClient
{
    /// <summary>
    /// Authenticate User
    /// </summary>
    public static class AuthenticateUser
    {
        static WebApi _api = new WebApi();

        /// <summary>
        /// Authenticate Users
        /// </summary>
        /// <param name="Email"></param>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        public static async Task<User> AuthenticateUsers(string AuthToken, string Email, string Username, string Password)
        {
            User user = new User();

            var values = new User();
            values.AuthToken = AuthToken;
            values.Email = Email;
            values.Password = Password;

            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(values);

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    HttpContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("api/Authentication/Authenticate", content);

                    var responseString = await response.Content.ReadAsStringAsync();
                    string responseUri = response.RequestMessage.RequestUri.ToString();

                    if (response.IsSuccessStatusCode)
                    {
                        user = JsonConvert.DeserializeObject<User>(responseString.ToString());
                    }
                }

                catch (Exception ex)
                {
                    var x = ex;
                }

            }

            return user;
        }


        /// <summary>
        /// Authenticate Users
        /// </summary>
        /// <param name="Email"></param>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        public static async Task<PrivateRun> AuthenticatePrivateRun(string PrivateRunNumber,  string Password)
        {
            PrivateRun user = new PrivateRun();

            var values = new PrivateRun();
            values.PrivateRunNumber = PrivateRunNumber;
            values.Password = Password;

            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(values);

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    HttpContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("api/Authentication/SocialMediaPrivateRunAuthenticate", content);

                    var responseString = await response.Content.ReadAsStringAsync();
                    string responseUri = response.RequestMessage.RequestUri.ToString();

                    if (response.IsSuccessStatusCode)
                    {
                        user = JsonConvert.DeserializeObject<PrivateRun>(responseString.ToString());
                    }
                }

                catch (Exception ex)
                {
                    var x = ex;
                }

            }

            return user;
        }

    }
}
