// ***********************************************************************
// Assembly         : MteSDRTest.Client
// Author           : Eclypses Developer
// Created          : 07-19-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-19-2022
// ***********************************************************************
// <copyright file="IWebHelper.cs" company="MteSDRTest.Client">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Net.Http;
using System.Threading.Tasks;

namespace MteSDRTest.Client.Helpers {
    /// <summary>
    /// Interface IWebHelper.
    /// </summary>
    public interface IWebHelper {
        /// <summary>
        /// Gets a string response from a server.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="route">The route.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        Task<string> GetFromServer(HttpClient httpClient, string route);

        /// <summary>
        /// Gets a typed object from the server.
        /// </summary>
        /// <typeparam name="T">The type of the object to GET.</typeparam>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="route">The route.</param>
        /// <returns>Task&lt;System.Nullable&lt;T&gt;&gt;.</returns>
        Task<T> GetFromServer<T>(HttpClient httpClient, string route);

        /// <summary>
        /// Posts a reeust to the server with no payload.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="route">The route.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        Task<string> PostToServer(HttpClient httpClient, string route);

        /// <summary>
        /// Posts a specified payload as a string to the server.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="route">The route.</param>
        /// <param name="payload">The payload.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        Task<string> PostToServer(HttpClient httpClient, string route, string payload);

        /// <summary>
        /// Posts an empty request to the server and receives back a typed response.
        /// </summary>
        /// <typeparam name="R">The type of the response to receive.</typeparam>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="route">The route.</param>
        /// <returns>Task&lt;System.Nullable&lt;R&gt;&gt;.</returns>
        Task<R> PostToServer<R>(HttpClient httpClient, string route);

        /// <summary>
        /// Posts a typed payload to the server and receives back a typed response.
        /// </summary>
        /// <typeparam name="T">The type of the payload to send.</typeparam>
        /// <typeparam name="R">The type of the response to receive.</typeparam>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="route">The route.</param>
        /// <param name="payload">The payload.</param>
        /// <returns>Task&lt;System.Nullable&lt;R&gt;&gt;.</returns>
        Task<R> PostToServer<T, R>(HttpClient httpClient, string route, T payload);
    }
}
