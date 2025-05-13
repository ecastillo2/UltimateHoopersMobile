
namespace ApiClient
{
    public class WebApi
    {
 
        public HttpClient Intial()
        {
            var Client = new HttpClient();

            //Production Enviornment
            Client.BaseAddress = new Uri("https://ultimatehoopersapi.azurewebsites.net/");


            //Local
           //Client.BaseAddress = new Uri("https://localhost:44314/");


            ///swagger/index.html

            return Client;
        }

       
    }
}
