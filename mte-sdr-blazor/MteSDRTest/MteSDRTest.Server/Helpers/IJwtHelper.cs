// ***********************************************************************
// Assembly         : MteSDRTest.Server
// Author           : Eclypses Developer
// Created          : 07-21-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-21-2022
// ***********************************************************************
// <copyright file="IJwtHelper.cs" company="MteSDRTest.Server">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Security.Claims;
using MteSDRTest.Common.Models;

namespace MteSDRTest.Server.Helpers {
    /// <summary>
    /// Interface IJwtHelper
    /// </summary>
    public interface IJwtHelper {
        /// <summary>
        /// Creates the JWT.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>System.String.</returns>
        string CreateTheJWT(ClientUserModel user);

        /// <summary>
        /// Extracts the claims principal.
        /// </summary>
        /// <param name="jwt">The JWT.</param>
        /// <returns>ClaimsPrincipal.</returns>
        ClaimsPrincipal ExtractClaimsPrincipal(string jwt);

        /// <summary>
        /// Extracts the current user.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns>ClientUserModel.</returns>
        ClientUserModel ExtractCurrentUser(ClaimsPrincipal principal);

        /// <summary>
        /// Pulls the JWT from query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>System.String.</returns>
        string PullJwtFromQuery(string query);
    }
}
