// ***********************************************************************
// Assembly         : MteSDRTest.Server
// Author           : Eclypses Developer
// Created          : 07-20-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-20-2022
// ***********************************************************************
// <copyright file="PayloadHelper.cs" company="MteSDRTest.Server">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MteSDRTest.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MteSDRTest.Server.Helpers {
    /// <summary>
    /// Class PayloadHelper.
    /// Implements the <see cref="MteSDRTest.Server.Helpers.IPayloadHelper" />.
    /// </summary>
    /// <seealso cref="MteSDRTest.Server.Helpers.IPayloadHelper" />
    public class PayloadHelper : IPayloadHelper {
        /// <summary>
        /// The mte.
        /// </summary>
        private readonly IMteHelper _mte;

        /// <summary>
        /// The configuration object from the startup.
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<PayloadHelper> _logger;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadHelper" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="mte">The mte.</param>
        /// <param name="config">The configuration.</param>
        public PayloadHelper(ILogger<PayloadHelper> logger, IMteHelper mte, IConfiguration config) {
            _logger = logger;
            _mte = mte;
            _config = config;
        }
        #endregion

        #region Reveal

        /// <summary>
        /// Reveals the incoming MKE encrypted payload from the specified request.
        /// </summary>
        /// <typeparam name="T">Type of the object that is revealed.</typeparam>
        /// <param name="request">The request.</param>
        /// <returns>T.</returns>
        public async Task<T> Reveal<T>(HttpRequest request) {
            try {
                //
                // Grab the content (MTE protected string).
                //
                var encoded = await new StreamReader(request.Body).ReadToEndAsync();
                if (_config["AppSettings:UseMTE"] is not null && _config["AppSettings:UseMTE"].Equals("true", StringComparison.OrdinalIgnoreCase)) {
                    //
                    // Get the client id header to identify this client.
                    //
                    string clientId = request.Headers[Constants.CLIENT_HEADER_KEY];

                    //
                    // Use MKE to decrypt the string and then deserialize it.
                    //
                    string json = await _mte.MkeDecryptString(clientId, encoded);
                    return JsonSerializer.Deserialize<T>(json);
                } else {
                    return JsonSerializer.Deserialize<T>(encoded);
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Exception revealing the payload.");
                throw;
            }
        }
        #endregion

        #region Conceal

        /// <summary>
        /// Conceals the clear object of type T and returns a base64 string.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="clear">The clear.</param>
        /// <returns>System.String.</returns>
        public async Task<string> Conceal(HttpRequest request, object clear) {
            try {
                string json = JsonSerializer.Serialize(clear);
                if (_config["appSettings:UseMTE"] is not null && _config["appSettings:UseMTE"].Equals("true", StringComparison.OrdinalIgnoreCase)) {
                    //
                    // Get the client id header to identify this client.
                    //
                    string clientId = request.Headers["X-ClientId"];
                    return await _mte.MkeEncryptString(clientId, json);
                } else {
                    return json;
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Exception concealing the payload.");
                throw;
            }
        }
        #endregion
    }
}
