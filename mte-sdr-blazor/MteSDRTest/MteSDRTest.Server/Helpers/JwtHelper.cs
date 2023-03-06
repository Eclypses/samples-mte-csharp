// ***********************************************************************
// Assembly         : MteSDRTest.Server
// Author           : Eclypses Developer
// Created          : 07-21-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-21-2022
// ***********************************************************************
// <copyright file="JwtHelper.cs" company="MteSDRTest.Server">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using MteSDRTest.Common.Models;
using MteSDRTest.Server.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MteSDRTest.Server.Helpers {
    /// <summary>
    /// Methods for managing the Jwt for this user and session.
    /// </summary>
    public class JwtHelper : IJwtHelper {
        /// <summary>
        /// The end user claim type.
        /// </summary>
        private static readonly string END_USER_CLAIM_TYPE = "EndUser";

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<JwtHelper> _logger;

        /// <summary>
        /// The JWT issuer options.
        /// </summary>
        private readonly JwtIssuerOptions _jwtIssuerOptions;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtHelper" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="jwtOptions">The JWT options.</param>
        public JwtHelper(ILogger<JwtHelper> logger, IOptions<JwtIssuerOptions> jwtOptions) {
            _jwtIssuerOptions = jwtOptions.Value;
            _logger = logger;
        }
        #endregion

        #region CreateTheJWT

        /// <summary>
        /// Creates the JWT.
        /// </summary>
        /// <param name="user">The user model to serialize into the Jwt.</param>
        /// <returns>System.String of the Jwt.</returns>
        public string CreateTheJWT(ClientUserModel user) {
            var keyBytes = Encoding.UTF8.GetBytes(_jwtIssuerOptions.JwtSecret);
            var symmetricSecurityKey = new SymmetricSecurityKey(keyBytes);
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);
            var cryptoKey = new EncryptingCredentials(symmetricSecurityKey, JwtConstants.DirectKeyUseAlg, SecurityAlgorithms.Aes256CbcHmacSha512);

            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(END_USER_CLAIM_TYPE, System.Text.Json.JsonSerializer.Serialize(user)),
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtIssuerOptions.TimeoutMinutes),
                Audience = _jwtIssuerOptions.Audience,
                Issuer = _jwtIssuerOptions.Issuer,
                NotBefore = DateTime.UtcNow.AddMinutes(-2),
                IssuedAt = DateTime.UtcNow.AddMinutes(-1),
                SigningCredentials = signingCredentials,
                EncryptingCredentials = cryptoKey,
            };

            //
            // Add the roles for this user
            //
            user.Roles.ForEach(r => {
                tokenDescriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, r));
            });

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        #endregion

        #region ExtractCurrentUser

        /// <summary>
        /// Extracts the serialized current user from a claims principal.
        /// </summary>
        /// <param name="principal">The claims principal of the logged in user.</param>
        /// <returns>CurrentUserModel re-hydrated from the specific claim in the principal.</returns>
        /// <exception cref="System.ApplicationException">No claimtype of {END_USER_CLAIM_TYPE} found in this Jwt - cannot continue.</exception>
        public ClientUserModel ExtractCurrentUser(ClaimsPrincipal principal) {
            try {
                var endUserClaim = principal.Claims.Where(c => c.Type == END_USER_CLAIM_TYPE).FirstOrDefault();
                if (endUserClaim == null) {
                    throw new ApplicationException($"No claimtype of {END_USER_CLAIM_TYPE} found in this Jwt - cannot continue.");
                }

                ClientUserModel model = System.Text.Json.JsonSerializer.Deserialize<ClientUserModel>(endUserClaim.Value);
                return model;
            } catch (Exception ex) {
                _logger.LogError(ex, $"Exception extracting current user: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region ExtractClaimsPrincipal

        /// <summary>
        /// Extracts the claims principal.
        /// </summary>
        /// <param name="jwt">The JWT.</param>
        /// <returns>ClaimsPrincipal extracted from the Jwt.</returns>
        public ClaimsPrincipal ExtractClaimsPrincipal(string jwt) {
            try {
                var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtIssuerOptions.JwtSecret));
                var handler = new JwtSecurityTokenHandler();
                SecurityToken validatedToken;
                var claimsPrincipal = handler.ValidateToken(
                    jwt,
                    new TokenValidationParameters {
                        ValidAudience = _jwtIssuerOptions.Audience,
                        ValidIssuer = _jwtIssuerOptions.Issuer,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = symmetricSecurityKey,
                        TokenDecryptionKey = symmetricSecurityKey,
                    },
                    out validatedToken);
                return claimsPrincipal;
            } catch (Exception ex) {
                _logger.LogError(ex, $"Could not extract user principal from jwt: {ex.Message}.");
                return null;
            }
        }
        #endregion

        #region PullJwtFromQuery

        /// <summary>
        /// Pulls the JWT from a query string.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="System.ApplicationException">No access token key is present on request Query.</exception>
        public string PullJwtFromQuery(string query) {
            if (query.StartsWith("?")) {
                query = query.Remove(0, 1);
            }

            if (!query.Contains("access_token")) {
                throw new ApplicationException("No access token key is present on request Query.");
            }

            string[] queries = query.Split("&");
            foreach (var bbquery in queries) {
                if (bbquery.StartsWith("access_token")) {
                    return bbquery.Split("=")[1];
                }
            }

            return null;
        }
        #endregion
    }
}
