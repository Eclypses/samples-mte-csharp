// ***********************************************************************
// Assembly         : MteSDRTest.Client
// Author           : Eclypses Developer
// Created          : 07-19-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 08-03-2022
// ***********************************************************************
// <copyright file="MteService.cs" company="MteSDRTest.Client">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MteSDRTest.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;

namespace MteSDRTest.Client.Services {
    /// <summary>
    /// Class MteService.
    /// Implements the <see cref="MteSDRTest.Client.Services.IMteService" />.
    /// This wraps the calls to the Eclypses MTE WASM module.
    /// </summary>
    /// <seealso cref="MteSDRTest.Client.Services.IMteService" />
    public class MteService : IMteService {
        /// <summary>
        /// A flag to indicate if the SDR is initialized.
        /// </summary>
        private static bool _sdrInitialized = false;

        /// <summary>
        /// The java script runtime.
        /// </summary>
        private readonly IJSRuntime _jsRuntime;

        /// <summary>
        /// The configuration object from appsettings.json in the wwwroot folder.
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// The Elliptical Curve Diffie-Hellman wrapper.
        /// </summary>
        private readonly IECDHService _ecdh;

        /// <summary>
        /// The mte helpers module.
        /// </summary>
        private IJSObjectReference _mteHelpersModule;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MteService" /> class.
        /// </summary>
        /// <param name="jSRuntime">The java script runtime.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="ecdh">The ecdh.</param>
        public MteService(IJSRuntime jSRuntime, IConfiguration config, IECDHService ecdh) {
            _jsRuntime = jSRuntime;
            _ecdh = ecdh;
            _mteHelpersModule = null;
            _config = config;
        }
        #endregion

        #region InstantiateMteWasm

        /// <summary>
        /// Instantiates the mte WASM module which is the actual implementation of the MTE
        /// and then calls to the Proxy server to pair this specific client.
        /// </summary>
        /// <param name="apiClient">The API client.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        public async Task<bool> InstantiateMteWasm(HttpClient apiClient) {
            try {
                //
                // If we are not using MTE, just return.
                //
                if (_config["AppSettings:UseMTE"] is null || _config["AppSettings:UseMTE"].Equals("false", StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }

                if (_mteHelpersModule is null) {
                    _mteHelpersModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/mte-helper.js");
                }

                //
                // Share public keys with the server to be able to compute entropy.
                //
                await _mteHelpersModule!.InvokeAsync<bool>("instantiateMteWasm");

                return await PairWithTheServer(apiClient);
            } catch (Exception ex) {
                Console.WriteLine(ex);
                return false;
            }
        }

        #endregion

        #region InitializeSDR

        /// <summary>
        /// Initializes the SDR by creating a new Session Storage SDR
        /// and re-hydrating a Local Storage SDR.
        /// </summary>
        /// <param name="localStorageValues">The local storage values.</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        /// <exception cref="System.ApplicationException">Cannot initialize SDR since the MTE java wrapper is not ready.</exception>
        public async Task<bool> InitializeSDR(ServerSDRValues localStorageValues) {
            try {
                if (_mteHelpersModule is null) {
                    throw new ApplicationException("Cannot initialize SDR since the MTE java wrapper is not ready.");
                }

                //
                // Make sure we only initialize once.
                //
                if (_sdrInitialized) {
                    return true;
                }

                byte[] localStorageEntropy = Convert.FromBase64String(localStorageValues.WorkstationLocalStorageEntropy);
                string localStorageNonce = localStorageValues.WorkstationLocalStorageNonce;
                await _mteHelpersModule!.InvokeVoidAsync("initializePersistentSdr", Constants.DISPLAY_LOCAL_ITEM_FILE, localStorageEntropy, localStorageNonce);

                byte[] sessionStorageEntropy = Convert.FromBase64String(await _jsRuntime.InvokeAsync<string>("getEntropy", 32));
                string sessionStorageNonce = DateTime.Now.ToString("yyMMddHHmmssffff");
                await _mteHelpersModule!.InvokeVoidAsync("initializeSessionSdr", Constants.DISPLAY_SESSION_ITEM_FILE, sessionStorageEntropy, sessionStorageNonce);

                _sdrInitialized = true;
                return true;
            } catch (Exception ex) {
                Console.WriteLine(ex);
                return false;
            }
        }
        #endregion

        #region ReadBrowserData

        /// <summary>
        /// Reads the browser data using Eclypses SDR.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="persistent">if set to <c>true</c> [persistent].</param>
        /// <returns>System.String.</returns>
        public async Task<string> ReadBrowserData(string name, bool persistent = false) {
            if (_mteHelpersModule is not null && _sdrInitialized) {
                return await _mteHelpersModule.InvokeAsync<string>("read", name, persistent);
            } else {
                return null;
            }
        }
        #endregion

        #region WriteBrowserData

        /// <summary>
        /// Writes the browser data using Eclypses SDR.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="data">The data.</param>
        /// <param name="persistent">if set to <c>true</c> [persistent].</param>
        /// <returns>Completed task.</returns>
        public async Task WriteBrowserData(string name, string data, bool persistent = false) {
            if (_mteHelpersModule is not null && _sdrInitialized) {
                await _mteHelpersModule.InvokeVoidAsync("write", name, data, persistent);
            }
        }
        #endregion

        #region RemoveBrowserData

        /// <summary>
        /// Removes the browser data using Eclypses SDR.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="persistent">if set to <c>true</c> [persistent].</param>
        /// <returns>Completed task.</returns>
        public async Task RemoveBrowserData(string name, bool persistent = false) {
            if (_mteHelpersModule is not null && _sdrInitialized) {
                await _mteHelpersModule.InvokeVoidAsync("remove", name, persistent);
            }
        }
        #endregion

        #region MkeEncryptString

        /// <summary>
        /// Encrypts a string using MKE.
        /// </summary>
        /// <param name="clear">The clear.</param>
        /// <returns>System.Nullable&lt;System.String&gt;.</returns>
        public async Task<string> MkeEncryptString(string clear) {
            if (_mteHelpersModule is not null) {
                var encoded = await _mteHelpersModule.InvokeAsync<string>("mkeEncryptString", clear);
                return encoded;
            } else {
                return null;
            }
        }
        #endregion

        #region MkeDecryptString

        /// <summary>
        /// Decrypts a string using MKE.
        /// </summary>
        /// <param name="encoded">The encoded.</param>
        /// <returns>System.Nullable&lt;System.String&gt;.</returns>
        public async Task<string> MkeDecryptString(string encoded) {
            if (_mteHelpersModule is not null) {
                return await _mteHelpersModule.InvokeAsync<string>("mkeDecryptString", encoded);
            } else {
                return null;
            }
        }
        #endregion

        #region PairWithTheServer

        /// <summary>
        /// Pairs with the server.
        /// </summary>
        /// <param name="apiClient">The API client.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <exception cref="System.ApplicationException">Error pairing with the server at {route} is {result.StatusCode}.</exception>
        private async Task<bool> PairWithTheServer(HttpClient apiClient) {
            try {
                //
                // Set the personalization string for this client
                //
                string personalization = Guid.NewGuid().ToString();

                //
                // Get the key pairs from Elliptical Curve Diffie-Hellman
                //
                string encoderPublicKey = await _ecdh.GetEcdh("encoder");
                string decoderPublicKey = await _ecdh.GetEcdh("decoder");

                //
                // Construct a model of the two public keys
                // and the personalization string and POST this to the Proxy server.
                //
                var model = new ClientPairModel {
                    ClientDecoderPublicKey = decoderPublicKey,
                    ClientEncoderPublicKey = encoderPublicKey,
                    Personalization = personalization,
                };
                string route = Constants.ROUTE_MTEPAIR;
                var result = await apiClient.PostAsJsonAsync(route, model);
                if (!result.IsSuccessStatusCode) {
                    throw new ApplicationException($"Error pairing with the server at {route} is {result.StatusCode}");
                }

                //
                // Get the returned data from the Proxy server and
                // compute the entropy for each pair. Note that the
                // server's encoder public key is used for the client's decoder
                // and vice versa.  This established two one-way pairs.
                //
                var serverPairModel = await result.Content.ReadFromJsonAsync<ServerPairModel>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                string nonce = serverPairModel!.Nonce.ToString();
                string encoderEntropy = await _ecdh.ComputeSharedSecret("encoder", serverPairModel!.ServerDecoderPublicKey!);
                string decoderEntropy = await _ecdh.ComputeSharedSecret("decoder", serverPairModel!.ServerEncoderPublicKey!);

                await CreateInitialMteStates(encoderEntropy, decoderEntropy, nonce, personalization);

                //
                // Just return true to indicate that this all worked.
                //
                return true;
            } catch (Exception) {
                return false;
            }
        }
        #endregion

        #region CreateInitialMteStates

        /// <summary>
        /// Creates the mte initial states
        /// by calling into the mtehelpers java script module.
        /// These are then stored as objects within the js module.
        /// </summary>
        /// <param name="encoderEntropy">The encoder entropy.</param>
        /// <param name="decoderEntropy">The decoder entropy.</param>
        /// <param name="nonce">The nonce.</param>
        /// <param name="personalization">The personalization.</param>
        /// <returns>Task.</returns>
        private async Task CreateInitialMteStates(string encoderEntropy, string decoderEntropy, string nonce, string personalization) {
            if (_mteHelpersModule is not null) {
                await _mteHelpersModule.InvokeVoidAsync("createInitialMteState", encoderEntropy, decoderEntropy, nonce, personalization);
            }
        }
        #endregion
    }
}
