using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MteDemoTest.Helpers;
using MteDemoTest.Models;
using MteDemoTest.Repository;
using System;
using System.Threading.Tasks;

namespace MteDemoTest.Controllers
{
    public class HandshakeController : Controller
    {
        ILogger<HandshakeController> _logger;
        private IHandshakeRepository _handshakeRepo;
        private readonly IAuthHelper _authHelper;

        public HandshakeController(ILogger<HandshakeController> logger, IHandshakeRepository handshakeRepo, IAuthHelper authHelper)
        {
            _logger = logger;
            _handshakeRepo = handshakeRepo;
            _authHelper = authHelper;
        }

        #region Handshake Controller
        /// <summary>
        /// Handshake controller to create entropy
        /// Using DH Container
        /// </summary>
        /// <param name="model">The handshake model.</param>
        /// <returns>IActionResult.</returns>
        [AllowAnonymous]
        [Route("api/handshake")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] HandshakeModel model)
        {
            EndUserCredentials endUser = null;
            try
            {
                //-------------------------------------
                // Get the user object out of the JWT
                //-------------------------------------
                endUser = await _authHelper.RetrieveEndUserCredentials(User);

                // create initial Encoder and Decoder and store state then return partner public key and time stamp
                ResponseModel<HandshakeModel> result = _handshakeRepo.StoreInitialClientHandshake(model);
                if(endUser != null)
                {
                    result.access_token = endUser.Token;
                }
                return new JsonResult(result);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception trying to complete the handshake.");
                return new BadRequestResult();
            }
        } 
        #endregion
    }
}
