// ***********************************************************************
// Assembly         : MteSDRTest.Common
// Author           : Eclypses Developer
// Created          : 07-19-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-27-2022
// ***********************************************************************
// <copyright file="CryptoHelper.cs" company="MteSDRTest.Common">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace MteSDRTest.Common.Helpers {
    /// <summary>
    /// Class Crypto.
    /// This wraps some OS specific encryption routines. It is not
    /// supported in Blazor, so for that platform you must use
    /// JSInterop and the window.crypto.subtle methods.
    /// Implements the <see cref="MteSDRTest.Common.Helpers.ICryptoHelper" />.
    /// </summary>
    /// <seealso cref="MteSDRTest.Common.Helpers.ICryptoHelper" />
    public class CryptoHelper : ICryptoHelper {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<CryptoHelper> _logger;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoHelper" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="config">The configuration.</param>
        public CryptoHelper(ILogger<CryptoHelper> logger) {
            _logger = logger;
        }
        #endregion

        #region CreateHash

        /// <inheritdoc/>
        public string CreateHash(string data) {
            try {
                var hasher = SHA256.Create();
                var theHash = hasher.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(theHash);
            } catch (Exception ex) {
                _logger.LogCritical(ex, "Exception creating hash from string.");
                throw;
            }
        }
        #endregion

        #region CreateHash

        /// <inheritdoc/>
        public byte[] CreateHash(byte[] data) {
            try {
                var hasher = SHA256.Create();
                var theHash = hasher.ComputeHash(data);
                return theHash;
            } catch (Exception ex) {
                _logger.LogCritical(ex, "Exception creating hash from bytes.");
                throw;
            }
        }
        #endregion

        #region EncryptBytes

        /// <inheritdoc/>
        public byte[] EncryptBytes(byte[] clear, byte[] key) {
            try {
                byte[] encrypted;

                using (var sha = SHA256.Create()) {
                    byte[] keyBytes = sha.ComputeHash(key);
                    byte[] ivBytes = Guid.Empty.ToByteArray();
                    using (var aes = Aes.Create()) {
                        using (ICryptoTransform encryptor = aes.CreateEncryptor(keyBytes, ivBytes.Take(16).ToArray())) {
                            using (var ms = new MemoryStream()) {
                                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write)) {
                                    cs.Write(clear, 0, clear.Length);
                                }

                                encrypted = ms.ToArray();
                            }
                        }
                    }
                }

                return encrypted;
            } catch (Exception ex) {
                _logger.LogCritical(ex, "Exception encrypting bytes.");
                throw;
            }
        }
        #endregion

        #region DecryptBytes

        /// <inheritdoc/>
        public byte[] DecryptBytes(byte[] encrypted, byte[] key) {
            try {
                byte[] clear;
                using (var sha = SHA256.Create()) {
                    byte[] keyBytes = sha.ComputeHash(key);
                    byte[] ivBytes = Guid.Empty.ToByteArray();
                    using (var aes = Aes.Create()) {
                        using (var decryptor = aes.CreateDecryptor(keyBytes, ivBytes.Take(16).ToArray())) {
                            using (var ms = new MemoryStream()) {
                                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write)) {
                                    cs.Write(encrypted, 0, encrypted.Length);
                                }

                                clear = ms.ToArray();
                            }
                        }
                    }
                }

                return clear;
            } catch (Exception ex) {
                _logger.LogCritical(ex, "Exception decrypting bytes.");
                throw;
            }
        }
        #endregion
    }
}
