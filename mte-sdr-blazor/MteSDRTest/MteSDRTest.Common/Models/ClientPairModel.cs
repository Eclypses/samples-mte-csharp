// ***********************************************************************
// Assembly         : MteSDRTest.Common
// Author           : Eclypses Developer
// Created          : 07-19-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-19-2022
// ***********************************************************************
// <copyright file="ClientPairModel.cs" company="MteSDRTest.Common">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace MteSDRTest.Common.Models {
    /// <summary>
    /// Class ClientPairModel.
    /// This is sent from a client when it is
    /// asking to pair with the server. It contains
    /// the public keys needed to create entropy and
    /// the personalization string that identifies this
    /// specific client session. The personalization
    /// string is the client's contribution to the three
    /// magic values required to pair two mte endpoints.
    /// </summary>
    public class ClientPairModel {
        /// <summary>
        /// Gets or sets the client encoder public key.
        /// </summary>
        /// <value>The client encoder public key.</value>
        public string? ClientEncoderPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the client decoder public key.
        /// </summary>
        /// <value>The client decoder public key.</value>
        public string? ClientDecoderPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the personalization string.
        /// </summary>
        /// <value>The personalization.</value>
        public string? Personalization { get; set; }
    }
}
