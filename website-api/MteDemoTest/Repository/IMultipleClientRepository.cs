using MteDemoTest.Models;

namespace MteDemoTest.Repository
{
    public interface IMultipleClientRepository
    {
        ResponseModel<string> MultiClientResponse(string incomingMessage, string clientId);
    }
}
