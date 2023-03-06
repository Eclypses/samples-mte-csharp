// ***********************************************************************
// Assembly         : MteSDRTest.Client
// Author           : Eclypses Developer
// Created          : 07-28-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 08-01-2022
// ***********************************************************************
// <copyright file="DataDisplayModel.cs" company="MteSDRTest.Client">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;

namespace MteSDRTest.Client.Models {
    /// <summary>
    /// Class DataDisplayModel.
    /// The properties contain the clear and raw (protected) data
    /// for both Local and Session storage.
    /// </summary>
    public class DataDisplayModel {
        /// <summary>
        /// Gets or sets the session clear data.
        /// </summary>
        /// <value>The session clear data.</value>
        public string SessionClearData { get; set; }

        /// <summary>
        /// Gets or sets the session protected data.
        /// </summary>
        /// <value>The session protected data.</value>
        public string SessionProtectedData { get; set; }

        /// <summary>
        /// Gets or sets the local clear data.
        /// </summary>
        /// <value>The local clear data.</value>
        public string LocalClearData { get; set; }

        /// <summary>
        /// Gets or sets the local protected data.
        /// </summary>
        /// <value>The local protected data.</value>
        public string LocalProtectedData { get; set; }
    }
}
