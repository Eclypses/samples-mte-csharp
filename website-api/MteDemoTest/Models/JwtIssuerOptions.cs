namespace MteDemoTest.Models
{
    /// <summary>
    /// JWT issueer options for user jwt
    /// </summary>
    public class JwtIssuerOptions
    {
        /// <summary>
        /// The JWT secret that is created
        /// </summary>
        public string JwtSecret { get; set; }
        /// <summary>
        /// The audience
        /// </summary>
        public string Audience { get; set; }
        /// <summary>
        /// The JWT issuer
        /// </summary>
        public string Issuer { get; set; }
        /// <summary>
        /// Time till expiration
        /// </summary>
        public double TimeoutMinutes { get; set; }
    }
}
