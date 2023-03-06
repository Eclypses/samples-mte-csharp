using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MteDemoTest.Models;
using MteDemoTest.Repository;
using System;
using System.Threading.Tasks;

namespace MteDemoTest.Controllers
{
    public class MultipleClientController : Controller
    {
        private readonly ILogger<MultipleClientController> _logger;
        private IMultipleClientRepository _multiRepo;

        public MultipleClientController(ILogger<MultipleClientController> logger, IMultipleClientRepository multiRepo)
        {
            _logger = logger;
            _multiRepo = multiRepo;
        }

        /// <summary>
        /// Message posted from Multi-Client test
        /// Header contains clientId
        /// Message should be encoded with MTE
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>IActionResult.</returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("api/multiclient")]
        public async Task<IActionResult> PostMessage([FromBody] string value)
        {
            try
            {
                _logger.LogDebug("Entered PostMessage method in multi client.");
                //---------------------------------
                // Get clientId from request header
                //---------------------------------
                string clientId = Request.Headers[Constants.ClientIdHeader];

                // add check to make sure we get an entry
                if (string.IsNullOrWhiteSpace(clientId))
                {
                    ResponseModel<Object> response = new ResponseModel<object>
                    {
                        Message = $"ClientId is empty or null, must have identifier in Header.",
                        ResultCode = Constants.RC_VALIDATION_ERROR,
                        Success = false
                    };
                    return new JsonResult(response);

                }

                //-----------------------------------------
                // send the body and clientId to Repository
                //-----------------------------------------
                ResponseModel<string> result = _multiRepo.MultiClientResponse(value, clientId);

                //---------------
                // Return result
                //---------------
                return new JsonResult(result);
            }
            catch (Exception e)
            {
                ResponseModel<string> errorResponse = new ResponseModel<string>
                {
                    Message = $"Exception: {e.Message}",
                    Data = null,
                    ResultCode = Constants.RC_CONTROLLER_EXCEPTION,
                    Success = false
                };
                _logger.LogError(errorResponse.Message);
                return new JsonResult(errorResponse);
            }
        }
    }
}
