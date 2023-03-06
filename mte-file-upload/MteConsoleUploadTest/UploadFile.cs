using Eclypses.MTE;
using MteConsoleUploadTest.Models;
using PackageCSharpECDH;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Eclypses.MTE.MteStatus;

namespace MteConsoleUploadTest
{
    public class UploadFile
    {
        #region UploadFile Constructor
        
        //---------------------------
        // Set upload file constants
        //---------------------------
        private const int RC_MAX_CHUNK_SIZE = 1024;
        private HttpWebRequest _webRequest = null;
        private FileStream _fileReader = null;
        private Stream _requestStream = null;

        #endregion

        #region Send
        /// <summary>
        /// Sends the file up to the server
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="useMte">Whether or not to use MTE when sending file.</param>
        /// <param name="encoderState">The current Encoder state.</param>
        /// <param name="decoderState">The current Decoder state.</param>
        /// <param name="clientId">The client id.</param>
        /// <returns>System.Int32.</returns>
        public ResponseModel<UploadResponse> Send(string path, 
                                                bool useMte, 
                                                string encoderState, 
                                                string decoderState, 
                                                string clientId)
        {
            ResponseModel<UploadResponse> uploadResponse = new()
            {
                Data = new UploadResponse
                {
                    EncoderState = encoderState,
                    DecoderState = decoderState
                }
            };
            try
            {
                //------------------------
                // Create default Encoder
                //------------------------
                var encoder = new MteMkeEnc();

                //------------------------------
                // Get file info and create URL
                //------------------------------
                var file = new FileInfo(path);
                var urlType = (useMte) ? "mte" : "nomte";
                var fileUrl = Path.Combine($"{Constants.RestAPIName}/FileUpload/", urlType + "?name=" + file.Name);

                //-----------------------------------
                // Create file stream and webRequest
                //-----------------------------------
                _fileReader = new FileStream(path, FileMode.Open, FileAccess.Read);
                _webRequest = (HttpWebRequest)WebRequest.Create(fileUrl);
                _webRequest.Method = "POST";
                if (useMte)
                {
                    //--------------------------------------------------
                    // If we are using the MTE adjust the content length
                    //--------------------------------------------------
                    var additionalBytes = encoder.EncryptFinishBytes();
                    var finalLength = (long)(_fileReader.Length + additionalBytes);
                    _webRequest.ContentLength = finalLength;
                }
                else
                {
                    //-------------------------------------------------------------
                    // Regular request will have the file length as content length
                    //-------------------------------------------------------------
                    _webRequest.ContentLength = _fileReader.Length;
                }
                _webRequest.Timeout = 600000;

                //--------------------------------------
                // Add client id to header if not null
                //--------------------------------------
                if (!string.IsNullOrWhiteSpace(clientId))
                {
                    _webRequest.Headers.Add(Constants.ClientIdHeader, clientId);
                }

                _webRequest.Credentials = CredentialCache.DefaultCredentials;
                _webRequest.AllowWriteStreamBuffering = false;
                _requestStream = _webRequest.GetRequestStream();

                var fileSize = _fileReader.Length;
                var remainingBytes = fileSize;
                var numberOfBytesRead = 0;

                if (useMte)
                {
                    //----------------------------
                    // Restore Encoder from state
                    //----------------------------
                    var status = encoder.RestoreStateB64(encoderState);
                    if (status != mte_status_success)
                    {
                        uploadResponse.Success = false;
                        uploadResponse.ResultCode = Constants.RC_MTE_STATE_RETRIEVAL;
                        uploadResponse.Message = $"Failed to restore MTE Encoder engine. Status: {encoder.GetStatusName(status)} / {encoder.GetStatusDescription(status)}";
                        return uploadResponse;
                    }

                    //----------------------------
                    // Start the chunking session
                    //----------------------------
                    status = encoder.StartEncrypt();
                    if (status != mte_status_success)
                    {
                        uploadResponse.Success = false;
                        uploadResponse.ResultCode = Constants.RC_MTE_ENCODE_EXCEPTION;
                        uploadResponse.Message = "Failed to start encode chunk. Status: "
                                            + encoder.GetStatusName(status) + " / "
                                            + encoder.GetStatusDescription(status);
                        return uploadResponse;
                    }
                }

                //------------------------
                // Break up files to send
                //------------------------
                while (numberOfBytesRead < fileSize)
                {
                    byte[] fileData;
                    SetByteArray(out fileData, remainingBytes);
                    var done = _fileReader.Read(fileData, 0, fileData.Length);
                    if (useMte)
                    {
                        //------------------------------------------------------------
                        // Encode the data in place - encoded data put back in buffer
                        //------------------------------------------------------------
                        var chunkStatus = encoder.EncryptChunk(fileData, 0, fileData.Length);
                        if (chunkStatus != mte_status_success)
                        {
                            uploadResponse.Success = false;
                            uploadResponse.ResultCode = Constants.RC_MTE_ENCODE_EXCEPTION;
                            uploadResponse.Message = "Failed to encode chunk. Status: "
                                                + encoder.GetStatusName(chunkStatus) + " / "
                                                + encoder.GetStatusDescription(chunkStatus);
                            return uploadResponse;
                        }
                    }
                    //-----------------------------
                    // Write the data to the stream
                    //-----------------------------
                    _requestStream.Write(fileData, 0, fileData.Length);
                    numberOfBytesRead += done;
                    remainingBytes -= done;
                }

                if (useMte)
                {
                    //----------------------------
                    // Finish the chunking session
                    //----------------------------
                    var finalEncodedChunk = encoder.FinishEncrypt(out var finishStatus);
                    if (finishStatus != mte_status_success)
                    {
                        uploadResponse.Success = false;
                        uploadResponse.ResultCode = Constants.RC_MTE_ENCODE_EXCEPTION;
                        uploadResponse.Message = "Failed to finish encode chunk. Status: "
                                            + encoder.GetStatusName(finishStatus) + " / "
                                            + encoder.GetStatusDescription(finishStatus);
                        return uploadResponse;
                    }

                    //------------------------------------
                    // Append the final data to the stream
                    //------------------------------------
                    _requestStream.Write(finalEncodedChunk, 0, finalEncodedChunk.Length);

                    //------------------------------
                    // Return the current seed count
                    //------------------------------
                    uploadResponse.Data.CurrentSeed = encoder.GetReseedCounter();

                    //-----------------------
                    // Save the encoderState
                    //-----------------------
                    uploadResponse.Data.EncoderState = encoder.SaveStateB64();

                }

                //------------------
                // Get the response.
                //------------------
                var response = _webRequest.GetResponse();

                //---------------------
                // get the return text
                //---------------------
                using Stream data = response.GetResponseStream();
                using var reader = new StreamReader(data);
                var text = reader.ReadToEnd();

                var textResponse =
                    JsonSerializer.Deserialize<ResponseModel<byte[]>>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!textResponse.Success)
                {
                    //-----------------------------------
                    // Check if we need to "re-handshake"
                    //-----------------------------------
                    if (textResponse.ResultCode.Equals(Constants.RC_MTE_STATE_NOT_FOUND,
                            StringComparison.InvariantCultureIgnoreCase))
                    {
                        //-------------------------------------------------------------------------
                        // The server does not have this client's state - we should "re-handshake"
                        //-------------------------------------------------------------------------
                        var handshakeResponse = HandshakeWithServer(clientId);

                        //-----------------------------------------------------------
                        // Return response, if successful give message to try again.
                        //-----------------------------------------------------------
                        uploadResponse.ReturnWithResponseData<HandshakeResponse>(handshakeResponse, uploadResponse.Data);
                        uploadResponse.Message = (uploadResponse.Success) 
                            ? "Server lost MTE state, client needed to handshake again, handshake successful, please try again."
                            : handshakeResponse.Message;
                        return uploadResponse;
                    }
                }

                if (useMte)
                {
                    //------------------
                    // Restore Decoder
                    //------------------
                    var decoder = new MteMkeDec();
                    var status = decoder.RestoreStateB64(decoderState);
                    if (status != mte_status_success)
                    {
                        uploadResponse.Success = false;
                        uploadResponse.ResultCode = Constants.RC_MTE_STATE_RETRIEVAL;
                        uploadResponse.Message = $"Failed to restore the MTE Decoder engine. Status: {decoder.GetStatusName(status)} / {decoder.GetStatusDescription(status)}";
                        return uploadResponse;
                    }

                    //----------------------------
                    // Start the chunking session
                    //----------------------------
                    status = decoder.StartDecrypt();
                    if (status != mte_status_success)
                    {
                        uploadResponse.Success = false;
                        uploadResponse.ResultCode = Constants.RC_MTE_STATE_RETRIEVAL;
                        uploadResponse.Message = "Failed to start decode chunk. Status: "
                                            + decoder.GetStatusName(status) + " / "
                                            + decoder.GetStatusDescription(status);
                        return uploadResponse;
                    }
                    //-----------------
                    // Decode the data
                    //-----------------
                    var decodedChunk = decoder.DecryptChunk(textResponse.Data);
                    var clearBytes = decoder.FinishDecrypt(out var finalStatus);
                    if (clearBytes == null) { clearBytes = Array.Empty<byte>(); }
                    if (finalStatus != mte_status_success)
                    {
                        uploadResponse.Success = false;
                        uploadResponse.ResultCode = Constants.RC_MTE_STATE_RETRIEVAL;
                        uploadResponse.Message = "Failed to finish decode. Status: "
                                            + decoder.GetStatusName(finalStatus) + " / "
                                            + decoder.GetStatusDescription(finalStatus);
                        return uploadResponse;
                    }
                    //---------------------
                    // Set decoded message
                    //---------------------
                    var decodedMessage = new byte[decodedChunk.Length + clearBytes.Length];
                    Buffer.BlockCopy(decodedChunk, 0, decodedMessage, 0, decodedChunk.Length);
                    Buffer.BlockCopy(clearBytes, 0, decodedMessage, decodedChunk.Length, clearBytes.Length);

                    //----------------------------
                    // Return the server response
                    //----------------------------
                    uploadResponse.Data.ServerResponse = Encoding.UTF8.GetString(decodedMessage);

                    //------------------------------
                    // Return the current seed count
                    //------------------------------
                    uploadResponse.Data.CurrentSeed = decoder.GetReseedCounter();

                    //------------------------
                    // Save the Decoder state
                    //------------------------
                    uploadResponse.Data.DecoderState = decoder.SaveStateB64();
                }
                else
                {
                    //----------------------------
                    // Return the server response
                    //----------------------------
                    uploadResponse.Data.ServerResponse = Encoding.UTF8.GetString(textResponse.Data);
                }

                //-----------------------------
                // Update the jwt/access_token
                //-----------------------------
                uploadResponse.access_token = textResponse.access_token;

            }
            catch (Exception e)
            {
                uploadResponse.Message = $"Exception uploading file to server. Ex: {e.Message}";
                uploadResponse.ResultCode = Constants.RC_UPLOAD_EXCEPTION;
                uploadResponse.Success = false;
            }
            return uploadResponse;
        }
        #endregion

        #region SetByteArray
        /// <summary>
        /// Sets the byte array.
        /// </summary>
        /// <param name="fileData">The file data.</param>
        /// <param name="bytesLeft">The bytes left.</param>
        private void SetByteArray(out byte[] fileData, long bytesLeft)
        {
            fileData = bytesLeft < RC_MAX_CHUNK_SIZE ? new byte[bytesLeft] : new byte[RC_MAX_CHUNK_SIZE];
        }
        #endregion

        #region HandshakeWithServer
        /// <summary>Handshakes with the server.</summary>
        /// <param name="clientId">The client identifier.</param>
        /// <returns>ResponseModel&lt;HandshakeResponse&gt;.</returns>
        public ResponseModel<HandshakeResponse> HandshakeWithServer(string clientId)
        {
            var response = new ResponseModel<HandshakeResponse> { Data = new HandshakeResponse() };
            try
            {
                //--------------------------------
                // Create clientId for this client
                //--------------------------------
                var handshake = new HandshakeModel { ConversationIdentifier = clientId };

                //-------------------------------------------
                // Create eclypses DH containers for handshake
                //-------------------------------------------
                var encoderEcdh = new EclypsesECDH();
                var decoderEcdh = new EclypsesECDH();

                //-------------------------------------------
                // Get the public key to send to other side
                //-------------------------------------------
                handshake.ClientEncoderPublicKey = encoderEcdh.GetPublicKey(encoderEcdh.GetTheContainer());
                handshake.ClientDecoderPublicKey = decoderEcdh.GetPublicKey(decoderEcdh.GetTheContainer());

                //-------------------
                // Perform handshake
                //-------------------
                var handshakeResponse =
                    MakeHttpCall($"{Constants.RestAPIName}/api/handshake", HttpMethod.Post, handshake.ConversationIdentifier,
                        Constants.JsonContentType, JsonSerializer.Serialize(handshake, Constants.JsonOptions)).Result;

                //---------------------------------------
                // De-serialize the result from handshake
                //---------------------------------------
                var serverResponse =
                    JsonSerializer.Deserialize<ResponseModel<HandshakeModel>>(handshakeResponse, Constants.JsonOptions);

                //-------------------------------------------------
                // If handshake was not successful return response
                //-------------------------------------------------
                if (!serverResponse.Success)
                {
                    Console.WriteLine($"Error making DH handshake for Client {clientId}: {serverResponse.Message}");
                    return response.ReturnWithResponseData<HandshakeModel>(serverResponse, response.Data);
                }

                //----------------------
                // Create shared secret
                //----------------------
                var encoderSharedSecretModel = encoderEcdh.ProcessPartnerPublicKey(serverResponse.Data.ClientEncoderPublicKey);
                var decoderSharedSecretModel = decoderEcdh.ProcessPartnerPublicKey(serverResponse.Data.ClientDecoderPublicKey);

                //--------------------------------
                // Set MTE settings and get state
                //--------------------------------
                if (!ulong.TryParse(serverResponse.Data.Timestamp, out ulong nonce))
                {
                    response.Success = false;
                    response.Message = $"Nonce is not valid ulong: {serverResponse.Data.Timestamp}.";
                    response.ResultCode = Constants.RC_INVALID_NONCE;
                    return response;
                }
                //----------------------------
                // Set Encoder and save state
                //----------------------------
                var encoder = new MteMkeEnc();
                encoder.SetEntropy(encoderSharedSecretModel.SharedSecret);
                encoder.SetNonce(nonce);
                var status = encoder.Instantiate(handshake.ConversationIdentifier);
                if (status != mte_status_success)
                {
                    response.Success = false;
                    response.Message = $"Failed to initialize the MTE encoder engine. Status: {encoder.GetStatusName(status)} / {encoder.GetStatusDescription(status)}";
                    response.ResultCode = Constants.RC_MTE_STATE_CREATION;
                    return response;
                }
                //-------------------------------
                // Get the DRBG Max Seed Interval
                //-------------------------------
                response.Data.MaxSeed = encoder.GetDrbgsReseedInterval(encoder.GetDrbg());
                //-----------------------
                // Get the Encoder State
                //-----------------------
                response.Data.EncoderState = encoder.SaveStateB64();

                //----------------------------
                // Set Decoder and save state
                //----------------------------
                var decoder = new MteMkeDec();
                decoder.SetEntropy(decoderSharedSecretModel.SharedSecret);
                decoder.SetNonce(nonce);
                status = decoder.Instantiate(handshake.ConversationIdentifier);
                if (status != mte_status_success)
                {
                    response.Success = false;
                    response.Message = $"Failed to initialize the MTE Decoder engine. Status: {decoder.GetStatusName(status)} / {decoder.GetStatusDescription(status)}";
                    response.ResultCode = Constants.RC_MTE_STATE_CREATION;
                    return response;
                }
                response.Data.DecoderState = decoder.SaveStateB64();
            }
            catch (Exception ex)
            {
                response.Message = $"Exception handshaking with server. Ex: {ex.Message}";
                response.ResultCode = Constants.RC_HANDSHAKE_EXCEPTION;
                response.Success = false;
            }
            return response;
        }
        #endregion

        #region MakeHttpCall (Async)
        /// <summary>
        /// Makes the HTTP call.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="method">The method.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="payload">The payload.</param>
        /// <returns>System.String.</returns>
        private static async Task<string> MakeHttpCall(string url, HttpMethod method, string clientId, string contentType, string payload = null, string authHeader = null)
        {
            //----------------------------------------------
            // Declare return payload string and initialize
            //----------------------------------------------
            var returnPayload = string.Empty;
            try
            {
                //-----------------------------------------
                // Set URI and other default Http settings
                //-----------------------------------------
                var uri = new Uri($"{url}");
                HttpResponseMessage responseMessage = null;
                var handler = new HttpClientHandler() { };
                using var client = new HttpClient(handler) { BaseAddress = uri };

                //-------------------------
                // Add client id to header 
                //-------------------------
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add(Constants.ClientIdHeader, clientId);

                //------------------------------------------------
                // If authHeader is included add it to the header
                //------------------------------------------------
                if (!string.IsNullOrWhiteSpace(authHeader))
                {
                    if (authHeader.StartsWith("Bearer"))
                        authHeader = authHeader.Substring("Bearer ".Length);

                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authHeader);
                }

                //---------------------------------------------------
                // Check if we have a payload or not and send request
                //---------------------------------------------------
                if (string.IsNullOrWhiteSpace(payload))
                {
                    //------------------------------------------------------------------------
                    // The only two methods that will not have any content are delete and get
                    //------------------------------------------------------------------------
                    responseMessage = method switch
                    {
                        var m when m == HttpMethod.Delete => await client.DeleteAsync(uri),
                        var m when m == HttpMethod.Get => await client.GetAsync(uri),
                        _ => await client.GetAsync(uri)
                    };
                }
                else
                {

                    //-----------------------------------------------
                    // Set byte payload and content type for request
                    //-----------------------------------------------
                    var bytePayload = Encoding.ASCII.GetBytes(payload);
                    var byteContent = new ByteArrayContent(bytePayload, 0, bytePayload.Length);
                    byteContent.Headers.Add("Content-Type", contentType);

                    //---------------------------------------------
                    // Create httpRequest with given byte payload
                    //---------------------------------------------
                    var httpRequest = new HttpRequestMessage
                    {
                        Method = method,
                        RequestUri = uri,
                        Content = byteContent
                    };

                    //------------------------
                    // Send out client request
                    //------------------------
                    responseMessage = await client.SendAsync(httpRequest).ConfigureAwait(false);
                }

                //------------------------------------------------------
                // If the response is successful get the return payload
                //------------------------------------------------------
                if (responseMessage.IsSuccessStatusCode)
                {
                    //------------------------------------------
                    // Use read as string if other content type
                    //------------------------------------------
                    returnPayload = await responseMessage.Content.ReadAsStringAsync();

                }
                else
                {
                    //-----------------------------------------------------------------
                    // If the response is NOT successful return error in ResponseModel
                    //-----------------------------------------------------------------
                    var errorResponse = new ResponseModel<object>();
                    errorResponse.Success = false;
                    errorResponse.Message = $"HttpResponse status was not okay, Message: {responseMessage.ReasonPhrase} -- Code: {responseMessage.StatusCode}";
                    errorResponse.ResultCode = Constants.RC_HTTP_ERROR;
                    errorResponse.Data = null;
                    returnPayload = JsonSerializer.Serialize(errorResponse, Constants.JsonOptions);
                }
            }
            catch (Exception ex)
            {
                var errorResponse = new ResponseModel<object>();
                errorResponse.Success = false;
                errorResponse.Message = $"Exception sending Message: {ex.Message}";
                errorResponse.ResultCode = Constants.RC_HTTP_ERROR;
                errorResponse.Data = null;
                returnPayload = JsonSerializer.Serialize(errorResponse, Constants.JsonOptions);
            }
            return returnPayload;
        }
        #endregion
    }
}

