// ***********************************************************************
// Assembly         : MteSDRTest.Server
// Author           : Eclypses Developer
// Created          : 07-20-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-20-2022
// ***********************************************************************
// <copyright file="DataExchangeService.cs" company="MteSDRTest.Server">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MteSDRTest.Common.Helpers;
using MteSDRTest.Common.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace MteSDRTest.Server.Services {
    /// <summary>
    /// Class DataExchangeService.
    /// This is an arbitrary key/value store for use by a client.
    /// It does not care what the data is, however, if it is sensitive
    /// it should be encrypted at the client prior to sending.
    /// Implements the <see cref="MteSDRTest.Server.Services.IDataExchangeService" />.
    /// </summary>
    /// <seealso cref="MteSDRTest.Server.Services.IDataExchangeService" />
    public class DataExchangeService : IDataExchangeService {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<DataExchangeService> _logger;

        /// <summary>
        /// The cache that persists the items we wish to store.
        /// </summary>
        private readonly IDistributedCache _cache;

        /// <summary>
        /// The cryptography wrapper.
        /// </summary>
        private readonly ICryptoHelper _crypto;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DataExchangeService" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="ecdh">The ecdh wrapper.</param>
        /// <param name="crypto">The crypto wrapper.</param>
        public DataExchangeService(ILogger<DataExchangeService> logger, IDistributedCache cache, ICryptoHelper crypto) {
            _logger = logger;
            _cache = cache;
            _crypto = crypto;
        }
        #endregion

        #region RetrieveValue

        /// <summary>
        /// Retrieves the value - if not found, one is created.
        /// This value is encrypted with a one-time key that is
        /// created using the ECDH algorithm.  The client sends
        /// in a public key and this is responsible for
        /// sending back a nonce value.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Completed Task.</returns>
        public async Task RetrieveValue(DataExchangeModel data) {
            try {
                //
                // Get a value for entropy for this client (data.ItemOwner + data.ItemKey)
                //
                data.Value = await GetTheValueFromCache(MakeCacheKey(data.ItemOwner, data.ItemKey));
            } catch (Exception ex) {
                _logger.LogError(ex, $"Exception getting value for owner: {data.ItemOwner} with key of {data.ItemKey}.");
            }
        }
        #endregion

        #region GetTheValueFromCache

        /// <summary>
        /// Gets the value from cache.
        /// That value is actually an encrypted ServerSDRValues object with
        /// an entropy and a nonce for use by the client when creating an
        /// Eclypses SDR object.
        /// If it cannot be found, a new cryptographically secure random value
        /// is created for entropy and a value for nonce.
        /// It is then encrypted, and stored in cache.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <returns>byte[].</returns>
        private async Task<string> GetTheValueFromCache(string cacheKey) {
            string theValue = await _cache.GetStringAsync(cacheKey);
            byte[] cacheEncryptionKeyBytes = MakeCacheEncryptionKeyBytes(cacheKey);
            if (string.IsNullOrWhiteSpace(theValue)) {
                var localStorageValues = new ServerSDRValues();

                //
                // Create 32 bytes of entropy.
                //
                byte[] workstationEntropyBytes = RandomNumberGenerator.GetBytes(32);

                //
                // Create a nonce value and populate the object to return to the workstation.
                //
                localStorageValues.WorkstationLocalStorageNonce = DateTime.Now.ToString("yyMMddHHmmss");
                localStorageValues.WorkstationLocalStorageEntropy = Convert.ToBase64String(workstationEntropyBytes);

                //
                // Serialize and Encrypt that object into a byte array.
                //
                string json = JsonSerializer.Serialize(localStorageValues);
                byte[] clearBytes = Encoding.UTF8.GetBytes(json);
                byte[] encryptedBytes = _crypto.EncryptBytes(clearBytes, cacheEncryptionKeyBytes);

                //
                // Store the encrypted bytes in cache as a Base-64 encoded string.
                //
                await _cache.SetStringAsync(cacheKey, Convert.ToBase64String(encryptedBytes));

                //
                // Return the json serialized item to send back to the browser.
                //
                return json;
            } else {
                //
                // Retrieve the Base-64 string of the encrypted information for this client
                // and decode it back to bytes and eventually to a json string of the workstation data.
                //
                byte[] encryptedBytes = Convert.FromBase64String(theValue);
                var clearBytes = _crypto.DecryptBytes(encryptedBytes, cacheEncryptionKeyBytes);
                var json = Encoding.UTF8.GetString(clearBytes);
                return json;
            }
        }
        #endregion

        #region MakeCacheKey

        /// <summary>
        /// Makes the cache key from the owner and the 'key'.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="key">The key.</param>
        /// <returns>System.String of the concatenated cache key.</returns>
        private string MakeCacheKey(string owner, string key) => $"{owner}~{key}";
        #endregion

        #region MakeCacheKeyBytes

        /// <summary>
        /// The static key part - used for part of the entropy encryption key.
        /// </summary>
        private static readonly string StaticKeyPart = "CA4E41E9-29F8-430D-93B2-48B9C2A4B913";

        /// <summary>
        /// Makes the cache encryption key used to encrypt and decrypt
        /// the value stored in cache for this users entropy.
        /// </summary>
        /// <param name="salt">The salt for the key.</param>
        /// <returns>System.Byte[].</returns>
        private byte[] MakeCacheEncryptionKeyBytes(string salt) {
            return Encoding.UTF8.GetBytes($"{salt}{StaticKeyPart}");
        }
        #endregion
    }
}
