// ***********************************************************************
// Assembly         : MteSDRTest.Client
// Author           : Eclypses Developer
// Created          : 07-20-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-20-2022
// ***********************************************************************
// <copyright file="AlertService.cs" company="MteSDRTest.Client">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;

namespace MteSDRTest.Client.Services {
    /// <summary>
    /// Class AlertService.
    /// Implements the <see cref="MteSDRTest.Client.Services.IAlertService" />.
    /// </summary>
    /// <seealso cref="MteSDRTest.Client.Services.IAlertService" />
    public class AlertService : IAlertService {
        /// <summary>
        /// The default identifier.
        /// </summary>
        private const string _defaultId = "default-alert";

        /// <summary>
        /// Occurs when [on alert].
        /// </summary>
        public event Action<Models.Alert> OnAlert;

        /// <summary>
        /// Invokes the Alert event on the hosted form.
        /// </summary>
        /// <param name="alert">The alert.</param>
        public void Alert(Models.Alert alert) {
            alert.Id = alert.Id ?? _defaultId;
            this.OnAlert?.Invoke(alert);
        }

        /// <summary>
        /// Clears the specified alert based on its identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void Clear(string id = _defaultId) {
            this.OnAlert?.Invoke(new Models.Alert { Id = id });
        }

        /// <summary>
        /// Builds a Success alert and invokes the "Alert" method.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="keepAfterRouteChange">if set to <c>true</c> [keep after route change].</param>
        /// <param name="autoClose">if set to <c>true</c> [automatic close].</param>
        public void Success(string message, bool keepAfterRouteChange = false, bool autoClose = true) {
            this.Alert(new Models.Alert {
                Message = message,
                Type = Models.AlertType.Success,
                KeepAfterRouteChange = keepAfterRouteChange,
                AutoClose = autoClose,
            });
        }

        /// <summary>
        /// Builds an Error alert and invokes the "Alert" method.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="keepAfterRouteChange">if set to <c>true</c> [keep after route change].</param>
        /// <param name="autoClose">if set to <c>true</c> [automatic close].</param>
        public void Error(string message, bool keepAfterRouteChange = false, bool autoClose = true) {
            this.Alert(new Models.Alert {
                Message = message,
                Type = Models.AlertType.Error,
                KeepAfterRouteChange = keepAfterRouteChange,
                AutoClose = autoClose,
            });
        }

        /// <summary>
        /// Builds an Information alert and invokes the "Alert" method.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="keepAfterRouteChange">if set to <c>true</c> [keep after route change].</param>
        /// <param name="autoClose">if set to <c>true</c> [automatic close].</param>
        public void Info(string message, bool keepAfterRouteChange = false, bool autoClose = true) {
            this.Alert(new Models.Alert {
                Message = message,
                Type = Models.AlertType.Info,
                KeepAfterRouteChange = keepAfterRouteChange,
                AutoClose = autoClose,
            });
        }

        /// <summary>
        /// Builds a Warning alert and invokes the "Alert" method.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="keepAfterRouteChange">if set to <c>true</c> [keep after route change].</param>
        /// <param name="autoClose">if set to <c>true</c> [automatic close].</param>
        public void Warning(string message, bool keepAfterRouteChange = false, bool autoClose = true) {
            this.Alert(new Models.Alert {
                Message = message,
                Type = Models.AlertType.Warning,
                KeepAfterRouteChange = keepAfterRouteChange,
                AutoClose = autoClose,
            });
        }

        /// <summary>
        /// Modals the warning.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="keepAfterRouteChange">if set to <c>true</c> [keep after route change].</param>
        /// <param name="autoClose">if set to <c>true</c> [automatic close].</param>
        /// <font color="red">Badly formed XML comment.</font>
        public void ModalWarning(string id, string message, bool keepAfterRouteChange = false, bool autoClose = false) {
            this.Alert(new Models.Alert {
                Message = message,
                Id = id,
                Type = Models.AlertType.Warning,
                KeepAfterRouteChange = keepAfterRouteChange,
                AutoClose = autoClose,
            });
        }

        /// <summary>
        /// Modals the error.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="keepAfterRouteChange">if set to <c>true</c> [keep after route change].</param>
        /// <param name="autoClose">if set to <c>true</c> [automatic close].</param>
        /// <font color="red">Badly formed XML comment.</font>
        public void ModalError(string id, string message, bool keepAfterRouteChange = false, bool autoClose = false) {
            this.Alert(new Models.Alert {
                Message = message,
                Id = id,
                Type = Models.AlertType.Error,
                KeepAfterRouteChange = keepAfterRouteChange,
                AutoClose = autoClose,
            });
        }
    }
}
