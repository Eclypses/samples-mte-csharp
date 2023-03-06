// ***********************************************************************
// Assembly         : MteSDRTest.Common
// Author           : Eclypses Developer
// Created          : 07-19-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-19-2022
// ***********************************************************************
// <copyright file="ICryptoHelper.cs" company="MteSDRTest.Common">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace MteSDRTest.Common.Helpers {
    /// <summary>
    /// Interface ICrypto - Provides contract for common cryptography methods.
    /// </summary>
    public interface ICryptoHelper {
        /// <summary>
        /// Creates the SHA256 hash from a string.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>System.Byte[].</returns>
        byte[] CreateHash(byte[] data);

        /// <summary>
        /// Creates the SHA256 hash from a byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Base64 representation of the hashed value.</returns>
        string CreateHash(string data);

        /// <summary>
        /// Encrypts the clear bytes using the designated key.
        /// </summary>
        /// <param name="clear">The clear byte array.</param>
        /// <param name="key">The encryption key.</param>
        /// <returns>System.Byte[] of the encrypted data.</returns>
        byte[] EncryptBytes(byte[] clear, byte[] key);

        /// <summary>
        /// Decrypts an encrypted byte array using the designated key.
        /// </summary>
        /// <param name="encrypted">The encrypted bytes.</param>
        /// <param name="key">The decryption key.</param>
        /// <returns>System.Byte[] of the decrypted data.</returns>
        byte[] DecryptBytes(byte[] encrypted, byte[] key);
    }
}
