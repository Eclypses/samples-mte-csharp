// ***********************************************************************
// Assembly         : MteSDRTest.Client
// Author           : Eclypses Developer
// Created          : 07-19-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-19-2022
// ***********************************************************************
// <copyright file="ECDHService.cs" company="MteSDRTest.Client">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace MteSDRTest.Client.Services {
    /// <summary>
    /// Class ECDHService.
    /// This wraps calls to the java script module for Elliptical Curve Diffie-Hellman.
    /// Implements the <see cref="MteSDRTest.Client.Services.IECDHService" />.
    /// </summary>
    /// <seealso cref="MteSDRTest.Client.Services.IECDHService" />
    public class ECDHService : IECDHService {
        /// <summary>
        /// The java script runtime.
        /// </summary>
        private readonly IJSRuntime _jsRuntime;

        /// <summary>
        /// The Elliptical Curve Diffie-Hellman helper java script module.
        /// </summary>
        private IJSObjectReference _ecdhHelperModule;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ECDHService" /> class.
        /// </summary>
        /// <param name="jsRuntime">The java script runtime.</param>
        public ECDHService(IJSRuntime jsRuntime) {
            _jsRuntime = jsRuntime;
            _ecdhHelperModule = null;
        }
        #endregion

        #region GetEcdh

        /// <summary>
        /// Gets the Elliptical Curve Diffie-Hellman public key.
        /// by calling into the ecdh java script module.
        /// </summary>
        /// <param name="flavor">The flavor.</param>
        /// <returns>System.String of the public key.</returns>
        public async Task<string> GetEcdh(string flavor) {
            try {
                if (_ecdhHelperModule is null) {
                    _ecdhHelperModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/ecdh.js");
                }

                return await _ecdhHelperModule.InvokeAsync<string>("getEcdh", flavor);
            } catch (Exception) {
                throw;
            }
        }
        #endregion

        #region ComputeSharedSecret

        /// <summary>
        /// Computes the shared secret
        /// by calling into the ecdh java script module
        /// with the two public keys (this browser and the Proxy server).
        /// </summary>
        /// <param name="flavor">The flavor.</param>
        /// <param name="pairedServerPublicKeyB64">The paired server public key as a B64 string.</param>
        /// <returns>System.Nullable&lt;System.String&gt;.</returns>
        /// <exception cref="System.ApplicationException">You must invoke the GetEcdh method prior to computing a shared secret.</exception>
        /// <exception cref="System.ApplicationException">You must include the proxy server public key prior to computing a shared secret.</exception>
        public async Task<string> ComputeSharedSecret(string flavor, string pairedServerPublicKeyB64) {
            try {
                if (_ecdhHelperModule is null) {
                    throw new ApplicationException("You must invoke the GetEcdh method prior to computing a shared secret.");
                }

                if (string.IsNullOrWhiteSpace(pairedServerPublicKeyB64)) {
                    throw new ApplicationException("You must include the proxy server public key prior to computing a shared secret.");
                }

                return await _ecdhHelperModule.InvokeAsync<string>("computeSharedSecret", flavor, pairedServerPublicKeyB64);
            } catch (Exception) {
                throw;
            }
        }
        #endregion
    }
}
