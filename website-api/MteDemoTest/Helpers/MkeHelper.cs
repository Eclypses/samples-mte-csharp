using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Eclypses.MTE;
using MteDemoTest.Models;

namespace MteDemoTest.Helpers
{
    /// <summary>
    /// Manages MKE calls
    /// </summary>
    public class MkeHelper
    {
        private readonly ILogger<MkeHelper> _logger;

        // MTE Settings
        private bool _encoderCreated = false;
        private bool _decoderCreated = false;

        public MkeHelper(ILogger<MkeHelper> logger)
        {
            _logger = logger;
        }

        #region EncodeMessage
        /// <summary>
        /// Encodes the message.
        /// </summary>
        /// <param name="encoder">The encoder.</param>
        /// <param name="clearBytes">The clear bytes.</param>
        /// <returns>MteEncoderResponse.</returns>
        public MteEncoderResponse EncodeMessage(MteMkeEnc encoder, byte[]? clearBytes)
        {
            MteEncoderResponse response = new MteEncoderResponse { encoder = encoder };
            try
            {
                //-----------------------------------------------
                // If encoder not created, create it
                //-----------------------------------------------
                if (!_encoderCreated)
                {
                    _logger.LogDebug("Create encoder.");
                    response.Status = response.encoder.StartEncrypt();
                    if (response.Status != MteStatus.mte_status_success)
                    {
                        _logger.LogError($"Error starting encoder: Status: {response.encoder.GetStatusName(response.Status)} / {response.encoder.GetStatusDescription(response.Status)}");
                        return response;
                    }
                    _encoderCreated = true;
                }
                //-----------------------------------------------
                // encode bytes or finish up the encoder
                //-----------------------------------------------
                if (clearBytes == null)
                {
                    //-----------------------------------------------
                    // If body is null then finish encoder and clear encoder
                    //-----------------------------------------------
                    var encodedBytes = response.encoder.FinishEncrypt(out MteStatus status);
                    if (status != MteStatus.mte_status_success)
                    {
                        response.Status = status;
                        _logger.LogError($"Error finishing encoder: Status: {response.encoder.GetStatusName(response.Status)} / {response.encoder.GetStatusDescription(response.Status)}");
                        return response;
                    }

                    response.Message = encodedBytes;
                    _encoderCreated = false;
                }
                else
                {
                    //-----------------------------------------------
                    // Encode the body that is coming in
                    //-----------------------------------------------
                    response.Status = response.encoder.EncryptChunk(clearBytes);
                    if (response.Status != MteStatus.mte_status_success)
                    {
                        _logger.LogError($"Error encoder chunking: Status: {response.encoder.GetStatusName(response.Status)} / {response.encoder.GetStatusDescription(response.Status)}");
                        return response;
                    }
                    response.Message = clearBytes;
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception encoding: {ex.Message}");
                Console.WriteLine(ex);
                throw;
            }
        } 
        #endregion

        #region DecodeMessage
        /// <summary>
        /// Decodes the message.
        /// </summary>
        /// <param name="decoder">The decoder.</param>
        /// <param name="encodedBytes">The encoded bytes.</param>
        /// <param name="bytesRead">The size of the bytes read.</param>
        /// <returns>MteDecoderResponse.</returns>
        public MteDecoderResponse DecodeMessage(MteMkeDec decoder, byte[]? encodedBytes, int bytesRead)
        {
            MteDecoderResponse response = new MteDecoderResponse { decoder = decoder };
            try
            {
                //-----------------------------------------------
                // If decoder not created, create it
                //-----------------------------------------------
                if (!_decoderCreated)
                {
                    _logger.LogDebug("Create decoder.");

                    response.Status = response.decoder.StartDecrypt();
                    if (response.Status != MteStatus.mte_status_success)
                    {
                        _logger.LogError($"Error starting decoder: Status: {response.decoder.GetStatusName(response.Status)} / {response.decoder.GetStatusDescription(response.Status)}");
                        return response;
                    }
                    _decoderCreated = true;
                }
                //-----------------------------------------------
                // encode bytes or finish up the encoder
                //-----------------------------------------------
                if (encodedBytes == null)
                {
                    //-----------------------------------------------
                    // If encodedBytes is null then finish decoder and clear decoder
                    //-----------------------------------------------

                    var clearBytes = response.decoder.FinishDecrypt(out MteStatus status);
                    if (status != MteStatus.mte_status_success)
                    {
                        response.Status = status;
                        _logger.LogError($"Error finishing decoder: Status: {response.decoder.GetStatusName(response.Status)} / {response.decoder.GetStatusDescription(response.Status)}");
                        return response;
                    }
                    response.Message = clearBytes;
                    _decoderCreated = false;
                }
                else
                {
                    //-----------------------------------------------
                    // Decode the body 
                    //-----------------------------------------------
                    if (bytesRead == encodedBytes.Length)
                    {
                        response.Message = response.decoder.DecryptChunk(encodedBytes);
                        response.Status = (response.Message != null)
                            ? MteStatus.mte_status_success
                            : MteStatus.mte_status_unsupported;
                    }
                    else
                    {
                        byte[] smBuffer = new byte[bytesRead];
                        Array.Copy(encodedBytes, 0, smBuffer, 0, bytesRead);
                        response.Message = response.decoder.DecryptChunk(smBuffer);
                        response.Status = (response.Message != null)
                            ? MteStatus.mte_status_success
                            : MteStatus.mte_status_unsupported;
                    }

                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception decoding: {ex.Message}");
                Console.WriteLine(ex);
                throw;
            }
        } 
        #endregion
    }
}
