using MteDemoTest.Models;
using System.Threading.Tasks;

namespace MteDemoTest.Helpers
{
    public interface IAuthHelper
    {
        /// <summary>
        /// Retrieves the End User Credentials
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<EndUserCredentials> RetrieveEndUserCredentials(System.Security.Claims.ClaimsPrincipal user);

        /// <summary>
        /// Authenticates the User
        /// </summary>
        /// <param name="model"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        Task<EndUserCredentials> Authenticate(LoginModel model, string clientId);

        /// <summary>
        /// Validates the user email and password
        /// </summary>
        /// <param name="email"></param>
        /// <param name="secret"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        Task<EndUserCredentials> ValidateUsage(string email, string secret, string clientId);
    }
}
