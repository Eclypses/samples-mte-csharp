namespace MteDemoTest.Models
{
    /// <summary>
    /// AppSettings for Login
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Authorized Email
        /// </summary>
        public string TestEmail { get; set; }
        /// <summary>
        /// Hash of the authorized password
        /// </summary>
        public string PasswordHash { get; set; }
    }
}
