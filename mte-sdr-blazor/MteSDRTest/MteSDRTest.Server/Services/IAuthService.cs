// ***********************************************************************
// Assembly         : MteSDRTest.Server
// Author           : Eclypses Developer
// Created          : 08-01-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 08-01-2022
// ***********************************************************************
// <copyright file="IAuthService.cs" company="MteSDRTest.Server">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Threading.Tasks;
using MteSDRTest.Common.Models;

namespace MteSDRTest.Server.Services {
    /// <summary>
    /// Interface IAuthService
    /// </summary>
    public interface IAuthService {
        /// <summary>
        /// Authorizes the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Task&lt;ClientUserModel&gt;.</returns>
        Task<ClientUserModel> Authorize(ClientCredentials model);
    }
}
