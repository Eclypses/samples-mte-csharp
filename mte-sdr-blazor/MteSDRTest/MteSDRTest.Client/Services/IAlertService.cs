// ***********************************************************************
// Assembly         : MteSDRTest.Client
// Author           : Eclypses Developer
// Created          : 07-20-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-20-2022
// ***********************************************************************
// <copyright file="IAlertService.cs" company="MteSDRTest.Client">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using MteSDRTest.Client.Models;

namespace MteSDRTest.Client.Services {
    /// <summary>
    /// Interface IAlertService.
    /// </summary>
    public interface IAlertService {
        /// <summary>
        /// Occurs when [on alert].
        /// </summary>
        event Action<Alert> OnAlert;

        /// <summary>
        /// Alerts the specified alert.
        /// </summary>
        /// <param name="alert">The alert.</param>
        void Alert(Alert alert);

        /// <summary>
        /// Clears the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        void Clear(string id = "default-alert");

        /// <summary>
        /// Errors the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="keepAfterRouteChange">if set to <c>true</c> [keep after route change].</param>
        /// <param name="autoClose">if set to <c>true</c> [automatic close].</param>
        void Error(string message, bool keepAfterRouteChange = false, bool autoClose = true);

        /// <summary>
        /// Informations the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="keepAfterRouteChange">if set to <c>true</c> [keep after route change].</param>
        /// <param name="autoClose">if set to <c>true</c> [automatic close].</param>
        void Info(string message, bool keepAfterRouteChange = false, bool autoClose = true);

        /// <summary>
        /// Modals the error.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="keepAfterRouteChange">if set to <c>true</c> [keep after route change].</param>
        /// <param name="autoClose">if set to <c>true</c> [automatic close].</param>
        void ModalError(string id, string message, bool keepAfterRouteChange = false, bool autoClose = false);

        /// <summary>
        /// Modals the warning.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="keepAfterRouteChange">if set to <c>true</c> [keep after route change].</param>
        /// <param name="autoClose">if set to <c>true</c> [automatic close].</param>
        void ModalWarning(string id, string message, bool keepAfterRouteChange = false, bool autoClose = false);

        /// <summary>
        /// Successes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="keepAfterRouteChange">if set to <c>true</c> [keep after route change].</param>
        /// <param name="autoClose">if set to <c>true</c> [automatic close].</param>
        void Success(string message, bool keepAfterRouteChange = false, bool autoClose = true);

        /// <summary>
        /// Warnings the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="keepAfterRouteChange">if set to <c>true</c> [keep after route change].</param>
        /// <param name="autoClose">if set to <c>true</c> [automatic close].</param>
        void Warning(string message, bool keepAfterRouteChange = false, bool autoClose = true);
    }
}
