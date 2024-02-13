// ***********************************************************************
// Assembly         : MteSDRTest.Client
// Author           : Eclypses Developer
// Created          : 07-19-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-19-2022
// ***********************************************************************
// <copyright file="Alert.cs" company="MteSDRTest.Client">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace MteSDRTest.Client.Models {
    /// <summary>
    /// Class Alert.
    /// Properties for managing alerts in the UI.
    /// </summary>
    public class Alert {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the AlertType.
        /// </summary>
        /// <value>The type.</value>
        public AlertType Type { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [automatic close].
        /// </summary>
        /// <value><c>true</c> if [automatic close]; otherwise, <c>false</c>.</value>
        public bool AutoClose { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [keep after route change].
        /// </summary>
        /// <value><c>true</c> if [keep after route change]; otherwise, <c>false</c>.</value>
        public bool KeepAfterRouteChange { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Alert" /> should fade.
        /// </summary>
        /// <value><c>true</c> if fade; otherwise, <c>false</c>.</value>
        public bool Fade { get; set; }
    }

    /// <summary>
    /// Enum AlertType.
    /// </summary>
    public enum AlertType {
        /// <summary>
        /// The success alert.
        /// </summary>
        Success,

        /// <summary>
        /// The error alert.
        /// </summary>
        Error,

        /// <summary>
        /// The information alert.
        /// </summary>
        Info,

        /// <summary>
        /// The warning alert.
        /// </summary>
        Warning,
    }
}
