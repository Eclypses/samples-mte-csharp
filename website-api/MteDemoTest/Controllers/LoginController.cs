using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MteDemoTest.Models;
using MteDemoTest.Repository;
using System;

namespace MteDemoTest.Controllers
{
    public class LoginController : Controller
    {
        /// <summary>
        /// Add in dependencies
        /// </summary>
        private readonly ILogger<LoginController> _logger;
        private readonly ILoginRepository _login;

        public LoginController(ILogger<LoginController> logger, ILoginRepository login)
        {
            _logger = logger;
            _login = login;
        }

        #region LoginWithMTE
        /// <summary>
        /// Login to API
        /// </summary>
        /// <param name="value">Encoded String of serialized login</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("api/login")]
        public IActionResult LoginWithMTE([FromBody] string value)
        {
            try
            {
                _logger.LogDebug("Entered Login Controller method.");
                //---------------------------------
                // Get clientId from request header
                //---------------------------------
                string clientId = Request.Headers[Constants.ClientIdHeader];

                // add check to make sure we get an entry
                if (string.IsNullOrWhiteSpace(clientId))
                {
                    ResponseModel response = new ResponseModel
                    {
                        Message = $"ClientId is empty or null, must have identifier in Header.",
                        ResultCode = Constants.RC_VALIDATION_ERROR,
                        Success = false
                    };
                    return new JsonResult(response);

                }

                //-----------------------------------------
                // send the message to the Login Repository
                //-----------------------------------------
                ResponseModel<string> result = _login.UserLogin(clientId, value);
                //---------------
                // Return result
                //---------------
                return new JsonResult(result);
            }
            catch (Exception e)
            {
                ResponseModel errorResponse = new ResponseModel
                {
                    Message = $"Exception: {e.Message}",
                    ResultCode = Constants.RC_CONTROLLER_EXCEPTION,
                    Success = false
                };
                _logger.LogError(errorResponse.Message);
                return new JsonResult(errorResponse);
            }
        } 
        #endregion
    }
}
