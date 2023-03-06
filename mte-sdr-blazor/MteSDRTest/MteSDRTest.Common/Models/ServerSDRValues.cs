// ***********************************************************************
// Assembly         : MteSDRTest.Common
// Author           : Eclypses Developer
// Created          : 08-03-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 08-03-2022
// ***********************************************************************
// <copyright file="ServerSDRValues.cs" company="MteSDRTest.Common">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace MteSDRTest.Common.Models {
    /// <summary>
    /// Class ServerSDRValues.
    /// These values are stored securely on the server for a specific
    /// workstation identifier and are used to initialize the Eclypses SDR
    /// for local storage. This is serialized into the "Value" property
    /// of the Data Exchange Model.
    /// </summary>
    public class ServerSDRValues {
        /// <summary>
        /// Gets or sets the workstation entropy.
        /// </summary>
        /// <value>The workstation entropy.</value>
        public string? WorkstationLocalStorageEntropy { get; set; }

        /// <summary>
        /// Gets or sets the workstation nonce.
        /// </summary>
        /// <value>The workstation nonce.</value>
        public string? WorkstationLocalStorageNonce { get; set; }
    }
}
