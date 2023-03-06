// ***********************************************************************
// Assembly         : MteSDRTest.Server
// Author           : Eclypses Developer
// Created          : 07-20-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-20-2022
// ***********************************************************************
// <copyright file="IPayloadHelper.cs" company="MteSDRTest.Server">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MteSDRTest.Server.Helpers {
    /// <summary>
    /// Interface IPayloadHelper
    /// </summary>
    public interface IPayloadHelper {
        /// <summary>
        /// Conceals the specified object payload.
        /// </summary>
        /// <param name="request">The http Request.</param>
        /// <param name="clear">The clear object.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        Task<string> Conceal(HttpRequest request, object clear);

        /// <summary>
        /// Reveals the specified request payload.
        /// </summary>
        /// <typeparam name="T">Type of the object to reveal.</typeparam>
        /// <param name="request">The http Request.</param>
        /// <returns>Task&lt;T&gt;.</returns>
        Task<T> Reveal<T>(HttpRequest request);
    }
}
