// ***********************************************************************
// Assembly         : MteSDRTest.Client
// Author           : Eclypses Developer
// Created          : 07-20-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-20-2022
// ***********************************************************************
// <copyright file="IAuthService.cs" company="MteSDRTest.Client">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Threading.Tasks;
using MteSDRTest.Common.Models;

namespace MteSDRTest.Client.Services {
    /// <summary>
    /// Interface IAuthService.
    /// </summary>
    public interface IAuthService {
        /// <summary>
        /// Logs in the specified login model.
        /// </summary>
        /// <param name="loginModel">The login model.</param>
        /// <returns>Task&lt;ClientUserModel&gt;.</returns>
        Task<ClientUserModel> Login(ClientCredentials loginModel);

        /// <summary>
        /// Logs out this instance.
        /// </summary>
        /// <returns>Task.</returns>
        Task Logout();
    }
}
