// ***********************************************************************
// Assembly         : MteSDRTest.Client
// Author           : Eclypses Developer
// Created          : 08-01-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 08-01-2022
// ***********************************************************************
// <copyright file="BrowserStorageHelper.cs" company="MteSDRTest.Client">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace MteSDRTest.Client.Helpers {
    /// <summary>
    /// Class BrowserStorageHelper.
    /// Allows management of the raw browser storage.
    /// Implements the <see cref="MteSDRTest.Client.Helpers.IBrowserStorageHelper" />.
    /// </summary>
    /// <seealso cref="MteSDRTest.Client.Helpers.IBrowserStorageHelper" />
    public class BrowserStorageHelper : IBrowserStorageHelper {
        /// <summary>
        /// The java script interop runtime.
        /// </summary>
        private readonly IJSRuntime _jsRuntime;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserStorageHelper"/> class.
        /// </summary>
        /// <param name="jsRuntime">The js runtime.</param>
        public BrowserStorageHelper(IJSRuntime jsRuntime) {
            _jsRuntime = jsRuntime;
        }
        #endregion

        #region GetLocalStorage

        /// <summary>
        /// Gets the raw local storage.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        public async Task<string> GetLocalStorage(string key) {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
        }
        #endregion

        #region GetSessionStorage

        /// <summary>
        /// Gets the raw session storage.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        public async Task<string> GetSessionStorage(string key) {
            return await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", key);
        }
        #endregion
    }
}
