using Domain;
using Newtonsoft.Json;

using System.Net.Http.Headers;
using System.Text;

namespace ApiClient
{
    public static class CourtApi
    {
        static WebApi _api = new WebApi();

        /// <summary>
        /// GetC ourts
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<List<Court>> GetCourts(string token)
        {
            WebApi _api = new WebApi();
            List<Court> modelList = new List<Court>();

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/Court/GetCourts/");
                    var responseString = await response.Content.ReadAsStringAsync();


                    if (response.IsSuccessStatusCode)
                    {
                        modelList = JsonConvert.DeserializeObject<List<Court>>(responseString);

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
        /// Create Court
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<bool> CreateCourt(Court obj, string token)
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
                    var response = await client.PostAsync("api/Court/CreateCourt/", content);
                    var responseString = await response.Content.ReadAsStringAsync();


                }

                catch (Exception ex)
                {
                    var x = ex;

                }

            }
            return true;
        }

        /// <summary>
        /// Get Court ById
        /// </summary>
        /// <param name="CourtId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<Court> GetCourtById(string CourtId, string token)
        {

            Court obj = new Court();
            string urlParameters = "?courtId=" + CourtId;

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer "+ token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
               
                try
                {
                    var response = await client.GetAsync("api/Court/GetCourtById" + urlParameters);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        obj = JsonConvert.DeserializeObject<Court>(responseString);

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
        /// Update Court
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateCourt(Court obj, string token)
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
                    var response = await client.PostAsync("api/Court/UpdateCourt", content);
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
        /// Delete Court
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task DeleteCourt(string CourtId, string token)
        {
            WebApi _api = new WebApi();
            
            string urlParameters = "?courtId=" + CourtId;

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.DeleteAsync("api/Court/DeleteCourt" + urlParameters);
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
