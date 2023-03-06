// ***********************************************************************
// Assembly         : MteSDRTest.Server
// Author           : Eclypses Developer
// Created          : 07-19-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 08-01-2022
// ***********************************************************************
// <copyright file="LoginController.cs" company="MteSDRTest.Server">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Threading.Tasks;
using MteSDRTest.Common.Models;
using MteSDRTest.Server.Helpers;
using MteSDRTest.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MteSDRTest.Server.Controllers {
    /// <summary>
    /// Class LoginController.
    /// Implements the <see cref="ControllerBase" />.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [ApiController]
    public class LoginController : ControllerBase {
        /// <summary>
        /// The configuration from appsettings.
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<LoginController> _logger;

        /// <summary>
        /// The payload helper responsible for revealing and concealing payloads
        /// that are protected with the MTE.
        /// </summary>
        private readonly IPayloadHelper _payloadHelper;

        /// <summary>
        /// The authentication service that validates the login request.
        /// </summary>
        private readonly IAuthService _authService;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginController" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="payloadHelper">The payload helper.</param>
        /// <param name="authService">The authentication service.</param>
        public LoginController(ILogger<LoginController> logger, IConfiguration config, IPayloadHelper payloadHelper, IAuthService authService) {
            _logger = logger;
            _config = config;
            _payloadHelper = payloadHelper;
            _authService = authService;
        }
        #endregion

        /// <summary>
        /// Posts this instance.
        /// </summary>
        /// <returns>IActionResult.</returns>
        [HttpPost]
        [Route(Constants.ROUTE_LOGIN)]
        [AllowAnonymous]
        public async Task<IActionResult> Post() {
            //
            // Use MKE to reveal the incoming payload.
            //
            ClientCredentials model = await _payloadHelper.Reveal<ClientCredentials>(Request);

            //
            // Now that we have a model, validate it to ensure the DataAnnotation rules pass.
            //
            if (TryValidateModel(model, nameof(model))) {
                //
                // Invoke the authorization service to identify and authorize the request.
                //
                var client = await _authService.Authorize(model);
                if (client is not null) {
                    //
                    // Use MKE to conceal the outgoing payload.
                    //
                    string encoded = await _payloadHelper.Conceal(Request, client);
                    return Content(encoded);
                } else {
                    return Unauthorized();
                }
            }

            return BadRequest();
        }
    }
}
