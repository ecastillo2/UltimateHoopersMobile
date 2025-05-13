using Microsoft.AspNetCore.Mvc;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.Swagger;

namespace WebAPI.Controllers
{
    /// <summary>
    /// ColorOption Controller
    /// </summary>
    [Route("api/[controller]")]
    public class ErrorExceptionController : Controller
    {
        HttpResponseMessage returnMessage = new HttpResponseMessage();
        private IErrorExceptionRepository colorOptionRepository;


        /// <summary>
        /// Error Exception Controller
        /// </summary>
        /// <param name="colorOptionContext"></param>
        public ErrorExceptionController(ErrorExceptionContext colorOptionContext)
        {
         
            this.colorOptionRepository = new ErrorExceptionRepository(colorOptionContext);

        }

        /// <summary>
        /// Get Error Exceptions
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetErrorExceptions")]
        [Authorize]
        public async Task<List<ErrorException>> GetErrorExceptions()
        {

            return await colorOptionRepository.GetErrorExceptions();

        }

        /// <summary>
        /// Get Error Exception ById
        /// </summary>
        /// <param name="errorExceptionId"></param>
        /// <returns></returns>
        [HttpGet("GetErrorExceptionById")]
        [Authorize]
        public async Task<ErrorException> GetErrorExceptionById(string errorExceptionId)
        {
            try
            {
                return await colorOptionRepository.GetErrorExceptionById(errorExceptionId);
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Create Error Exception
        /// </summary>
        /// <param name="errorException"></param>
        /// <returns></returns>
        [HttpPost("CreateErrorException")]
		//[Authorize]
		public async Task CreateErrorException([FromBody] ErrorException errorException)
        {
           
            
            try
            {
               await colorOptionRepository.InsertErrorException(errorException);

                
            }
            catch (Exception ex)
            {
                var x = ex;
            }

        }


        /// <summary>
        /// Delete Error Exception
        /// </summary>
        /// <param name="errorExceptionId"></param>
        /// <returns></returns>
        [HttpDelete("DeleteErrorException")]
        [Authorize]
        public async Task<HttpResponseMessage> DeleteErrorException(string errorExceptionId)
        {
            try
            {
                await colorOptionRepository.DeleteErrorException(errorExceptionId);

                returnMessage.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "DeleteErrorException");

                return await Task.FromResult(returnMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return await Task.FromResult(returnMessage);
        }

        /// <summary>
        /// Update Error Exception
        /// </summary>
        /// <param name="errorException"></param>
        /// <returns></returns>
        [HttpPost("UpdateErrorException")]
        [Authorize]
        public async Task UpdateErrorException([FromBody] ErrorException errorException)
        {

            try
            {
                await colorOptionRepository.UpdateErrorException(errorException);

            }
            catch (Exception ex)
            {
                
            }

        }

    }
}
