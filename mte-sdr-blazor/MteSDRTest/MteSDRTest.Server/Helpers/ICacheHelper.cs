// ***********************************************************************
// Assembly         : MteSDRTest.Server
// Author           : Eclypses Developer
// Created          : 07-20-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-20-2022
// ***********************************************************************
// <copyright file="ICacheHelper.cs" company="MteSDRTest.Server">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Threading.Tasks;

namespace MteSDRTest.Server.Helpers {
    /// <summary>
    /// Interface ICacheHelper
    /// </summary>
    public interface ICacheHelper {
        /// <summary>
        /// Stores the decoder cache.
        /// </summary>
        /// <param name="decoderCache">The decoder cache.</param>
        /// <param name="cacheId">The cache identifier.</param>
        /// <returns>Task.</returns>
        Task StoreDecoderCache(string decoderCache, string cacheId);

        /// <summary>
        /// Stores the encoder cache.
        /// </summary>
        /// <param name="encoderCache">The encoder cache.</param>
        /// <param name="cacheId">The cache identifier.</param>
        /// <returns>Task.</returns>
        Task StoreEncoderCache(string encoderCache, string cacheId);

        /// <summary>
        /// Takes the decoder cache.
        /// </summary>
        /// <param name="cacheId">The cache identifier.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        Task<string> TakeDecoderCache(string cacheId);

        /// <summary>
        /// Takes the encoder cache.
        /// </summary>
        /// <param name="cacheId">The cache identifier.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        Task<string> TakeEncoderCache(string cacheId);
    }
}
