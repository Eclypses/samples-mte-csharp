using MteDemoTest.Models;

namespace MteDemoTest.Repository
{
    public interface IHandshakeRepository
    {
        /// <summary>
        /// Creates and Stores the Initial MTE from the DH Handshake
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        ResponseModel<HandshakeModel> StoreInitialClientHandshake(HandshakeModel model);
    }
}
