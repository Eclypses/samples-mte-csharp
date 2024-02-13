// ***********************************************************************
// Assembly         : MteSDRTest.Client
// Author           : Eclypses Developer
// Created          : 07-19-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-19-2022
// ***********************************************************************
// <copyright file="IPayloadService.cs" company="MteSDRTest.Client">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Threading.Tasks;

namespace MteSDRTest.Client.Services {
    /// <summary>
    /// Interface IPayloadService.
    /// </summary>
    public interface IPayloadService {
        /// <summary>
        /// Conceals the specified payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        Task<string> Conceal(object payload);

        /// <summary>
        /// Reveals the specified payload.
        /// </summary>
        /// <typeparam name="T">Type of the payload to reveal.</typeparam>
        /// <param name="payload">The payload.</param>
        /// <returns>Task&lt;T&gt;.</returns>
        Task<T> Reveal<T>(string payload);
    }
}
