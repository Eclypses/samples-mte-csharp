// ***********************************************************************
// Assembly         : MteSDRTest.Server
// Author           : Eclypses Developer
// Created          : 07-20-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-20-2022
// ***********************************************************************
// <copyright file="IMteHelper.cs" company="MteSDRTest.Server">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Threading.Tasks;

namespace MteSDRTest.Server.Helpers {
    /// <summary>
    /// Interface IMteHelper
    /// </summary>
    public interface IMteHelper {
        /// <summary>
        /// Creates the mte decoder.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="personalization">The personalization.</param>
        /// <param name="nonce">The nonce.</param>
        /// <param name="entropy">The entropy.</param>
        /// <returns>Task.</returns>
        Task CreateMteDecoder(string clientId, string personalization, ulong nonce, string entropy);

        /// <summary>
        /// Creates the mte encoder.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="personalization">The personalization.</param>
        /// <param name="nonce">The nonce.</param>
        /// <param name="entropy">The entropy.</param>
        /// <returns>Task.</returns>
        Task CreateMteEncoder(string clientId, string personalization, ulong nonce, string entropy);

        /// <summary>
        /// Instantiates the specified licensed company.
        /// </summary>
        /// <param name="licensedCompany">The licensed company.</param>
        /// <param name="licenseKey">The license key.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        bool Instantiate(string licensedCompany, string licenseKey);

        /// <summary>
        /// Decrypts the encoded string.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="encoded">The encoded string.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        Task<string> MkeDecryptString(string clientId, string encoded);

        /// <summary>
        /// Bncrypts the clear string.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="clear">The clear string.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        Task<string> MkeEncryptString(string clientId, string clear);
    }
}
