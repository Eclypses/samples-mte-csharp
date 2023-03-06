// ***********************************************************************
// Assembly         : MteSDRTest.Client
// Author           : Eclypses Developer
// Created          : 07-20-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-20-2022
// ***********************************************************************
// <copyright file="StateContainer.cs" company="MteSDRTest.Client">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Security.Claims;

namespace MteSDRTest.Client.Models {
    /// <summary>
    /// Class StateContainer.
    /// </summary>
    public class StateContainer {
        /// <summary>
        /// Gets or sets the user's ClaimPrincipal.
        /// </summary>
        /// <value>The user.</value>
        public ClaimsPrincipal TheUserPrincipal { get; set; }

        /// <summary>
        /// Resets this instance of the StateContainer.
        /// </summary>
        public void Reset() {
            TheUserPrincipal = null;
        }
    }
}
