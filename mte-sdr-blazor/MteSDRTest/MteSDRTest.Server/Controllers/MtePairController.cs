// ***********************************************************************
// Assembly         : MteSDRTest.Server
// Author           : Eclypses Developer
// Created          : 07-20-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-20-2022
// ***********************************************************************
// <copyright file="MtePairController.cs" company="MteSDRTest.Server">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Threading.Tasks;
using MteSDRTest.Common.Models;
using MteSDRTest.Server.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MteSDRTest.Server.Controllers {
    /// <summary>
    /// Class MtePairController.
    /// Implements the <see cref="ControllerBase" />
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [ApiController]
    public class MtePairController : ControllerBase {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<MtePairController> _logger;

        /// <summary>
        /// The configuration object that wraps appsettings.json.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// The Elliptical Curve Diffie-Hellman helper.
        /// </summary>
        private readonly IECDHHelper _ecdh;

        /// <summary>
        /// The MTE helper.
        /// </summary>
        private readonly IMteHelper _mteHelper;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MtePairController" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="ecdh">The Elliptical Curve Diffie-Hellman helper.</param>
        /// <param name="mteHelper">The MTE helpers.</param>
        public MtePairController(ILogger<MtePairController> logger, IConfiguration configuration, IECDHHelper ecdh, IMteHelper mteHelper) {
            _logger = logger;
            _configuration = configuration;
            _ecdh = ecdh;
            _mteHelper = mteHelper;
        }
        #endregion

        #region POST: api/mtepair

        /// <summary>
        /// Accepts a payload from the client for pairing. This has a personalization string
        /// and a client public key so that the server can create its public key and entropy
        /// to establish the two pairs of MTEs (Encoder and Decoder).
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>IActionResult.</returns>
        [HttpPost]
        [Route(Constants.ROUTE_MTEPAIR)]
        public async Task<IActionResult> Post([FromBody] ClientPairModel model) {
            try {
                //
                // Get the client id header to identify this client.
                //
                string clientId = Request.Headers[Constants.CLIENT_HEADER_KEY];

                //
                // Calculate a nonce value from the current time.
                //
                var timeStamp = ulong.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssff"));

                //
                // Calculate a public key and entropy for the server encoder / client decoder pair.
                //
                var encoderInfo = _ecdh.ComputeSharedSecret("encoder", model.ClientDecoderPublicKey!);

                //
                // Calculate a public key and entropy for the server decoder / client encoder pair.
                //
                var decoderInfo = _ecdh.ComputeSharedSecret("decoder", model.ClientEncoderPublicKey!);

                //
                // Instance up an MTE using the license information from appsettings.json.
                //
                string licensedCompany = _configuration["appSettings:LicensedCompany"];
                string licenseKey = _configuration["appSettings:LicenseKey"];
                if (!_mteHelper.Instantiate(licensedCompany, licenseKey)) {
                    return BadRequest();
                }

                //
                // Create the Encoder (this saves the initial state for MTE which is also used for MKE).
                //
                await _mteHelper.CreateMteEncoder(clientId, model.Personalization!, timeStamp, encoderInfo.SharedSecret!);

                //
                // Create the Decoder (this saves the initial state for MTE which is also used for MKE).
                //
                await _mteHelper.CreateMteDecoder(clientId, model.Personalization!, timeStamp, decoderInfo.SharedSecret!);

                //
                // Return the Base-64 encoded public keys and the nonce to the client so it can pair.
                //
                var result = new {
                    serverEncoderPublicKey = Convert.ToBase64String(encoderInfo.ServerPublicKey!),
                    serverDecoderPublicKey = Convert.ToBase64String(decoderInfo.ServerPublicKey!),
                    nonce = timeStamp,
                };
                return new JsonResult(result);
            } catch (Exception ex) {
                _logger.LogError(ex, "Exception trying to pair.");
                return BadRequest();
            }
        }
        #endregion
    }
}
