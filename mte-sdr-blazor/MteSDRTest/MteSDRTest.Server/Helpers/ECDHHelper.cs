// ***********************************************************************
// Assembly         : MteSDRTest.Server
// Author           : Eclypses Developer
// Created          : 07-20-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-20-2022
// ***********************************************************************
// <copyright file="ECDHHelper.cs" company="MteSDRTest.Server">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Security.Cryptography;
using MteSDRTest.Server.Models;
using Microsoft.Extensions.Logging;

namespace MteSDRTest.Server.Helpers {
    /// <summary>
    /// Class ECDHHelper.
    /// Implements the <see cref="MteSDRTest.Server.Helpers.IECDHHelper" />
    /// </summary>
    /// <seealso cref="MteSDRTest.Server.Helpers.IECDHHelper" />
    public class ECDHHelper : IECDHHelper {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<ECDHHelper> _logger;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ECDHHelper" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public ECDHHelper(ILogger<ECDHHelper> logger) {
            _logger = logger;
        }
        #endregion

        #region ComputeSharedSecret

        /// <summary>
        /// Computes the shared secret using a partner public key
        /// and returns it along with the server's public key. Note: .Net actually
        /// takes the derived key material and returns a SHA-256 hash of it. This
        /// can cause problems if the client uses the generated key material, so
        /// your client may have to hash the regular value to get the shared
        /// secrets to actually match.
        /// </summary>
        /// <param name="flavor">The flavor.</param>
        /// <param name="clientPublicKey">The foreign public key as a Base-64 encoded string.</param>
        /// <returns>DerivedKeysModel with the shared secret and the server's public key.</returns>
        public DerivedKeysModel ComputeSharedSecret(string flavor, string clientPublicKey) {
            try {
                byte[] clientPublicKeyBytes = Convert.FromBase64String(clientPublicKey);
                var derivedKeyInfo = new DerivedKeysModel();
                using (var serverContainer = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256)) {
                    derivedKeyInfo.ServerPublicKey = serverContainer.ExportSubjectPublicKeyInfo();
                    using (var clientContainer = ECDiffieHellman.Create()) {
                        clientContainer.ImportSubjectPublicKeyInfo(clientPublicKeyBytes, out _);
                        derivedKeyInfo.SharedSecret = Convert.ToBase64String(serverContainer.DeriveKeyMaterial(clientContainer.PublicKey));

                        //
                        // For help with debugging, you may wish to un-comment the following:
                        //
                        // _logger.LogDebug($"Creating shared secret - {flavor}");
                        // _logger.LogDebug($"server public key for derivation: {flavor} - {Convert.ToBase64String(derivedKeyInfo.ServerPublicKey)}");
                        // _logger.LogDebug($"client public key for derivation: {flavor} - {Convert.ToBase64String(clientPublicKeyBytes)}");
                        // _logger.LogDebug($"server shared secret for derivation: {flavor} - {derivedKeyInfo.SharedSecret}");
                    }

                    return derivedKeyInfo;
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Exception computing ECDH shared secret.");
                throw;
            }
        }
        #endregion
    }
}
