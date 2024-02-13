using Eclypses.MTE;
using Microsoft.Extensions.Logging;
using MteDemoTest.Helpers;
using MteDemoTest.Models;
using PackageCSharpECDH;
using System;

namespace MteDemoTest.Repository
{
    public class MultipleClientRepository : IMultipleClientRepository
    {
        private ILogger<MultipleClientRepository> _logger;
        private readonly IMteStateHelper _stateHelper;

        public MultipleClientRepository(ILogger<MultipleClientRepository> logger, IMteStateHelper stateHelper)
        {
            _logger = logger;
            _stateHelper = stateHelper;
        }

        #region MultiClientResponse
        /// <summary>
        /// Multi client response.
        /// Decodes incoming message based on client id
        /// Encodes outgoing message based on client id
        /// </summary>
        /// <param name="incomingMessage">The incoming message.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <returns>ResponseModel&lt;System.String&gt;.</returns>
        public ResponseModel<string> MultiClientResponse(string incomingMessage, string clientId)
        {
            ResponseModel<string> response = new ResponseModel<string>();
            try
            {
                //------------------------------
                // decode the incoming message
                //------------------------------
                ResponseModel<string> decodeResponse = _stateHelper.DecodeMessage(incomingMessage, clientId);
                if (!decodeResponse.Success)
                {
                    response.Message = decodeResponse.Message;
                    response.ResultCode = decodeResponse.ResultCode;
                    response.Success = decodeResponse.Success;
                    return response;
                }
                //------------------
                // Create response
                //------------------
                string outgoingMessage = $"Received: {decodeResponse.Data}";

                //------------------------------
                // encode the outgoing response
                //------------------------------
                ResponseModel<string> encodeResponse = _stateHelper.EncodeMessage(outgoingMessage, clientId);
                if (!encodeResponse.Success)
                {
                    response.Message = encodeResponse.Message;
                    response.ResultCode = encodeResponse.ResultCode;
                    response.Success = encodeResponse.Success;
                    return response;
                }
                //-------------------------
                // return encoded response
                //-------------------------
                response.Data = encodeResponse.Data;
            }
            catch (Exception ex)
            {
                response.Message = $"Exception responding to client message. Ex: {ex.Message}";
                response.ResultCode = Constants.RC_REPOSITORY_EXCEPTION;
                response.Success = false;
            }

            return response;
        }
        #endregion
    }
}
