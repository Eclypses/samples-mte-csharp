// ***********************************************************************
// Assembly         : MteSDRTest.Server
// Author           : Eclypses Developer
// Created          : 07-20-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-20-2022
// ***********************************************************************
// <copyright file="MteHelper.cs" company="MteSDRTest.Server">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Text;
using System.Threading.Tasks;
using Eclypses.MTE;
using Microsoft.Extensions.Logging;

namespace MteSDRTest.Server.Helpers {
    /// <summary>
    /// Class MteHelper.
    /// Implements the <see cref="MteSDRTest.Server.Helpers.IMteHelper" />
    /// </summary>
    /// <seealso cref="MteSDRTest.Server.Helpers.IMteHelper" />
    public class MteHelper : IMteHelper {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<MteHelper> _logger;

        /// <summary>
        /// The cache helper - this wraps the IDistributedCache that holds MTE state.
        /// </summary>
        private readonly ICacheHelper _cache;

        /// <summary>
        /// Gets or sets the mte base.
        /// </summary>
        /// <value>The mte base.</value>
        private static MteBase _mteBase { get; set; }

        /// <summary>
        /// The encoder lock - MTE is single threaded, so throw a lock around its use.
        /// </summary>
        private static object _encoderLock = new object();

        /// <summary>
        /// The decoder lock - MTE is single threaded, so throw a lock around its use.
        /// </summary>
        private static object _decoderLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="MteHelper" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        public MteHelper(ILogger<MteHelper> logger, ICacheHelper cache) {
            _logger = logger;
            _cache = cache;
        }

        #region Instantiate

        /// <summary>
        /// Instantiates a single use MTE for the specified licensed company.
        /// This MUST be called before any other MTE methods can be used, usually as soon as the website loads.
        /// </summary>
        /// <param name="licensedCompany">The licensed company.</param>
        /// <param name="licenseKey">The license key.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        /// <exception cref="System.ApplicationException">Error instantiating MTE: {_mteBase.GetStatusName(licenseStatus)} - {_mteBase.GetStatusDescription(licenseStatus)}</exception>
        public bool Instantiate(string licensedCompany, string licenseKey) {
            try {
                //
                // Assign _mteBase variable so we can access mte static values.
                //
                _mteBase = new MteBase();
                if (!_mteBase.InitLicense(licensedCompany, licenseKey)) {
                    var licenseStatus = MteStatus.mte_status_license_error;
                    throw new ApplicationException($"Error instantiating MTE: {_mteBase.GetStatusName(licenseStatus)} - {_mteBase.GetStatusDescription(licenseStatus)}");
                }

                return true;
            } catch (Exception ex) {
                _logger.LogError(ex, "Exception - MteBase is not valid.");
                throw;
            }
        }
        #endregion

        #region CreateMteDecoder

        /// <summary>
        /// Creates the MKE decoder. Since we are pairing with a Java Script client,
        /// We must use a string value for entropy (not a byte array) and it should
        /// be used as a series of byte values of the string, not a decoded Base-64 string.
        /// That is because when WASM creates it's entropy, it needs a string and just
        /// converts it to bytes, so we must do the same on the server.
        /// </summary>
        /// <param name="clientId">The client identifier - used to get the proper MTE state.</param>
        /// <param name="personalization">The personalization token to use for this decoder.</param>
        /// <param name="nonce">The nonce value to use for this decoder.</param>
        /// <param name="entropy">The entropy value to use for this decoder.</param>
        /// <returns>MteDec.</returns>
        /// <exception cref="System.ApplicationException">Error instantiating MKE Decoder: {_mteBase!.GetStatusName(status)} - {_mteBase.GetStatusDescription(status)}</exception>
        public async Task CreateMteDecoder(string clientId, string personalization, ulong nonce, string entropy) {
            try {
                //
                // Uncomment to assist with debugging
                // _logger.LogDebug($"Decoder Personalization: {personalization}");
                // _logger.LogDebug($"Decoder Nonce: {nonce}");
                // _logger.LogDebug($"Decoder Entropy: {entropy}");
                //
                string state;
                lock (_decoderLock) {
                    var mteDecoder = new MteDec(sWindow: -63);
                    mteDecoder.SetEntropy(Encoding.UTF8.GetBytes(entropy));
                    mteDecoder.SetNonce(nonce);

                    var status = mteDecoder.Instantiate(personalization);
                    if (status != MteStatus.mte_status_success) {
                        throw new ApplicationException($"Error instantiating MKE Decoder: {_mteBase!.GetStatusName(status)} - {_mteBase.GetStatusDescription(status)}");
                    }

                    state = mteDecoder.SaveStateB64();

                    //
                    // Uncomment to assist with debugging
                    // _logger.LogDebug($"API - Decoder state after instantiation: {state}");
                    //
                    mteDecoder.Uninstantiate();
                    mteDecoder = null;
                }

                await _cache.StoreDecoderCache(state, clientId);
            } catch (Exception ex) {
                _logger.LogError(ex, "Exception - MkeDecoder is not valid.");
                throw;
            }
        }
        #endregion

        #region CreateMteEncoder

        /// <summary>
        /// Creates the MKE encoder. Since we are pairing with a Java Script client,
        /// We must use a string value for entropy (not a byte array) and it should
        /// be used as a series of byte values of the string, not a decoded Base-64 string.
        /// That is because when WASM creates it's entropy, it needs a string and just
        /// converts it to bytes, so we must do the same on the server.
        /// </summary>
        /// <param name="clientId">The client identifier - used to get the proper MTE state.</param>
        /// <param name="personalization">The personalization token to use for this encoder.</param>
        /// <param name="nonce">The nonce value to use for this encoder.</param>
        /// <param name="entropy">The entropy value to use for this encoder.</param>
        /// <returns>MteEnc.</returns>
        /// <exception cref="System.ApplicationException">Error instantiating MKE Encoder: {_mteBase!.GetStatusName(status)} - {_mteBase.GetStatusDescription(status)}</exception>
        public async Task CreateMteEncoder(string clientId, string personalization, ulong nonce, string entropy) {
            try {
                //
                // Uncomment to assist with debugging
                // _logger.LogDebug($"Encoder Personalization: {personalization}");
                // _logger.LogDebug($"Encoder Nonce: {nonce}");
                // _logger.LogDebug($"Encoder Entropy: {entropy}");
                //
                string state;
                lock (_encoderLock) {
                    var mteEncoder = new MteEnc();
                    mteEncoder.SetEntropy(Encoding.UTF8.GetBytes(entropy));
                    mteEncoder.SetNonce(nonce);
                    var status = mteEncoder.Instantiate(personalization);
                    if (status != MteStatus.mte_status_success) {
                        throw new ApplicationException($"Error instantiating MKE Encoder: {_mteBase!.GetStatusName(status)} - {_mteBase.GetStatusDescription(status)}");
                    }

                    state = mteEncoder.SaveStateB64();

                    //
                    // Uncomment to assist with debugging
                    // _logger.LogDebug($"API - Encoder state after instantiation: {state}");
                    //
                    mteEncoder.Uninstantiate();
                    mteEncoder = null;
                }

                await _cache.StoreEncoderCache(state, clientId);
            } catch (Exception ex) {
                _logger.LogError(ex, "Exception - MkeEncoder is not valid.");
                throw;
            }
        }
        #endregion

        #region MkeEncryptString

        /// <summary>
        /// Uses the MkeEncoder to encrypt a clear payload and return it as the encoded string.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="clear">The clear string to encode.</param>
        /// <returns>Task&lt;System.Byte[]&gt; of the encoded string.</returns>
        /// <exception cref="System.ApplicationException">Error Encoding bytes: {_mteBase!.GetStatusName(status)} - {_mteBase!.GetStatusDescription(status)}</exception>
        public async Task<string> MkeEncryptString(string clientId, string clear) {
            try {
                var state = await _cache.TakeEncoderCache(clientId);
                string encoded;
                lock (_encoderLock) {
                    //
                    // Create a new encoder and re-hydrate the state that we retrieve from cache for this client.
                    //
                    var mkeEncoder = new MteMkeEnc();

                    mkeEncoder.RestoreStateB64(state);

                    MteStatus status = MteStatus.mte_status_success;
                    encoded = mkeEncoder!.EncodeB64(clear, out status);
                    if (status != MteStatus.mte_status_success) {
                        throw new ApplicationException($"Error Encoding bytes: {_mteBase!.GetStatusName(status)} - {_mteBase!.GetStatusDescription(status)}");
                    }

                    state = mkeEncoder!.SaveStateB64();
                    mkeEncoder!.Uninstantiate();
                    mkeEncoder = null;
                }

                await _cache.StoreEncoderCache(state!, clientId);
                return encoded;
            } catch (Exception ex) {
                _logger.LogError(ex, "Exception - could not encode.");
                throw;
            }
        }
        #endregion

        #region MkeDecryptString

        /// <summary>
        /// Uses the MkeDecoder to decrypt an encoded payload and return it as the clear string.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="encoded">The encoded string.</param>
        /// <returns>Task&lt;System.Byte[]&gt; of the original clear string.</returns>
        /// <exception cref="System.ApplicationException">Error Decoding byte array: {_mteBase!.GetStatusName(status)} - {_mteBase.GetStatusDescription(status)}</exception>
        public async Task<string> MkeDecryptString(string clientId, string encoded) {
            try {
                var state = await _cache.TakeDecoderCache(clientId);
                string clear;
                lock (_decoderLock) {
                    var mkeDecoder = new MteMkeDec(sWindow: -63);

                    mkeDecoder.RestoreStateB64(state);
                    MteStatus status = MteStatus.mte_status_success;
                    clear = mkeDecoder!.DecodeStrB64(encoded, out status);
                    if (status != MteStatus.mte_status_success) {
                        throw new ApplicationException($"Error Decoding byte array: {_mteBase!.GetStatusName(status)} - {_mteBase.GetStatusDescription(status)}");
                    }

                    state = mkeDecoder!.SaveStateB64();
                    mkeDecoder!.Uninstantiate();
                    mkeDecoder = null;
                }

                await _cache.StoreDecoderCache(state!, clientId);
                return clear;
            } catch (Exception ex) {
                _logger.LogError(ex, "Exception - could not decode.");
                throw;
            }
        }
        #endregion
    }
}
