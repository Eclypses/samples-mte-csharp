// ***********************************************************************
// Assembly         : MteSDRTest.Client
// Author           : Eclypses Developer
// Created          : 07-19-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-20-2022
// ***********************************************************************
// <copyright file="MyAuthenticationStateProvider.cs" company="MteSDRTest.Client">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using MteSDRTest.Client.Services;
using MteSDRTest.Common.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace MteSDRTest.Client.Helpers {
    /// <summary>
    /// Class MyAuthenticationStateProvider.
    /// Implements the <see cref="AuthenticationStateProvider" />.
    /// </summary>
    /// <seealso cref="AuthenticationStateProvider" />
    public class MyAuthenticationStateProvider : AuthenticationStateProvider {
        /// <summary>
        /// The client user session storage key.
        /// </summary>
        private const string CLIENT_USER_SESSION_STORAGE_KEY = "UserClient";

        /// <summary>
        /// The MTE Wrapper for managing the SDR.
        /// </summary>
        private readonly IMteService _mteService;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MyAuthenticationStateProvider"/> class.
        /// </summary>
        /// <param name="mteService">The browser storage service.</param>
        public MyAuthenticationStateProvider(IMteService mteService) {
            _mteService = mteService;
        }
        #endregion

        #region GetAuthenticationStateAsync

        /// <summary>
        /// Get authentication state as an asynchronous operation
        /// from the MTE protected session storage.
        /// </summary>
        /// <returns>A Task&lt;AuthenticationState&gt; representing the asynchronous operation.</returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync() {
            //
            // Retrieve the client user object from local secure session storage.
            // If the MTE is not active yet, this will return NULL, so create an
            // un-authenticated ClaimsPrincipal.
            //
            var clientUser = await _mteService.ReadBrowserData(CLIENT_USER_SESSION_STORAGE_KEY);
            if (string.IsNullOrWhiteSpace(clientUser)) {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            ClaimsPrincipal user = MakeUserPrincipal(JsonSerializer.Deserialize<ClientUserModel>(clientUser));
            return new AuthenticationState(user);
        }
        #endregion

        #region MarkUserAsAuthenticated

        /// <summary>
        /// Marks the user as authenticated.
        /// </summary>
        /// <param name="clientUser">The client user.</param>
        /// <returns>Completed Task.</returns>
        public async Task MarkUserAsAuthenticated(ClientUserModel clientUser) {
            //
            // Store the client user object in local secure session storage.
            //
            await _mteService.WriteBrowserData(CLIENT_USER_SESSION_STORAGE_KEY, JsonSerializer.Serialize(clientUser));
            var authenticatedUser = MakeUserPrincipal(clientUser);
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }
        #endregion

        #region MarkUserAsLoggedOut

        /// <summary>
        /// Marks the user as logged out.
        /// </summary>
        /// <returns>Completed Task.</returns>
        public async Task MarkUserAsLoggedOut() {
            //
            // Remove the client user object from local secure session storage.
            //
            await _mteService.RemoveBrowserData(CLIENT_USER_SESSION_STORAGE_KEY);
            var anonymouseUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymouseUser));
            NotifyAuthenticationStateChanged(authState);
        }
        #endregion

        #region MakeUserPrincipal

        /// <summary>
        /// Makes the user principal.
        /// </summary>
        /// <param name="clientUser">The client user.</param>
        /// <returns>ClaimsPrincipal.</returns>
        private static ClaimsPrincipal MakeUserPrincipal(ClientUserModel clientUser) {
            var claims = new List<Claim>();
            if (clientUser != null) {
                claims.Add(new Claim(ClaimTypes.Name, clientUser.Name!));
                foreach (var role in clientUser.Roles!) {
                    if (!string.IsNullOrWhiteSpace(role)) {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                }
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        }
        #endregion
    }
}
