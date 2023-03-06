// ***********************************************************************
// Assembly         : MteSDRTest.Server
// Author           : Eclypses Developer
// Created          : 07-19-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-20-2022
// ***********************************************************************
// <copyright file="DataExchangeController.cs" company="MteSDRTest.Server">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Threading.Tasks;
using MteSDRTest.Common.Models;
using MteSDRTest.Server.Helpers;
using MteSDRTest.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MteSDRTest.Server.Controllers {
    /// <summary>
    /// Class DataExchangeController.
    /// Implements the <see cref="ControllerBase" />.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [ApiController]
    [Authorize(Roles = "User")]
    public class DataExchangeController : ControllerBase {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<DataExchangeController> _logger;

        /// <summary>
        /// The data exchange service.
        /// </summary>
        private readonly IDataExchangeService _dataExchangeService;

        /// <summary>
        /// The payload helper - manages MTE Encode and Decode.
        /// </summary>
        private readonly IPayloadHelper _payloadHelper;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DataExchangeController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="dataExchangeService">The data exchange service.</param>
        public DataExchangeController(ILogger<DataExchangeController> logger, IDataExchangeService dataExchangeService, IPayloadHelper payloadHelper) {
            _logger = logger;
            _dataExchangeService = dataExchangeService;
            _payloadHelper = payloadHelper;
        }
        #endregion

        #region POST: api/data

        /// <summary>
        /// Retrieves a value associated with the workstation identifier in
        /// the incoming model. The actual payloads are protected by the
        /// MTE and the user must be authenticated to make this route call.
        /// </summary>
        /// <param name="model">The data exchange request.</param>
        /// <returns>IActionResult containing the incoming model with the
        /// requested properties (server public key and encrypted data).</returns>
        [HttpPost]
        [Route("api/data")]
        public async Task<IActionResult> POST() {
            try {
                DataExchangeModel model = await _payloadHelper.Reveal<DataExchangeModel>(Request);
                if (TryValidateModel(model, nameof(model))) {
                    if (ModelState.IsValid) {
                        await _dataExchangeService.RetrieveValue(model);
                        string encoded = await _payloadHelper.Conceal(Request, model);
                        return Content(encoded);
                    }
                } else {
                    return BadRequest(ModelState);
                }

                return BadRequest();
            } catch (Exception ex) {
                _logger.LogCritical(ex, "Exception retrieving arbitrary data.");
                return BadRequest(ex);
            }
        }
        #endregion
    }
}
