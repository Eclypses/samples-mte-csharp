// ***********************************************************************
// Assembly         : MteSDRTest.Client
// Author           : Eclypses Developer
// Created          : 07-19-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-19-2022
// ***********************************************************************
// <copyright file="PayloadService.cs" company="MteSDRTest.Client">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MteSDRTest.Client.Services {
    /// <summary>
    /// Class PayloadService.
    /// Wraps specific calls to the MTE to Conceal and Reveal data.
    /// Implements the <see cref="MteSDRTest.Client.Services.IPayloadService" />.
    /// </summary>
    /// <seealso cref="MteSDRTest.Client.Services.IPayloadService" />
    public class PayloadService : IPayloadService {
        /// <summary>
        /// The mte wrapper around the Eclypses MTE WASM module.
        /// </summary>
        private readonly IMteService _mte;

        /// <summary>
        /// The configuration from the appsettings.
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<PayloadService> _logger;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadService" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="mte">The mte.</param>
        public PayloadService(ILogger<PayloadService> logger, IConfiguration config, IMteService mte) {
            _logger = logger;
            _mte = mte;
            _config = config;
        }
        #endregion

        #region Reveal

        /// <summary>
        /// Reveals the specified payload by calling into the Eclypses MKE.
        /// </summary>
        /// <typeparam name="T">Type of the object to reveal.</typeparam>
        /// <param name="payload">The payload.</param>
        /// <returns>Task&lt;System.Nullable&lt;T&gt;&gt;.</returns>
        public async Task<T> Reveal<T>(string payload) {
            try {
                if (_config["appSettings:UseMTE"] is not null && _config["appSettings:UseMTE"].Equals("true", StringComparison.OrdinalIgnoreCase)) {
                    string json = await _mte.MkeDecryptString(payload);
                    return JsonSerializer.Deserialize<T>(json!);
                } else {
                    return JsonSerializer.Deserialize<T>(payload!);
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Exception revealing payload.");
                throw;
            }
        }
        #endregion

        #region Conceal

        /// <summary>
        /// Conceals the specified payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>Task&lt;System.Nullable&lt;System.String&gt;&gt;.</returns>
        public async Task<string> Conceal(object payload) {
            try {
                string json = JsonSerializer.Serialize(payload);
                if (_config["appSettings:UseMTE"] is not null && _config["appSettings:UseMTE"].Equals("true", StringComparison.OrdinalIgnoreCase)) {
                    string encoded = await _mte.MkeEncryptString(json);
                    return encoded;
                } else {
                    return json;
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Exception concealing payload.");
                throw;
            }
        }
        #endregion
    }
}
