// ***********************************************************************
// Assembly         : MteSDRTest.Server
// Author           : Eclypses Developer
// Created          : 07-20-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-20-2022
// ***********************************************************************
// <copyright file="CacheHelper.cs" company="MteSDRTest.Server">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace MteSDRTest.Server.Helpers {
    /// <summary>
    /// Class Cache.
    /// This manages the MTE State Cache for each individual connected client.
    /// Since this uses the IDistributedCache interface, it can implement any
    /// .Net cache manager (which is registered in the "Startup" class.
    /// Once it retrieves a cached item, it deletes it, and when one is set
    /// It has a sliding expiration date to ensure that if not used for a period
    /// of time, it is flushed.
    /// The bytes that are stored in cache are what re-hydrates an MTE object,
    /// so this encrypts the cached item prior to storage.  The key used to encrypt
    /// is derived from a concatenation of a fixed guid, and the client id (cache key).
    /// Implements the <see cref="MteSDRTest.Server.Helpers.ICacheHelper" />.
    /// </summary>
    /// <seealso cref="MteSDRTest.Server.Helpers.ICacheHelper" />
    public class CacheHelper : ICacheHelper {
        /// <summary>
        /// The cache.
        /// </summary>
        private readonly IDistributedCache _cache;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<CacheHelper> _logger;

        /// <summary>
        /// The crypto salt - used to prepend the cacheid
        /// prior to encrypting / decrypting the cached item.
        /// </summary>
        private readonly string _cryptoSalt = "63CE0406-ED4D-4DE7-8BAA-32A99C6D9D30";

        /// <summary>
        /// The static iv - used in encryption and decryption.
        /// </summary>
        private readonly byte[] _staticIV = Guid.Parse("CE39F2C0-78AE-4E99-96D0-4E32A5E5055E").ToByteArray();

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheHelper" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        public CacheHelper(ILogger<CacheHelper> logger, IDistributedCache cache) {
            _logger = logger;
            _cache = cache;
        }
        #endregion

        #region StoreEncoderCache

        /// <summary>
        /// Stores the encoder cache.
        /// </summary>
        /// <param name="encoderCache">The encoder cache.</param>
        /// <param name="cacheId">The cache identifier.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        public async Task StoreEncoderCache(string encoderCache, string cacheId) {
            try {
                // Uncomment for debugging
                // _logger.LogDebug($"   Stored encoder cache for {cacheId} - {encoderCache}");
                var encryptedCache = EncryptPayload(encoderCache, cacheId);
                await _cache.SetStringAsync(
                    $"E-{cacheId}",
                    encryptedCache,
                    new DistributedCacheEntryOptions {
                        SlidingExpiration = new TimeSpan(1, 0, 0),
                    });
            } catch (Exception ex) {
                _logger.LogError(ex, $"Exception storing cache for E-{cacheId}.");
                throw;
            }
        }
        #endregion

        #region StoreDecoderCache

        /// <summary>
        /// Stores the decoder cache.
        /// </summary>
        /// <param name="decoderCache">The decoder cache as a Base-64 string.</param>
        /// <param name="cacheId">The cache identifier.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        public async Task StoreDecoderCache(string decoderCache, string cacheId) {
            try {
                // Uncomment for debugging
                // _logger.LogDebug($"   Stored decoder cache for {cacheId} - {decoderCache}");
                var encryptedCache = EncryptPayload(decoderCache, cacheId);
                await _cache.SetStringAsync(
                    $"D-{cacheId}",
                    encryptedCache,
                    new DistributedCacheEntryOptions {
                        SlidingExpiration = new TimeSpan(1, 0, 0),
                    });
            } catch (Exception ex) {
                _logger.LogError(ex, $"Exception storing cache for D-{cacheId}.");
                throw;
            }
        }
        #endregion

        #region TakeEncoderCache

        /// <summary>
        /// Takes the encoder cache (and removes it from the distributed cache).
        /// </summary>
        /// <param name="cacheId">The cache identifier.</param>
        /// <returns>Decrypted cached item.</returns>
        public async Task<string> TakeEncoderCache(string cacheId) {
            try {
                string item = await _cache.GetStringAsync($"E-{cacheId}");

                if (!string.IsNullOrWhiteSpace(item)) {
                    _cache.Remove($"E-{cacheId}");
                }

                var cache = DecryptPayload(item!, cacheId);

                // Uncomment for debugging
                // _logger.LogDebug($"Retrieved encoder cache for {cacheId} - {cache}");
                return cache;
            } catch (Exception ex) {
                _logger.LogError(ex, $"Exception getting cache for E-{cacheId}.");
                throw;
            }
        }
        #endregion

        #region TakeDecoderCache

        /// <summary>
        /// Takes the decoder cache (and removes it from the distributed cache).
        /// </summary>
        /// <param name="cacheId">The cache identifier.</param>
        /// <returns>Decrypted cached item.</returns>
        public async Task<string> TakeDecoderCache(string cacheId) {
            try {
                string item = await _cache.GetStringAsync($"D-{cacheId}");
                if (item is not null) {
                    _cache.Remove($"D-{cacheId}");
                }

                var cache = DecryptPayload(item!, cacheId);

                // Uncomment for debugging
                // _logger.LogDebug($"Retrieved decoder cache for {cacheId} - {cache}");
                return cache;
            } catch (Exception ex) {
                _logger.LogError(ex, $"Exception getting cache for D-{cacheId}.");
                throw;
            }
        }
        #endregion

        #region EncryptPayload

        /// <summary>
        /// Encrypts the payload prior to storing it in cache.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <returns>System.String.</returns>
        private string EncryptPayload(string payload, string clientId) {
            try {
                using (var aes = Aes.Create()) {
                    using (var encryptor = aes.CreateEncryptor(MakeCryptoKey(clientId), _staticIV)) {
                        using (var ms = new MemoryStream()) {
                            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write)) {
                                using (var sw = new StreamWriter(cs)) {
                                    sw.Write(payload);
                                }
                            }

                            return Convert.ToBase64String(ms.ToArray());
                        }
                    }
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Exception encrypting value prior to storage in cache.");
                throw;
            }
        }
        #endregion

        #region DecryptPayload

        /// <summary>
        /// Decrypts the payload after it has been retrieved from cache.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <returns>Decrypted payload.</returns>
        private string DecryptPayload(string payload, string clientId) {
            try {
                using (var aes = Aes.Create()) {
                    using (ICryptoTransform decryptor = aes.CreateDecryptor(MakeCryptoKey(clientId), _staticIV)) {
                        using (var ms = new MemoryStream(Convert.FromBase64String(payload))) {
                            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read)) {
                                using (var sr = new StreamReader(cs)) {
                                    return sr.ReadToEnd();
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Exception encrypting value prior to storage in cache.");
                throw;
            }
        }
        #endregion

        #region MakeCryptoKey

        /// <summary>
        /// Makes the crypto key from the specific client id and a prefix.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <returns>System.Byte[].</returns>
        private byte[] MakeCryptoKey(string clientId) {
            using (var sha = SHA256.Create()) {
                return sha.ComputeHash(Encoding.UTF8.GetBytes($"{_cryptoSalt}{clientId}"));
            }
        }
        #endregion
    }
}
