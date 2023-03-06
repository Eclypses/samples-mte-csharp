// ***********************************************************************
// Assembly         : MteSDRTest.Client
// Author           : Eclypses Developer
// Created          : 07-19-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-28-2022
// ***********************************************************************
// <copyright file="AuthService.cs" company="MteSDRTest.Client">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MteSDRTest.Client.Helpers;
using MteSDRTest.Common.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace MteSDRTest.Client.Services {
    /// <summary>
    /// Class AuthService.
    /// Calls the API to authenticate a user. The user
    /// information is protected by the Eclypses MTE using the payload helper.
    /// Implements the <see cref="MteSDRTest.Client.Services.IAuthService" />.
    /// </summary>
    /// <seealso cref="MteSDRTest.Client.Services.IAuthService" />
    public class AuthService : IAuthService {
        /// <summary>
        /// The HTTP client for the API server.
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// The payload service for managing high-level interactions with the Eclypses MKE.
        /// </summary>
        private readonly IPayloadService _payloadService;

        /// <summary>
        /// The mte wrapper - manages low-level interactions with the Eclypses MTE.
        /// </summary>
        private readonly IMteService _mte;

        /// <summary>
        /// The SDR service - manages getting info from the API
        /// so that the SDR can build a consistent and persistant Local Storage.
        /// </summary>
        private readonly ISDRService _sdrService;

        /// <summary>
        /// The web wrapper.
        /// </summary>
        private readonly IWebHelper _web;

        /// <summary>
        /// The state provider.
        /// </summary>
        private readonly AuthenticationStateProvider _stateProvider;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService" /> class.
        /// This is responsible for logging in and out of the Proxy server.
        /// When logging in, the proxy forwards the request to the GAPI
        /// for ultimate authentication.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="stateProvider">The state provider.</param>
        /// <param name="mte">The mte.</param>
        /// <param name="payloadService">The payload service.</param>
        /// <param name="sdrService">The SDR service.</param>
        /// <param name="web">The web.</param>
        public AuthService(HttpClient httpClient, AuthenticationStateProvider stateProvider, IMteService mte, IPayloadService payloadService, ISDRService sdrService, IWebHelper web) {
            _httpClient = httpClient;
            _stateProvider = stateProvider;
            _web = web;
            _payloadService = payloadService;
            _mte = mte;
            _sdrService = sdrService;
        }
        #endregion

        #region Login

        /// <summary>
        /// Logs in the user by calling the API to verify credentials.
        /// If successful, the StateProvider is updated and the http client gets the Jwt
        /// from the server.
        /// </summary>
        /// <param name="loginModel">The login model.</param>
        /// <returns>ClientUserModel.</returns>
        /// <exception cref="System.ApplicationException">Echo failed, server may not be available.</exception>
        /// <exception cref="System.ApplicationException">MTE Pairing with the server failed - cannot continue.</exception>
        public async Task<ClientUserModel> Login(ClientCredentials loginModel) {
            try {
                var echoString = await _web.GetFromServer<string>(_httpClient, "api/echo/helloAPI");

                if (string.IsNullOrWhiteSpace(echoString)) {
                    throw new ApplicationException($"Echo failed, server may not be available.");
                }

                //
                // Set the client id header to identify this client.
                // This allows the server to keep the MTE States in sync
                // for this connected client.
                //
                _httpClient.DefaultRequestHeaders.Add(Constants.CLIENT_HEADER_KEY, Guid.NewGuid().ToString());

                //
                // Set up the MTE in the browser and pair with the server.
                //
                if (!await _mte.InstantiateMteWasm(_httpClient)) {
                    throw new ApplicationException($"MTE Pairing with the server failed - cannot continue.");
                }

                //
                // Build a login request, protect it with the MKE and POST it
                // to the Proxy server for authentication. For this demo,
                // the only thing that is checked is the password. (see the API for details).
                //
                string encoded = await _payloadService.Conceal(loginModel!);
                var returnEncoded = await _web.PostToServer(_httpClient, Constants.ROUTE_LOGIN, encoded!);
                var clientUserModel = await _payloadService.Reveal<ClientUserModel>(returnEncoded);
                if (clientUserModel!.Success) {
                    await ((MyAuthenticationStateProvider)_stateProvider).MarkUserAsAuthenticated(clientUserModel);

                    //
                    // If the client successfully authenticated, save the JWT associated with the
                    // conversation between this browser and the Proxy in the Auth Header.
                    //
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", clientUserModel.ClientAuthToken);

                    //
                    // Get the values required to initiate this workstation's local storage SDR.
                    //
                    var localStorageValues = await _sdrService.ObtainLocalStorageInformation(_httpClient);
                    await _mte.InitializeSDR(localStorageValues);
                } else {
                    clientUserModel = new ClientUserModel {
                        ClientAuthToken = string.Empty,
                        Roles = new List<string>(),
                        Success = false,
                    };
                }

                return clientUserModel;
            } catch (Exception) {
                throw;
            }
        }
        #endregion

        #region Logout

        /// <summary>
        /// Logs out this current user by invoking the State Provider
        /// and clearing out the auth header's jwt.
        /// </summary>
        /// <returns>System.Threading.Tasks.Task.</returns>
        public async Task Logout() {
            await ((MyAuthenticationStateProvider)_stateProvider).MarkUserAsLoggedOut();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
        #endregion
    }
}
