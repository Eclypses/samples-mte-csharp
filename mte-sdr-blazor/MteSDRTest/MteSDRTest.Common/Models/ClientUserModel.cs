// ***********************************************************************
// Assembly         : MteSDRTest.Common
// Author           : Eclypses Developer
// Created          : 07-21-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-21-2022
// ***********************************************************************
// <copyright file="ClientUserModel.cs" company="MteSDRTest.Common">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Collections.Generic;

namespace MteSDRTest.Common.Models {
    /// <summary>
    /// Class ClientUserModel.
    /// This is returned to the client after successful
    /// authentication. It contains the JWT that will be
    /// used in subsequent API calls as the auth token.
    /// </summary>
    public class ClientUserModel {
        /// <summary>
        /// Gets or sets the name of the logged in client.
        /// </summary>
        /// <value>The name.</value>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the client authentication token (the jwt).
        /// </summary>
        /// <value>The client authentication token.</value>
        public string? ClientAuthToken { get; set; }

        /// <summary>
        /// Gets or sets the list of roles.
        /// </summary>
        /// <value>The roles.</value>
        public List<string>? Roles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ClientUserModel"/> request was successful.
        /// </summary>
        /// <value><c>true</c> if successful; otherwise, <c>false</c>.</value>
        public bool Success { get; set; }
    }
}
