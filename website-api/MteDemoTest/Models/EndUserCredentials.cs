using System.Collections.Generic;

namespace MteDemoTest.Models
{
    /// <summary>
    /// End User Credentials
    /// </summary>
    public class EndUserCredentials
    {
        /// <summary>
        /// The email address (user name) for this user
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Contains the JWT for this user and is updated for each route
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// Primary conversation identifier for this transaction
        /// </summary>
        public string ConversationIdentifier { get; set; }
        /// <summary>
        /// List of the roles for this user
        /// </summary>
        public List<string> Roles { get; set; }
    }
}
