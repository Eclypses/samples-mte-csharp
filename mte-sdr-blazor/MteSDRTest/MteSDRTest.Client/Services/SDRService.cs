// ***********************************************************************
// Assembly         : MteSDRTest.Client
// Author           : Eclypses Developer
// Created          : 08-04-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 08-08-2022
// ***********************************************************************
// <copyright file="SDRService.cs" company="MteSDRTest.Client">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MteSDRTest.Client.Helpers;
using MteSDRTest.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MteSDRTest.Client.Services {
    /// <summary>
    /// Class SDRService.
    /// Implements the <see cref="MteSDRTest.Client.Services.ISDRService" />.
    /// </summary>
    /// <seealso cref="MteSDRTest.Client.Services.ISDRService" />
    public class SDRService : ISDRService {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<SDRService> _logger;

        /// <summary>
        /// The configuration.
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// The payload service for managing protected data.
        /// </summary>
        private readonly IPayloadService _payloadService;

        /// <summary>
        /// The web helper to call the API.
        /// </summary>
        private readonly IWebHelper _web;

        /// <summary>
        /// Initializes a new instance of the <see cref="SDRService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="payloadService">The payload service.</param>
        /// <param name="web">The web.</param>
        public SDRService(ILogger<SDRService> logger, IConfiguration config, IPayloadService payloadService, IWebHelper web) {
            _logger = logger;
            _payloadService = payloadService;
            _web = web;
            _config = config;
        }

        /// <summary>
        /// Obtains the local storage information
        /// from the API to create a consistant local storage SDR.
        /// </summary>
        /// <param name="apiClient">The API client.</param>
        /// <returns>Task&lt;ServerSDRValues&gt;.</returns>
        /// <exception cref="System.ArgumentNullException">No workstation identifier found - cannot create long term secure storage.</exception>
        public async Task<ServerSDRValues> ObtainLocalStorageInformation(HttpClient apiClient) {
            try {
                string workstationIdentifier = _config["AppSettings:WorkstationIdentifier"];
                if (string.IsNullOrWhiteSpace(workstationIdentifier)) {
                    throw new ArgumentNullException("No workstation identifier found - cannot create long term secure storage");
                }

                //
                // Call the API to get a protected item to use for entropy and nonce with
                // the long-term secure data.
                //
                var model = new DataExchangeModel { ItemKey = "longTerm", ItemOwner = workstationIdentifier };
                string encoded = await _payloadService.Conceal(model!);
                string returnEncoded = await _web.PostToServer(apiClient, Constants.ROUTE_RETRIEVE_SOME_DATA, encoded!);

                //
                // The response is protected by the MTE, so reveal it to get the
                // information required for creating the consistent secure local storage.
                //
                model = await _payloadService.Reveal<DataExchangeModel>(returnEncoded);
                return JsonSerializer.Deserialize<ServerSDRValues>(model.Value);
            } catch (Exception ex) {
                _logger.LogError(ex, "Could not establish secure local storage.");
                throw;
            }
        }
    }
}
