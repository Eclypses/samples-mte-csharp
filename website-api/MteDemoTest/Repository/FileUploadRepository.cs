using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Eclypses.MTE;
using MteDemoTest.Helpers;
using MteDemoTest.Models;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MteDemoTest.Repository
{
    public class FileUploadRepository : IFileUploadRepository
    {
        //--------------------------
        // File Upload Mte Settings
        //--------------------------
        private static string _uploadDirectory = "\\mteUploads";
        private static string _noMteUploadDirectory = "\\noMteUploads";
        private readonly ILogger<MkeHelper> _mteLogger;
        private static int _bufferSize = 1024;

        public FileUploadRepository(ILogger<MkeHelper> logger)
        {
            _mteLogger = logger;
        }

        #region FileUpload
        /// <summary>
        /// Files the upload.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="request">The request.</param>
        /// <param name="useMte">if set to <c>true</c> [use mte].</param>
        /// <returns>ResponseModel&lt;System.Byte[]&gt;.</returns>
        public async Task<ResponseModel<byte[]>> FileUpload(string fileName, HttpRequest request, bool useMte)
        {
            ResponseModel<byte[]> response = new ResponseModel<byte[]>();
            try
            {
                //--------------------------------------------------------
                // Get the path to where we want to save the uploaded file
                //--------------------------------------------------------
                string subPath = (useMte)
                    ? Directory.GetCurrentDirectory() + _uploadDirectory
                    : Directory.GetCurrentDirectory() + _noMteUploadDirectory;

                //------------------------------------------
                // Check if directory exists, if not create
                //------------------------------------------
                bool exists = Directory.Exists(subPath);
                if (!exists)
                    Directory.CreateDirectory(subPath);

                //------------------
                // Create file name
                //------------------
                string file = Path.Combine(subPath, fileName);

                //------------------------------------------------
                // If file exists already use different file name
                //------------------------------------------------
                if (File.Exists(file))
                {
                    Random rnd = new Random();
                    while (File.Exists(file))
                    {
                        file = Path.Combine(subPath, $"N{rnd.Next(999)}{fileName}");
                    }
                }

                //--------------------------------------------------------
                // Run different methods depending on if using MTE or not
                //--------------------------------------------------------
                if (useMte)
                {
                    response = await UploadFileMte(file, request);
                }
                else
                {
                    response = await UploadFileNoMte(file, request);
                }
            }
            catch (Exception ex)
            {
                response.Message = $"Exception uploading file with MTE: {ex.Message}";
                response.Success = false;
                response.ResultCode = Constants.RC_REPOSITORY_EXCEPTION;
                _mteLogger.LogError(ex.Message);
            }

            return response;
        }
        #endregion

        #region EncodeResponse
        /// <summary>
        /// Encodes the response.
        /// </summary>
        /// <param name="outgoingResponse">The outgoing response.</param>
        /// <param name="clientId">The Id of the client.</param>
        /// <returns>ResponseModel&lt;System.Byte[]&gt;.</returns>
        private ResponseModel<byte[]> EncodeResponse(string outgoingResponse, string clientId)
        {
            ResponseModel<byte[]> response = new ResponseModel<byte[]>();
            try
            {
                //-----------------------------------------
                // Get encryption IV and create AES Helper
                //-----------------------------------------
                var enc = new AesHelper();
                string myIV = Constants.MteClientState.Get(Constants.IVKey);

                MteMkeEnc encoder = new MteMkeEnc();
                MkeHelper mteHelper = new MkeHelper(_mteLogger);
                
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
                MteStatus encoderStatus = encoder.RestoreStateB64(decryptedState);
                if (encoderStatus != MteStatus.mte_status_success)
                {
                    response.ResultCode = Constants.RC_MTE_ENCODE_EXCEPTION;
                    response.Message = $"Error restoring state of encoder: Status: {encoder.GetStatusName(encoderStatus)} / {encoder.GetStatusDescription(encoderStatus)}";
                    response.Success = false;
                    return response;
                }

                //--------------------------
                // Encode chunk the message
                //--------------------------
                MteEncoderResponse result = mteHelper.EncodeMessage(encoder, Encoding.UTF8.GetBytes(outgoingResponse));
                if (result.Status != MteStatus.mte_status_success)
                {
                    response.Success = false;
                    response.ResultCode = Constants.RC_MTE_ENCODE_CHUNK_ERROR;
                    response.Message = "Failed to encode. Status: "
                                       + encoder.GetStatusName(result.Status) + " / "
                                       + encoder.GetStatusDescription(result.Status);
                    return response;
                }

                //--------------------
                // Finish the encoder
                //--------------------
                MteEncoderResponse finalResult = mteHelper.EncodeMessage(result.encoder, null);
                if (finalResult.Status != MteStatus.mte_status_success)
                {
                    response.Success = false;
                    response.ResultCode = Constants.RC_MTE_ENCODE_FINISH_ERROR;
                    response.Message = "Failed to finish encode. Status: "
                                       + encoder.GetStatusName(finalResult.Status) + " / "
                                       + encoder.GetStatusDescription(finalResult.Status);
                    return response;
                }
                //--------------------------------------------------------------
                // Save encoder state, encrypt state, and store to memory cache
                //--------------------------------------------------------------
                encoderState = finalResult.encoder.SaveStateB64();
                var encryptedState = enc.Encrypt(encoderState, clientId, myIV);
                Constants.MteClientState.Store($"{Constants.EncoderPrefix}{clientId}", encryptedState, TimeSpan.FromMinutes(Constants.ExpireMinutes));

                //------------------------------------------------
                // Check to see if the final result had more text
                // If so append it to the result
                //------------------------------------------------
                finalResult.Message ??= new byte[0];
                response.Data = new byte[result.Message.Length + finalResult.Message.Length];
                Buffer.BlockCopy(result.Message, 0, response.Data, 0, result.Message.Length);
                Buffer.BlockCopy(finalResult.Message, 0, response.Data, result.Message.Length, finalResult.Message.Length);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ResultCode = Constants.RC_MTE_ENCODE_EXCEPTION;
                response.Message = $"Exception Encoding response: {ex.Message}";
            }

            return response;
        }
        #endregion

        #region UploadFileMte
        /// <summary>
        /// Uploads the file mte.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="request">The request.</param>
        /// <returns>ResponseModel&lt;System.Byte[]&gt;.</returns>
        private async Task<ResponseModel<byte[]>> UploadFileMte(string file, HttpRequest request)
        {
            ResponseModel<byte[]> response = new ResponseModel<byte[]>();
            try
            {
                //-----------------------------------------
                // Get encryption IV and create AES Helper
                //-----------------------------------------
                var enc = new AesHelper();
                string myIV = Constants.MteClientState.Get(Constants.IVKey);
                //---------------------------------
                // Get clientId from request header
                //---------------------------------
                string clientId = request.Headers[Constants.ClientIdHeader];

                // add check to make sure we get an entry
                if (string.IsNullOrWhiteSpace(clientId))
                {
                    response.Message = $"ClientId is empty or null, must have identifier in Header.";
                    response.ResultCode = Constants.RC_VALIDATION_ERROR;
                    response.Success = false;
                    return response;

                }
                //-------------------
                // Create MTE Helper
                //-------------------
                MkeHelper mteHelper = new MkeHelper(_mteLogger);

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
                MteMkeDec decoder = new MteMkeDec();
                MteStatus decoderStatus = decoder.RestoreStateB64(decryptedState);
                if (decoderStatus != MteStatus.mte_status_success)
                {
                    response.ResultCode = Constants.RC_MTE_DECODE_EXCEPTION;
                    response.Message = $"Error restoring state of decoder: Status: {decoder.GetStatusName(decoderStatus)} / {decoder.GetStatusDescription(decoderStatus)}";
                    response.Success = false;
                    return response;
                }

                //------------------------------------------------
                // iterate through request body and write to file
                //------------------------------------------------
                await using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write,
                    FileShare.None, _bufferSize, useAsync: true))
                {
                    var buffer = new byte[_bufferSize];
                    var bytesRead = default(int);
                    while ((bytesRead = await request.Body.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        //-----------------
                        // decode the data
                        //-----------------
                        MteDecoderResponse decResponse = mteHelper.DecodeMessage(decoder, buffer, bytesRead);
                        if (decResponse.Status != MteStatus.mte_status_success)
                        {
                            response.Data = null;
                            response.Success = false;
                            response.ResultCode = Constants.RC_MTE_DECODE_CHUNK_ERROR;
                            response.Message = $"Failed to Decode chunk. Status: " +
                                            $"{decoder.GetStatusName(decResponse.Status)} / " +
                                            $"{decoder.GetStatusDescription(decResponse.Status)}";
                            return response;
                        }
                        //----------------------------------
                        // write decoded data to file
                        // debuging stuff --> look at bytes
                        //----------------------------------
                        string thesebytes = Encoding.Default.GetString(decResponse.Message);
                        await fs.WriteAsync(decResponse.Message, 0, decResponse.Message.Length);
                        //---------------------------------------------------
                        // set the decoder to the latest version of decoder
                        //---------------------------------------------------
                        decoder = decResponse.decoder;
                    }
                    //--------------------------------------
                    // Finish the decoding chunking session
                    //--------------------------------------
                    MteDecoderResponse decFinalResponse = mteHelper.DecodeMessage(decoder, null, 0);
                    if (decFinalResponse.Status != MteStatus.mte_status_success)
                    {
                        response.Data = null;
                        response.Success = false;
                        response.ResultCode = Constants.RC_MTE_DECODE_FINISH_ERROR;
                        response.Message = "Failed to finish decode chunk. Status: "
                                        + decoder.GetStatusName(decFinalResponse.Status) + " / "
                                        + decoder.GetStatusDescription(decFinalResponse.Status);
                        return response;
                    }
                    //-------------------------------
                    // Encrypt and save decoder state
                    //-------------------------------
                    decoderState = decFinalResponse.decoder.SaveStateB64();
                    var encryptedState = enc.Encrypt(decoderState, clientId, myIV);
                    Constants.MteClientState.Store($"{Constants.DecoderPrefix}{clientId}", encryptedState, TimeSpan.FromMinutes(Constants.ExpireMinutes));
                    //-----------------------------------------------------------------------
                    // Check if there is additional bytes if not initialize empty byte array
                    //-----------------------------------------------------------------------
                    if (decFinalResponse.Message.Length <= 0) { decFinalResponse.Message = new byte[0]; }
                    //------------------------------------
                    // Append the final data to the file
                    //------------------------------------
                    string finishbytes = Encoding.Default.GetString(decFinalResponse.Message);
                    await fs.WriteAsync(decFinalResponse.Message, 0, decFinalResponse.Message.Length);
                }
                
                response.Message = null;
                return EncodeResponse("Successfully uploaded file.", clientId);
               
            }
            catch (Exception ex)
            {
                response.Message = $"Exception uploading file with MTE: {ex.Message}";
                response.Success = false;
                response.ResultCode = Constants.RC_MTE_DECODE_EXCEPTION;
                _mteLogger.LogError(ex.Message);
                return response;
            }
        }
        #endregion

        #region UploadFileNoMte
        /// <summary>
        /// Uploads the file no mte.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="request">The request.</param>
        /// <returns>ResponseModel&lt;System.Byte[]&gt;.</returns>
        private async Task<ResponseModel<byte[]>> UploadFileNoMte(string file, HttpRequest request)
        {
            ResponseModel<byte[]> response = new ResponseModel<byte[]>();
            try
            {
                //------------------------------------------------
                // iterate through request body and write to file
                //------------------------------------------------
                await using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write,
                    FileShare.None, _bufferSize, useAsync: true))
                {
                    var buffer = new byte[_bufferSize];
                    var bytesRead = default(int);
                    while ((bytesRead = await request.Body.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        //--------------------
                        // write data to file
                        //--------------------
                        await fs.WriteAsync(buffer, 0, bytesRead);
                    }
                }

                response.Data = Encoding.UTF8.GetBytes("Successfully uploaded file.");
            }
            catch (Exception ex)
            {
                response.Message = $"Exception uploading file with MTE: {ex.Message}";
                response.Success = false;
                response.ResultCode = Constants.RC_REPOSITORY_EXCEPTION;
                _mteLogger.LogError(ex.Message);
            }
            return response;
        } 
        #endregion
    }
}
