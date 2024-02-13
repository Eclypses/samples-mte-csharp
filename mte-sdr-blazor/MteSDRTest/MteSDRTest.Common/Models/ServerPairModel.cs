// ***********************************************************************
// Assembly         : MteSDRTest.Common
// Author           : Eclypses Developer
// Created          : 07-19-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-19-2022
// ***********************************************************************
// <copyright file="ServerPairModel.cs" company="MteSDRTest.Common">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace MteSDRTest.Common.Models {
    /// <summary>
    /// Class ServerPairModel. This is returned from the server
    /// when a client initiates a "pairing" request.  The public
    /// keys are used to then generate the entropy at the client
    /// which is different for Encoders and Decoders. The Nonce is
    /// the same for both Encoder and Decoder  and it is the server's
    /// contribution to the three magic values for pairing MTEs.
    /// </summary>
    public class ServerPairModel {
        /// <summary>
        /// Gets or sets the server Encoder public key.
        /// </summary>
        /// <value>The server Encoder public key.</value>
        public string? ServerEncoderPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the server Decoder public key.
        /// </summary>
        /// <value>The server Decoder public key.</value>
        public string? ServerDecoderPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the nonce that is returned from the server.
        /// </summary>
        /// <value>The nonce.</value>
        public ulong? Nonce { get; set; }
    }
}
