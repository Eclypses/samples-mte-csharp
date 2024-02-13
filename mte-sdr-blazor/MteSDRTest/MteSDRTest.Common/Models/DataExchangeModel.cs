// ***********************************************************************
// Assembly         : MteSDRTest.Common
// Author           : Eclypses Developer
// Created          : 07-20-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-20-2022
// ***********************************************************************
// <copyright file="DataExchangeModel.cs" company="MteSDRTest.Common">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.ComponentModel.DataAnnotations;

namespace MteSDRTest.Common.Models {
    /// <summary>
    /// Class DataExchangeModel.
    /// Contains arbitrary data for the client to retrieve from
    /// the server. This is protected by the MTE while in transit.
    /// </summary>
    public class DataExchangeModel {
        /// <summary>
        /// Gets or sets the owner id (workstation unique)
        /// so that the server can retrieve the proper item.
        /// </summary>
        /// <value>The owner.</value>
        [Required]
        public string? ItemOwner { get; set; }

        /// <summary>
        /// Gets or sets the item key
        /// to provide better identification of
        /// which piece of arbitrary data
        /// (associated with the owner) to retrieve.
        /// </summary>
        /// <value>The item key.</value>
        [Required]
        public string? ItemKey { get; set; }

        /// <summary>
        /// Gets or sets the value to store or retrieve.
        /// </summary>
        /// <value>The value.</value>
        public string? Value { get; set; }
    }
}
