// ***********************************************************************
// Assembly         : MteSDRTest.Server
// Author           : Eclypses Developer
// Created          : 07-21-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-21-2022
// ***********************************************************************
// <copyright file="JwtIssuerOptions.cs" company="MteSDRTest.Server">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace MteSDRTest.Server.Models {
    /// <summary>
    /// Options for configuring the JWT (from the app settings file).
    /// </summary>
    public class JwtIssuerOptions {
        /// <summary>
        /// Gets or sets the signing key for this Jwt.
        /// </summary>
        /// <value>The Jwt secret.</value>
        public string JwtSecret { get; set; }

        /// <summary>
        /// Gets or sets the number of minutes prior to this Jwt expiring.
        /// </summary>
        /// <value>The timeout minutes.</value>
        public int TimeoutMinutes { get; set; }

        /// <summary>
        /// Gets or sets the audience for this Jwt.
        /// </summary>
        /// <value>The audience.</value>
        public string Audience { get; set; }

        /// <summary>
        /// Gets or sets the issuer of this Jwt.
        /// </summary>
        /// <value>The issuer.</value>
        public string Issuer { get; set; }
    }
}
