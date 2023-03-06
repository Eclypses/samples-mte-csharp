// ***********************************************************************
// Assembly         : MteSDRTest.Client
// Author           : Eclypses Developer
// Created          : 08-08-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 08-08-2022
// ***********************************************************************
// <copyright file="ISDRService.cs" company="MteSDRTest.Client">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Net.Http;
using System.Threading.Tasks;
using MteSDRTest.Common.Models;

namespace MteSDRTest.Client.Services {
    /// <summary>
    /// Interface ISDRService.
    /// </summary>
    public interface ISDRService {
        /// <summary>
        /// Obtains the local storage information
        /// from the API to create a consistant local storage SDR.
        /// </summary>
        /// <param name="apiClient">The API client.</param>
        /// <returns>Task&lt;ServerSDRValues&gt;.</returns>
        Task<ServerSDRValues> ObtainLocalStorageInformation(HttpClient apiClient);
    }
}
