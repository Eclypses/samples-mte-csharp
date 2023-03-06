// ***********************************************************************
// Assembly         : MteSDRTest.Server
// Author           : Eclypses Developer
// Created          : 07-20-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-20-2022
// ***********************************************************************
// <copyright file="EchoController.cs" company="MteSDRTest.Server">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Threading.Tasks;
using MteSDRTest.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MteSDRTest.Server.Controllers {
    /// <summary>
    /// Class EchoController.
    /// Implements the <see cref="Microsoft.AspNetCore.Mvc.ControllerBase" />.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [ApiController]
    public class EchoController : ControllerBase {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<EchoController> _logger;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="EchoController" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public EchoController(ILogger<EchoController> logger) {
            _logger = logger;
        }
        #endregion

        #region GET: api/echo/{msg}

        /// <summary>
        /// Echoes back the incoming message - this is used to verify the server is alive.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <returns>System.String.</returns>
        [HttpGet]
        [Route(Constants.ROUTE_GET_ECHO)]
        [AllowAnonymous]
        public async Task<string> Get([FromRoute] string msg) {
            try {
                string echoMsg = $"Echoed at {DateTime.Now} - {msg} ";
                _logger.LogDebug(echoMsg);
                return echoMsg;
            } catch (Exception ex) {
                _logger.LogError(ex, "Exception trying to echo");
                return ex.Message;
            }
        }
        #endregion
    }
}
