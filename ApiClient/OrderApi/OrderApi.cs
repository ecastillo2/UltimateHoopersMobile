
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ApiClient
{
    public static class OrderApi
    {
        static WebApi _api = new WebApi();
        public static async Task<List<Order>> GetOrders(string token)
        {
            WebApi _api = new WebApi();
            List<Order> modelList = new List<Order>();
           
            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {
                client.Timeout =  TimeSpan.FromSeconds(300);
                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {

                        var response = await client.GetAsync("api/Order/GetOrders");
                        var responseString = await response.Content.ReadAsStringAsync();
                        string responseUri = response.RequestMessage.RequestUri.ToString();

                        if (response.IsSuccessStatusCode)
                        {
                            modelList = JsonConvert.DeserializeObject<List<Order>>(responseString);

                        }
                    
                }

                catch (Exception ex)
                {
                    var x = ex;
                }

            }

            return modelList;

        }
        public static async Task<Order> GetActiveOrders(string token)
        {
            WebApi _api = new WebApi();
            Order modelList = new Order();

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(1000000);
                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {


                    var response = await client.GetAsync("api/Order/GetActiveOrders/");
                    var responseString = await response.Content.ReadAsStringAsync();
                    string responseUri = response.RequestMessage.RequestUri.ToString();

                    if (response.IsSuccessStatusCode)
                    {
                        modelList = JsonConvert.DeserializeObject<Order>(responseString);

                    }

                }

                catch (Exception ex)
                {
                    var x = ex;
                }

            }

            return modelList;

        }
        public static async Task<int> GetOrderCount(string token)
        {
            WebApi _api = new WebApi();
            int bundleList = 0;

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/Order/GetOrderCount/");
                    var responseString = await response.Content.ReadAsStringAsync();
                    string responseUri = response.RequestMessage.RequestUri.ToString();

                    if (response.IsSuccessStatusCode)
                    {

                        bundleList = JsonConvert.DeserializeObject<int>(responseString);


                    }
                }

                catch (Exception ex)
                {
                    var x = ex;
                }

            }

            return bundleList;

        }
		public static async Task CreateOrder(Order order, string token)
		{

			var userJsonString = JsonConvert.SerializeObject(order);
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
					var response = await client.PostAsync("api/Order/CreateOrder/", content);
					var responseString = await response.Content.ReadAsStringAsync();
					string responseUri = response.RequestMessage.RequestUri.ToString();
				}

				catch (Exception ex)
				{
					var x = ex;

				}

			}

		}
		public static async Task CreateSizeOption(Order order, string token)
        {
       
            var userJsonString = JsonConvert.SerializeObject(order);
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
                    var response = await client.PostAsync("api/Order/CreateOrder/", content);
                    var responseString = await response.Content.ReadAsStringAsync();
                    string responseUri = response.RequestMessage.RequestUri.ToString();
                }

                catch (Exception ex)
                {
                    var x = ex;

                }

            }

        }
        public static async Task<Order> GetOrderById(string id, string token)
        {

            Order model = new Order();
            string urlParameters = "?id=" + id;
            var clientBaseAddress = _api.Intial();

            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer "+ token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
               
                try
                {
                    var response = await client.GetAsync("api/Order/GetOrderById" + urlParameters);
                    var responseString = await response.Content.ReadAsStringAsync();
                    string responseUri = response.RequestMessage.RequestUri.ToString();

                    if (response.IsSuccessStatusCode)
                    {
                        model = JsonConvert.DeserializeObject<Order>(responseString);

                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());

                }

            }
            return model;
        }

        public static async Task OrderUpdateStatus(string id, string status, string token)
        {

            Order model = new Order();
            string urlParameters = "?id=" + id;
            string urlParametersTwo = "&status=" + status;
            var clientBaseAddress = _api.Intial();

            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/Order/OrderUpdateStatus" + urlParameters + urlParametersTwo);
                    var responseString = await response.Content.ReadAsStringAsync();
                    string responseUri = response.RequestMessage.RequestUri.ToString();

                   
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());

                }

            }
            
        }
        public static async Task OrderUpdateOrderStatus(string orderId, string status, string token)
        {

            Order model = new Order();
            string urlParameters = "?orderId=" + orderId;
            string urlParametersTwo = "&status=" + status;
            var clientBaseAddress = _api.Intial();

            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/Order/OrderUpdateOrderStatus" + urlParameters + urlParametersTwo);
                    var responseString = await response.Content.ReadAsStringAsync();
                    string responseUri = response.RequestMessage.RequestUri.ToString();


                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());

                }

            }

        }
        public static async Task<bool> UpdateOrder(Order order, string token)
        {
            
            var userJsonString = JsonConvert.SerializeObject(order);
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
                    var response = await client.PostAsync("api/Order/UpdateOrder", content);
                    var responseString = await response.Content.ReadAsStringAsync();
                    string responseUri = response.RequestMessage.RequestUri.ToString();

                }

                catch (Exception ex)
                {
                    var x = ex;

                }

            }

            return true;

        }
        public static async Task DeleteOrder(string orderId, string token)
        {
            WebApi _api = new WebApi();
            string urlParameters = "?orderId=" + orderId;

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.DeleteAsync("api/Order/DeleteOrder" + urlParameters);
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
		public static async Task<int> GenerateNewOrderNumber(string token)
		{

			int model = 0;

			var clientBaseAddress = _api.Intial();

			using (var client = new HttpClient())
			{

				client.DefaultRequestHeaders.Accept.Clear();
				client.BaseAddress = clientBaseAddress.BaseAddress;
				client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				try
				{
					var response = await client.GetAsync("api/Order/GenerateNewOrderNumber");
					var responseString = await response.Content.ReadAsStringAsync();
					string responseUri = response.RequestMessage.RequestUri.ToString();

					if (response.IsSuccessStatusCode)
					{
						model = JsonConvert.DeserializeObject<int>(responseString);

					}
				}

				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());

				}

			}
			return model;
		}
		public static async Task<List<Order>> GetOrderByProfileId(string profileId,  string token)
        {

            WebApi _api = new WebApi();
            List<Order> modelList = new List<Order>();
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
                    var response = await client.GetAsync("api/Order/GetOrderByProfileId" + urlParameters );
                    var responseString = await response.Content.ReadAsStringAsync();
                    string responseUri = response.RequestMessage.RequestUri.ToString();


                    if (response.IsSuccessStatusCode)
                    {
                        modelList = JsonConvert.DeserializeObject<List<Order>>(responseString);

                    }
                }

                catch (Exception ex)
                {
                    var x = ex;
                }

            }

            return modelList;
        }


        public static async Task<List<Order>> GetOrderByOrderNumber(string OrderNumber, string OrgId, string token)
        {

            WebApi _api = new WebApi();
            List<Order> modelList = new List<Order>();
            string urlParameters = "?orderNumber=" + OrderNumber;
            string urlParametersThree = "&orgId=" + OrgId;

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/Order/GetOrderByOrderNumber" + urlParameters + urlParametersThree);
                    var responseString = await response.Content.ReadAsStringAsync();
                    string responseUri = response.RequestMessage.RequestUri.ToString();


                    if (response.IsSuccessStatusCode)
                    {
                        modelList = JsonConvert.DeserializeObject<List<Order>>(responseString);

                    }
                }

                catch (Exception ex)
                {
                    var x = ex;
                }

            }

            return modelList;
        }
        public static async Task<Order> GetOrderByCustomerId(string customerId, string token)
        {

            Order seller = new Order();
            string urlParameters = "?customerId=" + customerId;

            var clientBaseAddress = _api.Intial();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync("api/Order/GetOrderByCustomerId" + urlParameters);
                    var responseString = await response.Content.ReadAsStringAsync();
                    string responseUri = response.RequestMessage.RequestUri.ToString();
                    if (response.IsSuccessStatusCode)
                    {

                        seller = JsonConvert.DeserializeObject<Order>(responseString);


                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());

                }

            }
            return seller;
        }
        public static async Task  RemoveItemFromCart(string OrderId,string bundleId,string token)
        {

            Order model = new Order();
            string urlParameters = "?orderId=" + OrderId;
            string urlParametersThree = "&bundleId=" + bundleId;
            var clientBaseAddress = _api.Intial();

            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Clear();
                client.BaseAddress = clientBaseAddress.BaseAddress;
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.DeleteAsync("api/Order/RemoveItemFromCart" + urlParameters +  urlParametersThree);
                    var responseString = await response.Content.ReadAsStringAsync();
                    string responseUri = response.RequestMessage.RequestUri.ToString();

                    if (response.IsSuccessStatusCode)
                    {
                        model = JsonConvert.DeserializeObject<Order>(responseString);

                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());

                }

            }
            
        }
    }
}
