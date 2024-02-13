using MteDemoTest.Models;

namespace MteDemoTest.Helpers
{
    public interface IMteStateHelper
    {
        /// <summary>
        /// Creates the MTE states and saves them to the cache
        /// </summary>
        /// <param name="personal"></param>
        /// <param name="encoderEntropy"></param>
        /// <param name="decoderEntropy"></param>
        /// <param name="nonce"></param>
        /// <returns></returns>
        ResponseModel CreateMteStates(string personal, byte[] encoderEntropy, byte[] decoderEntropy, ulong nonce);

        /// <summary>
        /// Encodes the message using cached state of clientId
        /// </summary>
        /// <param name="message"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        ResponseModel<string> EncodeMessage(string message, string clientId);

        /// <summary>
        /// Decodes the message using cached state of clientId
        /// </summary>
        /// <param name="message"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        ResponseModel<string> DecodeMessage(string message, string clientId);

    }
}
