// ***********************************************************************
// Assembly         : MteSDRTest.Server
// Author           : Eclypses Developer
// Created          : 07-20-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-20-2022
// ***********************************************************************
// <copyright file="IECDHHelper.cs" company="MteSDRTest.Server">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using MteSDRTest.Server.Models;

namespace MteSDRTest.Server.Helpers {
    /// <summary>
    /// Interface IECDHHelper.
    /// </summary>
    public interface IECDHHelper {
        /// <summary>
        /// Computes the shared secret.
        /// </summary>
        /// <param name="flavor">The flavor.</param>
        /// <param name="clientPublicKey">The client public key.</param>
        /// <returns>DerivedKeysModel.</returns>
        DerivedKeysModel ComputeSharedSecret(string flavor, string clientPublicKey);
    }
}
