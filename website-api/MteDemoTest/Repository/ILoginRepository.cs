using MteDemoTest.Models;

namespace MteDemoTest.Repository
{
    public interface ILoginRepository
    {
        /// <summary>
        /// Logs in user based on incoming encoded login model
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="encodedInput"></param>
        /// <returns></returns>
        public ResponseModel<string> UserLogin(string clientId, string encodedInput);
    }
}
