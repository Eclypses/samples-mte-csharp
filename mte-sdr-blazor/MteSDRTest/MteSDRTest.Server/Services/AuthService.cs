// ***********************************************************************
// Assembly         : MteSDRTest.Server
// Author           : Eclypses Developer
// Created          : 07-19-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 08-01-2022
// ***********************************************************************
// <copyright file="AuthService.cs" company="MteSDRTest.Server">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;
using System.Threading.Tasks;
using MteSDRTest.Common.Helpers;
using MteSDRTest.Common.Models;
using MteSDRTest.Server.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MteSDRTest.Server.Services {
    /// <summary>
    /// Class AuthService.
    /// Implements the <see cref="MteSDRTest.Server.Services.IAuthService" />
    /// </summary>
    /// <seealso cref="MteSDRTest.Server.Services.IAuthService" />
    public class AuthService : IAuthService {
        /// <summary>
        /// The configuration from appsettings.
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<AuthService> _logger;

        /// <summary>
        /// The crypto helper.
        /// </summary>
        private readonly ICryptoHelper _crypto;

        /// <summary>
        /// The JWT helper.
        /// </summary>
        private readonly IJwtHelper _jwtHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="crypto">The crypto helper.</param>
        /// <param name="jwtHelper">The JWT helper.</param>
        public AuthService(ILogger<AuthService> logger, IConfiguration config, ICryptoHelper crypto, IJwtHelper jwtHelper) {
            _config = config;
            _logger = logger;
            _crypto = crypto;
            _jwtHelper = jwtHelper;
        }

        /// <summary>
        /// Authorizes the specified request model.
        /// </summary>
        /// <param name="model">The client credentials model.</param>
        /// <returns>ClientUserModel.</returns>
        public async Task<ClientUserModel> Authorize(ClientCredentials model) {
            //
            // For this demo, just check to see if the password matches what we have
            // configured in the appsettings.json file.
            //
            if (_crypto.CreateHash(model.Password) == _crypto.CreateHash(_config.GetValue<string>("AppSettings:ValidPassword"))) {
                _logger.LogInformation($"User with email of {model.UserId} successfully logged in.");
                var client = new ClientUserModel {
                    ClientAuthToken = string.Empty,
                    Name = model.UserId,
                    Success = true,
                    Roles = new List<string> { "User" },
                };

                //
                // Create an encrypted JWT token to use for authentication.
                //
                client.ClientAuthToken = _jwtHelper.CreateTheJWT(client);
                return client;
            } else {
                _logger.LogInformation($"User with email of {model.UserId} is not authorized to log in.");
                return null;
            }
        }
    }
}
