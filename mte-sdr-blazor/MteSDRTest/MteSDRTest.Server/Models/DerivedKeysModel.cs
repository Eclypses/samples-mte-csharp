// ***********************************************************************
// Assembly         : MteSDRTest.Server
// Author           : Eclypses Developer
// Created          : 07-20-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-20-2022
// ***********************************************************************
// <copyright file="DerivedKeysModel.cs" company="MteSDRTest.Server">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace MteSDRTest.Server.Models {
    /// <summary>
    /// Class DerivedKeysModel.
    /// This is passed between the Elliptical Curve Diffie-Hellman helper
    /// and the mte pairing controller.
    /// </summary>
    public class DerivedKeysModel {
        /// <summary>
        /// Gets or sets the server public key.
        /// </summary>
        /// <value>The server public key.</value>
        public byte[] ServerPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the shared secret as a string.
        /// This string is the Base-64 representation of the actual
        /// shared secret bytes, but it must be passed to the
        /// mte creation methods as the byte representation of
        /// the literal string since java script treats it as such.
        /// </summary>
        /// <value>The shared secret.</value>
        public string SharedSecret { get; set; }
    }
}
