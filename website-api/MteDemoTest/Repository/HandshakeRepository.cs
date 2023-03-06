using Microsoft.Extensions.Logging;
using MteDemoTest.Helpers;
using MteDemoTest.Models;
using PackageCSharpECDH;
using System;

namespace MteDemoTest.Repository
{
    public class HandshakeRepository : IHandshakeRepository
    {
        ILogger<IHandshakeRepository> _logger;
        private readonly IMteStateHelper _stateHelper;

        public HandshakeRepository(ILogger<IHandshakeRepository> logger, IMteStateHelper stateHelper)
        {
            _logger = logger;
            _stateHelper = stateHelper;
        }

        #region StoreInitialClientHandshake
        /// <summary>
        /// Stores the initial client handshake.
        /// Creates the MTE encode and decode states
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>ResponseModel&lt;HandshakeModel&gt;.</returns>
        public ResponseModel<HandshakeModel> StoreInitialClientHandshake(HandshakeModel model)
        {
            ResponseModel<HandshakeModel> response = new ResponseModel<HandshakeModel>
            {
                Data = new HandshakeModel { ConversationIdentifier = model.ConversationIdentifier }
            };

            try
            {
                //----------------------
                // Create DH containers
                //----------------------
                EclypsesECDH encoderEcdh = new EclypsesECDH();
                EclypsesECDH decoderEcdh = new EclypsesECDH();
                //-----------------------------------------------------
                // create encoder shared secret from decoder public key
                //-----------------------------------------------------
                var encoderSharedSecret = decoderEcdh.ProcessPartnerPublicKey(model.ClientDecoderPublicKey);
                //-----------------------------------------------------
                // create decoder shared secret from encoder public key
                //-----------------------------------------------------
                var decoderSharedSecret = encoderEcdh.ProcessPartnerPublicKey(model.ClientEncoderPublicKey);

                //---------------------------------------------------------
                // Create a timestamp that both ends can use for the Nonce
                //---------------------------------------------------------
                response.Data.Timestamp = DateTime.Now.ToString("yyMMddHHmmssffff");
                // Make sure the decoder == encoder on client side 
                // Make sure the encoder == decoder on client side
                response.Data.ClientEncoderPublicKey = decoderSharedSecret.PublicKey;
                response.Data.ClientDecoderPublicKey = encoderSharedSecret.PublicKey;

                //---------------------------------------------------------
                // Create and store MTE Encoder and Decoder for this Client
                //---------------------------------------------------------
                ResponseModel mteResponse = _stateHelper.CreateMteStates(model.ConversationIdentifier, encoderSharedSecret.SharedSecret, decoderSharedSecret.SharedSecret, Convert.ToUInt64(response.Data.Timestamp));
                response.Message = mteResponse.Message;
                response.ResultCode = mteResponse.ResultCode;
                response.Success = mteResponse.Success;

                //-------------------------
                // Clear current ecdh
                //-------------------------
                encoderEcdh.ClearContainer();
                decoderEcdh.ClearContainer();

            }
            catch (Exception ex)
            {
                response.Message = $"Exception initial client handshake. Ex: {ex.Message}";
                response.ResultCode = Constants.RC_REPOSITORY_EXCEPTION;
                response.Success = false;
            }
            return response;
        }
        #endregion
    }
}
