using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MteDemoTest.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MteDemoTest.Helpers
{
    public class AuthHelper : IAuthHelper
    {
        private readonly ILogger<AuthHelper> _logger;
        private JwtIssuerOptions _jwtIssuerOptions;
        private AppSettings _appSettings;
        public AuthHelper(ILogger<AuthHelper> logger, IOptions<JwtIssuerOptions> jwtOptions, IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            _jwtIssuerOptions = jwtOptions.Value;
            _appSettings = appSettings.Value;
        }

        #region RetrieveEndUserCredentials
        /// <summary>
        /// Retrieves a EndUserCredential from the ClaimsPrincipal object
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<EndUserCredentials> RetrieveEndUserCredentials(System.Security.Claims.ClaimsPrincipal user)
        {
            if (user != null)
            {
                var endUserClaim = user.Claims.Where(c => c.Type == "EndUser").FirstOrDefault();
                if (endUserClaim != null)
                {
                    EndUserCredentials endUserCredentials = System.Text.Json.JsonSerializer.Deserialize<EndUserCredentials>(endUserClaim.Value);
                    CreateTheJWT(endUserCredentials);
                    return endUserCredentials;
                }
            }
            return null;
        }
        #endregion

        #region Authenticate
        /// <summary>
        ///  Authenticates a user by invoking the Identity Helper with the login model
        /// </summary>
        /// <param name="model">The Login model</param>
        /// <param name="clientId">the client id</param>
        /// <returns></returns>
        public async Task<EndUserCredentials> Authenticate(LoginModel model, string clientId)
        {
            //
            // Call our identity service to find the user with the requested id and secret
            //
            EndUserCredentials user = await ValidateUsage(model.UserName, model.Password, clientId);
            if (user == null)
            {
                _logger.LogWarning("User with name of {0} not found with the requested password", model.UserName);
                return null;
            }

            CreateTheJWT(user);
            _logger.LogDebug("Succesfully authenticated user with name of {0}", model.UserName);
            return user;
        }
        #endregion

        #region ValidateUsage
        /// <summary>
        /// Validates the user credentials - this is app specific, but it must return an EndUserCredentials object.
        /// </summary>
        /// <param name="email">The identifier.</param>
        /// <param name="secret">The secret.</param>
        /// <returns>Task&lt;EndUserCredentials&gt;.</returns>
        public async Task<EndUserCredentials> ValidateUsage(string email, string secret, string clientId)
        {
            EndUserCredentials credentials = null;

            //-----------------------------------------------------------
            // Hash password and compare to password hash in appSettings
            //-----------------------------------------------------------
            string hashedSecret = ComputeSha256Hash(secret);
            if (hashedSecret != _appSettings.PasswordHash)
            {
                _logger.LogDebug($"Invalid secret.");
            }
            if (!email.Equals(email, System.StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogDebug($"User not found with email of {email}");
            }
            else
            {
                //------------------------------
                // Create a credentials object
                //------------------------------
                credentials = new EndUserCredentials
                {
                    Email = email,
                    ConversationIdentifier = clientId,
                    Roles = new System.Collections.Generic.List<string> { "Customer" }
                };
            }
            return credentials;
        }
        #endregion

        /// <summary>
        /// Computes the SHA 256 hash
        /// </summary>
        /// <param name="rawData">Raw data being hashed</param>
        /// <returns></returns>
        private static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        #region CreateTheJWT
        /// <summary>
        /// Creates the JWT which is encrypted so that only the server can get to its details.
        /// </summary>
        /// <param name="user">The EncUser Credentials just created</param>
        private void CreateTheJWT(EndUserCredentials user)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_jwtIssuerOptions.JwtSecret);
            var symmetricSecurityKey = new SymmetricSecurityKey(keyBytes);
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);
            var cryptoKey = new EncryptingCredentials(symmetricSecurityKey, JwtConstants.DirectKeyUseAlg, SecurityAlgorithms.Aes256CbcHmacSha512);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("EndUser", System.Text.Json.JsonSerializer.Serialize(user)),
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtIssuerOptions.TimeoutMinutes),
                Audience = _jwtIssuerOptions.Audience,
                Issuer = _jwtIssuerOptions.Issuer,
                NotBefore = DateTime.UtcNow.AddMinutes(-2),
                IssuedAt = DateTime.UtcNow.AddMinutes(-1),
                SigningCredentials = signingCredentials,
                EncryptingCredentials = cryptoKey
            };
            //
            // Add the roles for this user
            //
            user.Roles.ForEach(r =>
            {
                tokenDescriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, r));
            });

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);
        }
    }
    #endregion
}

