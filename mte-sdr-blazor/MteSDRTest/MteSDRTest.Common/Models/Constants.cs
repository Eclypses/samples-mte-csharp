// ***********************************************************************
// Assembly         : MteSDRTest.Common
// Author           : Eclypses Developer
// Created          : 07-20-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-20-2022
// ***********************************************************************
// <copyright file="Constants.cs" company="MteSDRTest.Common">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace MteSDRTest.Common.Models {
    /// <summary>
    /// Class Constants used by both the client and the server.
    /// </summary>
    public class Constants {
        /// <summary>
        /// The client header key - used to keep a unique session key
        /// for storage and retrieval of the associated MKE state.
        /// </summary>
        public const string CLIENT_HEADER_KEY = "X-ClientId";

        /// <summary>
        /// The display session item key - used to identify which
        /// piece of data is stored in session storage.
        /// </summary>
        public const string DISPLAY_SESSION_ITEM_KEY = "temporaryData";

        /// <summary>
        /// The display session item file - used by the MTE Vault
        /// to identify a category of items to store in session storage.
        /// </summary>
        public const string DISPLAY_SESSION_ITEM_FILE = "Session";

        /// <summary>
        /// The display local item key - used to identify which
        /// piece of data is stored in local storage.
        /// </summary>
        public const string DISPLAY_LOCAL_ITEM_KEY = "permanentData";

        /// <summary>
        /// The display local item file - used by the MTE Vault
        /// to identify a category of items to store in local storage.
        /// </summary>
        public const string DISPLAY_LOCAL_ITEM_FILE = "Persisted";

        /// <summary>
        /// Route to retrieve arbitrary data from the server.
        /// </summary>
        public const string ROUTE_RETRIEVE_SOME_DATA = "api/data";

        /// <summary>
        /// Unauthenticated route to echo to ensure the server is alive.
        /// </summary>
        public const string ROUTE_GET_ECHO = "api/echo/{msg}";

        /// <summary>
        /// Unauthenticated route to pair a client and server's MKEs.
        /// </summary>
        public const string ROUTE_MTEPAIR = "api/mtepair";

        /// <summary>
        /// Unauthenticated route to attempt a login - the credentials
        /// are protected by MKE.
        /// </summary>
        public const string ROUTE_LOGIN = "api/login";
    }
}
