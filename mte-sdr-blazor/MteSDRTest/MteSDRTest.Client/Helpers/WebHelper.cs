// ***********************************************************************
// Assembly         : MteSDRTest.Client
// Author           : Eclypses Developer
// Created          : 07-19-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 08-10-2022
// ***********************************************************************
// <copyright file="WebHelper.cs" company="MteSDRTest.Client">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MteSDRTest.Client.Helpers {
    /// <summary>
    /// Class WebHelper.
    /// Manages Http calls to the API Server.
    /// Implements the <see cref="MteSDRTest.Client.Helpers.IWebHelper" />.
    /// </summary>
    /// <seealso cref="MteSDRTest.Client.Helpers.IWebHelper" />
    public class WebHelper : IWebHelper {
        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<WebHelper> _logger;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHelper" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public WebHelper(ILogger<WebHelper> logger) {
            _logger = logger;
        }
        #endregion

        #region GetFromServer<T>

        /// <summary>
        /// Sends a GET request to the route on the designated httpClient.
        /// </summary>
        /// <typeparam name="T">The type of the object expected to be returned.</typeparam>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="route">The route.</param>
        /// <returns>Task&lt;System.Nullable&lt;T&gt;&gt;.</returns>
        /// <exception cref="System.TimeoutException">Timeout to the GAPI for this JWT.</exception>
        /// <exception cref="System.ApplicationException">Get to {route} failed with HttpStatus code of {result!.StatusCode}.</exception>
        public async Task<T> GetFromServer<T>(HttpClient httpClient, string route) {
            try {
                var result = await httpClient.GetAsync(route);
                if (result != null && result.IsSuccessStatusCode) {
                    string content = await result.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(content)) {
                        if (typeof(T) == typeof(string)) {
                            return (T)Convert.ChangeType(content, typeof(T));
                        } else {
                            return JsonSerializer.Deserialize<T>(content);
                        }
                    } else {
                        return default;
                    }
                } else if ((int)result.StatusCode == StatusCodes.Status419AuthenticationTimeout) {
                    throw new TimeoutException("Timeout to the GAPI for this JWT.");
                } else {
                    throw new ApplicationException($"Get to {route} failed with HttpStatus code of {result!.StatusCode}");
                }
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error Getting from server at {route}.");
                throw;
            }
        }
        #endregion

        #region GetFromServer - string

        /// <summary>
        /// Sends a GET request to the route on the designated httpClient.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="route">The route.</param>
        /// <returns>Task&lt;System.Nullable&lt;System.String&gt;&gt; of an MKE protected payload.</returns>
        /// <exception cref="System.TimeoutException">Timeout to the GAPI for this JWT.</exception>
        /// <exception cref="System.ApplicationException">Get to {route} failed with HttpStatus code of {result!.StatusCode}.</exception>
        public async Task<string> GetFromServer(HttpClient httpClient, string route) {
            try {
                var result = await httpClient.GetAsync(route);
                if (result != null && result.IsSuccessStatusCode) {
                    string content = await result.Content.ReadAsStringAsync();
                    return content;
                } else if ((int)result.StatusCode == StatusCodes.Status419AuthenticationTimeout) {
                    throw new TimeoutException("Timeout to the GAPI for this JWT.");
                } else {
                    throw new ApplicationException($"Get to {route} failed with HttpStatus code of {result!.StatusCode}");
                }
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error Getting from server at {route}.");
                throw;
            }
        }
        #endregion

        #region PostToServer<R>

        /// <summary>
        /// Send a POST request to the route on the designated http client with no payload.
        /// </summary>
        /// <typeparam name="R">The type of the object expected to be returned.</typeparam>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="route">The route.</param>
        /// <returns>Task&lt;System.Nullable&lt;R&gt;&gt;.</returns>
        /// <exception cref="System.TimeoutException">Timeout to the GAPI for this JWT.</exception>
        /// <exception cref="System.ApplicationException">Error POSTing to {route} {result.StatusCode}.</exception>
        public async Task<R> PostToServer<R>(HttpClient httpClient, string route) {
            try {
                var result = await httpClient.PostAsync(route, null);
                if (result != null && result.IsSuccessStatusCode) {
                    string returnJson = await result!.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(returnJson)) {
                        return default;
                    } else {
                        return JsonSerializer.Deserialize<R>(returnJson);
                    }
                } else if ((int)result.StatusCode == StatusCodes.Status419AuthenticationTimeout) {
                    throw new TimeoutException("Timeout to the GAPI for this JWT.");
                } else {
                    throw new ApplicationException($"Error POSTing to {route} {result.StatusCode}");
                }
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error Posting to server at {route}.");
                throw;
            }
        }
        #endregion

        #region PostToServer<T, R>

        /// <summary>
        /// Send a POST request to the route on the designated http client with a payload.
        /// </summary>
        /// <typeparam name="T">The type of the payload object.</typeparam>
        /// <typeparam name="R">The type of the object expected to be returned.</typeparam>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="route">The route.</param>
        /// <param name="payload">The payload.</param>
        /// <returns>Task&lt;System.Nullable&lt;R&gt;&gt;.</returns>
        /// <exception cref="System.ApplicationException">Payload is missing, nothing to POST.</exception>
        /// <exception cref="System.ApplicationException">Error POSTing to {route} {result.StatusCode}.</exception>
        /// <exception cref="System.TimeoutException">Timeout to the GAPI for this JWT.</exception>
        public async Task<R> PostToServer<T, R>(HttpClient httpClient, string route, T payload) {
            try {
                if (payload == null) {
                    throw new ApplicationException("Payload is missing, nothing to POST.");
                }

                string json = JsonSerializer.Serialize(payload);
                var result = await httpClient.PostAsync(route, new StringContent(json, Encoding.UTF8, "application/json"));
                if (result != null && result.IsSuccessStatusCode) {
                    string returnJson = await result!.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(returnJson)) {
                        return default;
                    } else {
                        return JsonSerializer.Deserialize<R>(returnJson);
                    }
                } else if ((int)result.StatusCode == StatusCodes.Status419AuthenticationTimeout) {
                    throw new TimeoutException("Timeout to the GAPI for this JWT.");
                } else {
                    throw new ApplicationException($"Error POSTing to {route} {result.StatusCode}");
                }
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error Posting to server at {route}.");
                throw;
            }
        }
        #endregion

        #region PostToServer(string)

        /// <summary>
        /// Send a POST request to the route on the designated http client with a string payload (probably MKE protected).
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="route">The route.</param>
        /// <param name="payload">The string payload.</param>
        /// <returns>Task&lt;string&gt;&gt;.</returns>
        /// <exception cref="System.ApplicationException">Payload is missing, nothing to POST.</exception>
        /// <exception cref="System.ApplicationException">Error POSTing to {route} {result.StatusCode}.</exception>
        /// <exception cref="System.TimeoutException">Timeout to the GAPI for this JWT.</exception>
        public async Task<string> PostToServer(HttpClient httpClient, string route, string payload) {
            try {
                if (string.IsNullOrWhiteSpace(payload)) {
                    throw new ApplicationException("Payload is missing, nothing to POST.");
                }

                var result = await httpClient.PostAsync(route, new StringContent(payload, Encoding.UTF8, "application/json"));
                if (result != null && result.IsSuccessStatusCode) {
                    string returnPayload = await result!.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(returnPayload)) {
                        return string.Empty;
                    } else {
                        return returnPayload;
                    }
                } else if ((int)result.StatusCode == StatusCodes.Status419AuthenticationTimeout) {
                    throw new TimeoutException("Timeout to the GAPI for this JWT.");
                } else {
                    throw new ApplicationException($"Error POSTing to {route} {result.StatusCode}");
                }
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error Posting to server at {route}.");
                throw;
            }
        }
        #endregion

        #region PostToServer - no payload

        /// <summary>
        /// Send a POST request to the route on the designated http client with no payload
        /// and expect back a string of an encoded payload.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="route">The route.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        /// <exception cref="System.TimeoutException">Timeout to the GAPI for this JWT.</exception>
        /// <exception cref="System.ApplicationException">Error POSTing to {route} {result.StatusCode}.</exception>
        public async Task<string> PostToServer(HttpClient httpClient, string route) {
            try {
                var result = await httpClient.PostAsync(route, null);
                if (result != null && result.IsSuccessStatusCode) {
                    string resultPayload = await result!.Content.ReadAsStringAsync();
                    return resultPayload;
                } else if ((int)result.StatusCode == StatusCodes.Status419AuthenticationTimeout) {
                    throw new TimeoutException("Timeout to the GAPI for this JWT.");
                } else {
                    throw new ApplicationException($"Error POSTing to {route} {result.StatusCode}");
                }
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error Posting to server at {route}.");
                throw;
            }
        }
        #endregion
    }
}
