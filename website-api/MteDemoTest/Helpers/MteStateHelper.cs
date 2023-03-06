using Eclypses.MTE;
using Microsoft.Extensions.Logging;
using MteDemoTest.Models;
using System;

namespace MteDemoTest.Helpers
{
    /// <summary>
    /// Creates and manages the MTE states and saves them to cache
    /// </summary>
    public class MteStateHelper : IMteStateHelper
    {
        private readonly ILogger<MteStateHelper> _logger;

        public MteStateHelper(ILogger<MteStateHelper> logger)
        {
            _logger = logger;
        }

        #region CreateMteStates
        /// <summary>
        /// Creates the MTE states
        /// </summary>
        /// <param name="personal">Personalization string</param>
        /// <param name="encoderEntropy">Encoder entropy bytes</param>
        /// <param name="decoderEntropy">Decoder entropy bytes</param>
        /// <param name="nonce">nonce</param>
        /// <returns></returns>
        public ResponseModel CreateMteStates(string personal, byte[] encoderEntropy, byte[] decoderEntropy, ulong nonce)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                //------------------------------
                // Create Aes Helper and get IV
                //------------------------------
                var enc = new AesHelper();
                string myIV = Constants.MteClientState.Get(Constants.IVKey);

                //----------------
                // Create encoder
                //----------------
                MteEnc encoder = new MteEnc();
                encoder.SetEntropy(encoderEntropy);
                encoder.SetNonce(nonce);
                MteStatus status = encoder.Instantiate(personal);
                if (status != MteStatus.mte_status_success)
                {
                    _logger.LogError($"Error creating encoder: Status: {encoder.GetStatusName(status)} / {encoder.GetStatusDescription(status)}");
                    response.Message =
                        $"Error creating encoder: Status: {encoder.GetStatusName(status)} / {encoder.GetStatusDescription(status)}";
                    response.ResultCode = Constants.RC_MTE_ENCODE_EXCEPTION;
                    response.Success = false;
                    return response;
                }

                //--------------------------------------------------
                // Save and encrypt state then save to memory cache
                //--------------------------------------------------
                var encoderState = encoder.SaveStateB64();
                var encryptEncState = enc.Encrypt(encoderState, personal, myIV);
                Constants.MteClientState.Store($"{Constants.EncoderPrefix}{personal}", encryptEncState, TimeSpan.FromMinutes(Constants.ExpireMinutes));

                //-------------------
                // Create Decoder
                //-------------------
                MteDec decoder = new MteDec();
                decoder.SetEntropy(decoderEntropy);
                decoder.SetNonce(nonce);
                status = decoder.Instantiate(personal);
                if (status != MteStatus.mte_status_success)
                {
                    _logger.LogError($"Error creating decoder: Status: {decoder.GetStatusName(status)} / {decoder.GetStatusDescription(status)}");
                    response.Message =
                        $"Error creating decoder: Status: {decoder.GetStatusName(status)} / {decoder.GetStatusDescription(status)}";
                    response.ResultCode = Constants.RC_MTE_DECODE_EXCEPTION;
                    response.Success = false;
                    return response;
                }

                //--------------------------------------------------
                // Save and encrypt state then save to memory cache
                //--------------------------------------------------
                var decodeState = decoder.SaveStateB64();
                var encryptDecState = enc.Encrypt(decodeState, personal, myIV);
                Constants.MteClientState.Store($"{Constants.DecoderPrefix}{personal}", encryptDecState, TimeSpan.FromMinutes(Constants.ExpireMinutes));
                response.Success = true;
                response.ResultCode = Constants.RC_SUCCESS;
                response.Message = Constants.STR_SUCCESS;
            }
            catch (Exception ex)
            {
                response.Message = $"Exception creating MTE state. Ex: {ex.Message}";
                response.ResultCode = Constants.RC_MTE_STATE_CREATION;
                response.Success = false;
            }

            return response;
        }
        #endregion

        #region EncodeMessage
        /// <summary>
        /// Encodes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <returns>ResponseModel&lt;System.String&gt;.</returns>
        public ResponseModel<string> EncodeMessage(string message, string clientId)
        {
            ResponseModel<string> response = new ResponseModel<string>();
            try
            {
                //-----------------------------------------
                // Get encryption IV and create AES Helper
                //-----------------------------------------
                string myIV = Constants.MteClientState.Get(Constants.IVKey);
                var enc = new AesHelper();

                //-------------------
                // Get encoder state
                //-------------------
                string encoderState = Constants.MteClientState.Get($"{Constants.EncoderPrefix}{clientId}");
                if (string.IsNullOrWhiteSpace(encoderState))
                {
                    response.Message = "MTE state not found, please handshake again.";
                    response.ResultCode = Constants.RC_MTE_STATE_NOT_FOUND;
                    response.Success = false;
                }
                //----------------------
                // Decrypt encoder state
                //----------------------
                var decryptedState = enc.Decrypt(encoderState, clientId, myIV);

                //-----------------------------------------
                // Restore MTE Encoder and check for error
                //-----------------------------------------
                MteEnc encoder = new MteEnc();
                MteStatus encoderStatus = encoder.RestoreStateB64(decryptedState);
                if (encoderStatus != MteStatus.mte_status_success)
                {
                    response.ResultCode = Constants.RC_MTE_ENCODE_EXCEPTION;
                    response.Message = $"Error restoring state of encoder: Status: {encoder.GetStatusName(encoderStatus)} / {encoder.GetStatusDescription(encoderStatus)}";
                    response.Success = false;
                    return response;
                }

                //-----------------
                // Encode message
                //-----------------
                response.Data = encoder.EncodeB64(message, out encoderStatus);
                if (encoderStatus != MteStatus.mte_status_success)
                {
                    response.ResultCode = Constants.RC_MTE_ENCODE_EXCEPTION;
                    response.Message = $"Error encoding outgoing message: Status: {encoder.GetStatusName(encoderStatus)} / {encoder.GetStatusDescription(encoderStatus)}";
                    response.Success = false;
                    return response;
                }

                //--------------------------------------------------------------
                // Save encoder state, encrypt state, and store to memory cache
                //--------------------------------------------------------------
                encoderState = encoder.SaveStateB64();
                var encryptedState = enc.Encrypt(encoderState, clientId, myIV);
                Constants.MteClientState.Store($"{Constants.EncoderPrefix}{clientId}", encryptedState, TimeSpan.FromMinutes(Constants.ExpireMinutes));
            }
            catch (Exception ex)
            {
                response.Message = $"Exception encoding message. Ex: {ex.Message}";
                response.ResultCode = Constants.RC_MTE_ENCODE_EXCEPTION;
                response.Success = false;
            }

            return response;
        }
        #endregion

        #region DecodeMessage
        /// <summary>
        /// Decodes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <returns>ResponseModel&lt;System.String&gt;.</returns>
        public ResponseModel<string> DecodeMessage(string message, string clientId)
        {
            ResponseModel<string> response = new ResponseModel<string>();
            try
            {
                //-----------------------------------------
                // Get encryption IV and create AES Helper
                //-----------------------------------------
                var enc = new AesHelper();
                string myIV = Constants.MteClientState.Get(Constants.IVKey);

                //-------------------------------
                // Get decoder state and decrypt
                //-------------------------------
                string decoderState = Constants.MteClientState.Get($"{Constants.DecoderPrefix}{clientId}");
                if (string.IsNullOrWhiteSpace(decoderState))
                {
                    response.Message = "MTE state not found, please handshake again.";
                    response.ResultCode = Constants.RC_MTE_STATE_NOT_FOUND;
                    response.Success = false;
                }
                var decryptedState = enc.Decrypt(decoderState, clientId, myIV);

                //--------------------------------------
                // Restore decoder and check for errors
                //--------------------------------------
                MteDec decoder = new MteDec();
                MteStatus decoderStatus = decoder.RestoreStateB64(decryptedState);
                if (decoderStatus != MteStatus.mte_status_success)
                {
                    response.ResultCode = Constants.RC_MTE_DECODE_EXCEPTION;
                    response.Message = $"Error restoring state of decoder: Status: {decoder.GetStatusName(decoderStatus)} / {decoder.GetStatusDescription(decoderStatus)}";
                    response.Success = false;
                    return response;
                }

                //----------------
                // Decode message
                //----------------
                response.Data = decoder.DecodeStrB64(message, out decoderStatus);
                if (decoderStatus != MteStatus.mte_status_success)
                {
                    response.ResultCode = Constants.RC_MTE_DECODE_EXCEPTION;
                    response.Message = $"Error decoding incoming message: Status: {decoder.GetStatusName(decoderStatus)} / {decoder.GetStatusDescription(decoderStatus)}";
                    response.Success = false;
                    return response;
                }

                //-------------------------------
                // Encrypt and save decoder state
                //-------------------------------
                decoderState = decoder.SaveStateB64();
                var encryptedState = enc.Encrypt(decoderState, clientId, myIV);
                Constants.MteClientState.Store($"{Constants.DecoderPrefix}{clientId}", encryptedState, TimeSpan.FromMinutes(Constants.ExpireMinutes));
                _logger.LogDebug($"Decoded message: {response.Data}");

            }
            catch (Exception ex)
            {
                response.Message = $"Exception decoding client message. Ex: {ex.Message}";
                response.ResultCode = Constants.RC_REPOSITORY_EXCEPTION;
                response.Success = false;
            }
            return response;
        }
        #endregion
    }
}
