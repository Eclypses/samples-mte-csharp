// ***********************************************************************
// Assembly         : MteSDRTest.Common
// Author           : Eclypses Developer
// Created          : 07-19-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-21-2022
// ***********************************************************************
// <copyright file="ClientCredentials.cs" company="MteSDRTest.Common">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.ComponentModel.DataAnnotations;

namespace MteSDRTest.Common.Models {
    /// <summary>
    /// Class ClientCredentials.
    /// This is sent to the server to authenticate
    /// and identify this client. It is protected
    /// in flight by the MTE.
    /// </summary>
    public class ClientCredentials {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        [Required]
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        [Required]
        public string? Password { get; set; }
    }
}
