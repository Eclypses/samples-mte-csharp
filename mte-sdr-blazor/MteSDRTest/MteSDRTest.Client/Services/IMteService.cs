// ***********************************************************************
// Assembly         : MteSDRTest.Client
// Author           : Eclypses Developer
// Created          : 07-19-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 08-08-2022
// ***********************************************************************
// <copyright file="IMteService.cs" company="MteSDRTest.Client">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Net.Http;
using System.Threading.Tasks;
using MteSDRTest.Common.Models;

namespace MteSDRTest.Client.Services {
    /// <summary>
    /// Interface IMteService.
    /// </summary>
    public interface IMteService {
        /// <summary>
        /// Instantiates the mte wasm.
        /// </summary>
        /// <param name="apiClient">The API client.</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        Task<bool> InstantiateMteWasm(HttpClient apiClient);

        /// <summary>
        /// Mkes the decrypt string.
        /// </summary>
        /// <param name="encoded">The encoded.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        Task<string> MkeDecryptString(string encoded);

        /// <summary>
        /// Mkes the encrypt string.
        /// </summary>
        /// <param name="clear">The clear.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        Task<string> MkeEncryptString(string clear);

        /// <summary>
        /// Initializes the SDR.
        /// </summary>
        /// <param name="localStorageValues">The local storage values.</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        Task<bool> InitializeSDR(ServerSDRValues localStorageValues);

        /// <summary>
        /// Reads the browser data.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="persistent">if set to <c>true</c> [persistent].</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        Task<string> ReadBrowserData(string name, bool persistent = false);

        /// <summary>
        /// Writes the browser data.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="data">The data.</param>
        /// <param name="persistent">if set to <c>true</c> [persistent].</param>
        /// <returns>Task.</returns>
        Task WriteBrowserData(string name, string data, bool persistent = false);

        /// <summary>
        /// Removes the browser data.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="persistent">if set to <c>true</c> [persistent].</param>
        /// <returns>Task.</returns>
        Task RemoveBrowserData(string name, bool persistent = false);
    }
}
