// ***********************************************************************
// Assembly         : MteSDRTest.Client
// Author           : Eclypses Developer
// Created          : 07-19-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-19-2022
// ***********************************************************************
// <copyright file="IECDHService.cs" company="MteSDRTest.Client">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Threading.Tasks;

namespace MteSDRTest.Client.Services {
    /// <summary>
    /// Interface IECDHService.
    /// </summary>
    public interface IECDHService {
        /// <summary>
        /// Computes the shared secret.
        /// </summary>
        /// <param name="flavor">The flavor.</param>
        /// <param name="pairedServerPublicKeyB64">The paired server public key in Base 64.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        Task<string> ComputeSharedSecret(string flavor, string pairedServerPublicKeyB64);

        /// <summary>
        /// Gets the ECDH instance public key.
        /// </summary>
        /// <param name="flavor">The flavor.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        Task<string> GetEcdh(string flavor);
    }
}
