// ***********************************************************************
// Assembly         : MteSDRTest.Client
// Author           : Eclypses Developer
// Created          : 08-01-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 08-01-2022
// ***********************************************************************
// <copyright file="IBrowserStorageHelper.cs" company="MteSDRTest.Client">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Threading.Tasks;

namespace MteSDRTest.Client.Helpers {
    /// <summary>
    /// Interface IBrowserStorageHelper.
    /// </summary>
    public interface IBrowserStorageHelper {
        /// <summary>
        /// Gets the raw local storage.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        Task<string> GetLocalStorage(string key);

        /// <summary>
        /// Gets the raw session storage.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        Task<string> GetSessionStorage(string key);
    }
}
