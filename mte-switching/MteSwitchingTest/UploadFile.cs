using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Eclypses.MTE;
using MteSwitchingTest.Models;
using PackageCSharpECDH;
using static System.Net.WebRequest;

namespace MteSwitchingTest
{
    public class UploadFile
    {
        #region UploadFile Constructor

        //---------------------------
        // Set upload File constants
        //---------------------------
        private const int RC_MAX_CHUNK_SIZE = 1024;
        private HttpWebRequest _webRequest = null;
        private FileStream _fileReader = null;
        private Stream _requestStream = null;

        private ulong _maxReseedInterval = 0;

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
        /// <param name="authHeader">The JWT for the authentication header.</param>
        /// <returns>System.Int32.</returns>
        public ResponseModel<UploadResponse> Send(string path,
                                                bool useMte,
                                                string encoderState,
                                                string decoderState,
                                                string clientId,
                                                string authHeader)
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
                var fileUrl = Path.Combine($"{Constants.RestAPIName}/FileUploadLogin/", urlType + "?name=" + file.Name);

                //-----------------------------------
                // Create file stream and webRequest
                //-----------------------------------
                _fileReader = new FileStream(path, FileMode.Open, FileAccess.Read);
                _webRequest = (HttpWebRequest)Create(fileUrl);
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

                //-------------------------
                // Add client id to header if not null
                //-------------------------
                if (!string.IsNullOrWhiteSpace(clientId))
                {
                    _webRequest.Headers.Add(Constants.ClientIdHeader, clientId);
                }

                if (!string.IsNullOrWhiteSpace(authHeader))
                {
                    if (authHeader.StartsWith("Bearer"))
                        authHeader = authHeader.Substring("Bearer ".Length);

                    _webRequest.Headers.Add("Authorization", "Bearer " + authHeader);
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
                    if (status != MteStatus.mte_status_success)
                    {
                        uploadResponse.Success = false;
                        uploadResponse.ResultCode = Constants.RC_MTE_STATE_RETRIEVAL;
                        uploadResponse.Message = $"Failed to restore MTE Encoder engine. Status: {encoder.GetStatusName(status)} / {encoder.GetStatusDescription(status)}";
                        return uploadResponse;
                    }

                    //----------------------------
                    // start the chunking session
                    //----------------------------
                    status = encoder.StartEncrypt();
                    if (status != MteStatus.mte_status_success)
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
                    SetByteArray(out var fileData, remainingBytes);
                    var done = _fileReader.Read(fileData, 0, fileData.Length);
                    if (useMte)
                    {
                        //------------------------------------------------------------
                        // Encode the data in place - encoded data put back in buffer
                        //------------------------------------------------------------
                        var chunkStatus = encoder.EncryptChunk(fileData, 0, fileData.Length);
                        if (chunkStatus != MteStatus.mte_status_success)
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
                    var finalEncodedChunk = encoder.FinishEncrypt(out MteStatus finishStatus);
                    if (finishStatus != MteStatus.mte_status_success)
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


                    //-----------------------
                    // Save the Encoder State
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
                using var data = response.GetResponseStream();
                if (data != null)
                {
                    using var reader = new StreamReader(data);
                    var text = reader.ReadToEnd();

                    var textResponse =
                        JsonSerializer.Deserialize<ResponseModel<byte[]>>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (textResponse is { Success: false })
                    {
                        //-----------------------------------
                        // Check if we need to "re-handshake"
                        //-----------------------------------
                        if (textResponse.ResultCode.Equals(Constants.RC_MTE_STATE_NOT_FOUND,
                                StringComparison.InvariantCultureIgnoreCase))
                        {
                            //-------------------------------------------------------------------------
                            // the server does not have this client's state - we should "re-handshake"
                            //-------------------------------------------------------------------------
                            var handshakeResponse = HandshakeWithServer(clientId, authHeader);

                            //-----------------------------------------------------------
                            // return response, if successful give message to try again.
                            //-----------------------------------------------------------
                            uploadResponse.Success = handshakeResponse.Success;
                            uploadResponse.Message = (uploadResponse.Success)
                                ? "Server lost MTE state, client needed to handshake again, handshake successful, please try again."
                                : handshakeResponse.Message;
                            uploadResponse.ResultCode = handshakeResponse.ResultCode;
                            uploadResponse.access_token = handshakeResponse.access_token;
                            uploadResponse.Data.DecoderState = handshakeResponse.Data.DecoderState;
                            uploadResponse.Data.EncoderState = handshakeResponse.Data.EncoderState;
                            return uploadResponse;
                        }
                    }
                    bool runReseed = false;
                    if (useMte)
                    {
                        //------------------
                        // Restore Decoder
                        //------------------
                        var decoder = new MteMkeDec();
                        var status = decoder.RestoreStateB64(decoderState);
                        if (status != MteStatus.mte_status_success)
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
                        if (status != MteStatus.mte_status_success)
                        {
                            throw new Exception("Failed to start decode chunk. Status: "
                                                + decoder.GetStatusName(status) + " / "
                                                + decoder.GetStatusDescription(status));
                        }
                        //-----------------
                        // Decode the data
                        //-----------------
                        if (textResponse != null && textResponse.Data != null && textResponse.Data.Length > 0)
                        {
                            var decodedChunk = decoder.DecryptChunk(textResponse.Data);
                            var clearBytes = decoder.FinishDecrypt(out var finalStatus) ?? Array.Empty<byte>();
                            if (finalStatus != MteStatus.mte_status_success)
                            {
                                throw new Exception("Failed to finish decode. Status: "
                                                    + decoder.GetStatusName(finalStatus) + " / "
                                                    + decoder.GetStatusDescription(finalStatus));
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
                        }
                        // Check Re-Seed Interval
                        var currentSeed = encoder.GetReseedCounter();
                        if (currentSeed > (_maxReseedInterval * Constants.ReSeedPercentage))
                        {
                            //--------------------------------------
                            // Handshake with server and create MTE 
                            //--------------------------------------
                            var handshakeResponse = HandshakeWithServer(clientId, authHeader);
                            if (!handshakeResponse.Success)
                            {
                                uploadResponse.Success = handshakeResponse.Success;
                                uploadResponse.Message = handshakeResponse.Message;
                                uploadResponse.ResultCode = handshakeResponse.ResultCode;
                                uploadResponse.access_token = handshakeResponse.access_token;
                                return uploadResponse;
                            }

                            // Set access token and Encoder and Decoder states
                            uploadResponse.Data.DecoderState = handshakeResponse.Data.DecoderState;
                            uploadResponse.Data.EncoderState = handshakeResponse.Data.EncoderState;
                            uploadResponse.access_token = handshakeResponse.access_token;
                            runReseed = true;
                        }
                        else
                        {

                            //------------------------
                            // Save the Decoder state
                            //------------------------
                            uploadResponse.Data.DecoderState = decoder.SaveStateB64();
                        }
                    }
                    else
                    {
                        //----------------------------
                        // Return the server response
                        //----------------------------
                        if (textResponse != null)
                            uploadResponse.Data.ServerResponse = Encoding.UTF8.GetString(textResponse.Data);
                    }

                    //-----------------------------
                    // update the JWT/access_token
                    //-----------------------------
                    if (textResponse != null && !runReseed) uploadResponse.access_token = textResponse.access_token;
                }
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
        private static void SetByteArray(out byte[] fileData, long bytesLeft)
        {
            fileData = bytesLeft < RC_MAX_CHUNK_SIZE ? new byte[bytesLeft] : new byte[RC_MAX_CHUNK_SIZE];
        }
        #endregion

        #region HandshakeWithServer
        /// <summary>Handshakes the with server.</summary>
        /// <param name="clientId">The client identifier.</param>
        /// <returns>ResponseModel&lt;HandshakeResponse&gt;.</returns>
        public ResponseModel<HandshakeResponse> HandshakeWithServer(string clientId, string authHeader = null)
        {
            var response = new ResponseModel<HandshakeResponse> { Data = new HandshakeResponse() };
            try
            {
                //--------------------------------
                // create clientId for this client
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
                string handshakeResponse =
                    MakeHttpCall($"{Constants.RestAPIName}/api/handshake", HttpMethod.Post, handshake.ConversationIdentifier,
                        Constants.JsonContentType, JsonSerializer.Serialize(handshake, Constants.JsonOptions), authHeader).Result;

                //---------------------------------------
                // De-serialize the result from handshake
                //---------------------------------------
                var serverResponse =
                    JsonSerializer.Deserialize<ResponseModel<HandshakeModel>>(handshakeResponse, Constants.JsonOptions);

                //---------------------------------------
                // If handshake was not successful break
                //---------------------------------------
                if (serverResponse is { Success: false })
                {
                    response.Success = serverResponse.Success;
                    response.Message = serverResponse.Message;
                    response.ResultCode = serverResponse.ResultCode;
                    response.access_token = serverResponse.access_token;
                    Console.WriteLine($"Error making DH handshake for Client {clientId}: {serverResponse.Message}");
                    return response;
                }

                //----------------------
                // Create shared secret
                //----------------------
                if (serverResponse != null)
                {
                    var encoderSharedSecretModel = encoderEcdh.ProcessPartnerPublicKey(serverResponse.Data.ClientEncoderPublicKey);
                    var decoderSharedSecretModel = decoderEcdh.ProcessPartnerPublicKey(serverResponse.Data.ClientDecoderPublicKey);

                    //--------------------------------
                    // Set MTE settings and get state
                    //--------------------------------
                    if (!ulong.TryParse(serverResponse.Data.Timestamp, out var nonce))
                    {
                        response.Success = false;
                        response.Message = $"Nonce is not valid ulong: {serverResponse.Data.Timestamp}.";
                        response.ResultCode = Constants.RC_INVALID_NONCE;
                        response.access_token = serverResponse.access_token;
                        return response;
                    }
                    //----------------------------
                    // Set Encoder and save state
                    //----------------------------
                    var encoder = new MteMkeEnc();
                    encoder.SetEntropy(encoderSharedSecretModel.SharedSecret);
                    encoder.SetNonce(nonce);
                    var status = encoder.Instantiate(handshake.ConversationIdentifier);
                    if (status != MteStatus.mte_status_success)
                    {
                        response.Success = false;
                        response.Message = $"Failed to initialize the MTE Encoder engine. Status: {encoder.GetStatusName(status)} / {encoder.GetStatusDescription(status)}";
                        response.ResultCode = Constants.RC_MTE_STATE_CREATION;
                        return response;
                    }

                    _maxReseedInterval = encoder.GetDrbgsReseedInterval(encoder.GetDrbg());

                    response.Data.EncoderState = encoder.SaveStateB64();

                    //----------------------------
                    // Set Decoder and save state
                    //----------------------------
                    var decoder = new MteMkeDec();
                    decoder.SetEntropy(decoderSharedSecretModel.SharedSecret);
                    decoder.SetNonce(nonce);
                    status = decoder.Instantiate(handshake.ConversationIdentifier);
                    if (status != MteStatus.mte_status_success)
                    {
                        response.Success = false;
                        response.Message = $"Failed to initialize the MTE Decoder engine. Status: {decoder.GetStatusName(status)} / {decoder.GetStatusDescription(status)}";
                        response.ResultCode = Constants.RC_MTE_STATE_CREATION;
                        return response;
                    }
                    response.Data.DecoderState = decoder.SaveStateB64();
                    response.access_token = serverResponse.access_token;
                }
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

        /// <summary>
        /// Login to the server using MTE Core
        /// </summary>
        /// <param name="clientId">The client id</param>
        /// <param name="encoderState">The current Encoder State</param>
        /// <param name="decoderState">The current Decoder State</param>
        /// <returns></returns>
        public ResponseModel<LoginResponse> LoginToServer(string clientId, string encoderState, string decoderState)
        {
            var response = new ResponseModel<LoginResponse> { Data = new LoginResponse { EncoderState = encoderState, DecoderState = decoderState } };
            try
            {
                var login = new LoginModel { Password = "P@ssw0rd!", UserName = "email@eclypses.com" };

                //---------------------------------
                // Serialize login model and encode
                //---------------------------------
                var serializedLogin = JsonSerializer.Serialize(login);

                //----------------------------------
                // Encode outgoing message with MTE
                //----------------------------------
                var enc = new MteEnc();
                var encoderStatus = enc.RestoreStateB64(encoderState);
                if (encoderStatus != MteStatus.mte_status_success)
                {
                    response.Message = $"Failed to restore MTE Encoder engine. Status: {enc.GetStatusName(encoderStatus)} / {enc.GetStatusDescription(encoderStatus)}";
                    response.Success = false;
                    response.ExceptionUid = Guid.NewGuid().ToString();
                    response.ResultCode = Constants.RC_MTE_STATE_RETRIEVAL;
                    return response;
                }
                var encodeResult = enc.EncodeB64(serializedLogin, out encoderStatus);
                if (encoderStatus != MteStatus.mte_status_success)
                {
                    response.Message = $"Failed to encode the login. Status: {enc.GetStatusName(encoderStatus)} / {enc.GetStatusDescription(encoderStatus)}";
                    response.Success = false;
                    response.ExceptionUid = Guid.NewGuid().ToString();
                    response.ResultCode = Constants.RC_MTE_ENCODE_EXCEPTION;
                    return response;
                }

                //---------------------------
                // Save updated Encoder State
                //---------------------------
                response.Data.EncoderState = enc.SaveStateB64();

                //-------------------
                // Perform Login
                //-------------------
                var loginResponse =
                    MakeHttpCall($"{Constants.RestAPIName}/api/login", HttpMethod.Post, clientId,
                        Constants.TextContentType, encodeResult).Result;

                //---------------------------------------
                // De-serialize the result from login
                //---------------------------------------
                var serverResponse =
                    JsonSerializer.Deserialize<ResponseModel<string>>(loginResponse, Constants.JsonOptions);

                //--------------
                // If error end
                //--------------
                if (serverResponse is { Success: false })
                {
                    response.Message = serverResponse.Message;
                    response.Success = serverResponse.Success;
                    response.ExceptionUid = serverResponse.ExceptionUid;
                    response.ResultCode = serverResponse.ResultCode;
                    return response;
                }

                //----------------------
                // Set JWT/access_token
                //----------------------
                if (serverResponse != null)
                {
                    response.access_token = serverResponse.access_token;

                    //-----------------------------------------------
                    // Decode the response and re-save Decoder State
                    //-----------------------------------------------
                    var dec = new MteDec();
                    var decoderStatus = dec.RestoreStateB64(decoderState);
                    if (decoderStatus != MteStatus.mte_status_success)
                    {
                        response.Message =
                            $"Failed to restore MTE Decoder engine. Status: {enc.GetStatusName(decoderStatus)} / {enc.GetStatusDescription(decoderStatus)}";
                        response.Success = false;
                        response.ExceptionUid = Guid.NewGuid().ToString();
                        response.ResultCode = Constants.RC_MTE_STATE_RETRIEVAL;
                        return response;
                    }

                    var decodedResult = dec.DecodeStrB64(serverResponse.Data, out decoderStatus);
                    if (decoderStatus != MteStatus.mte_status_success)
                    {
                        response.Message =
                            $"Failed to decode message. Status: {enc.GetStatusName(decoderStatus)} / {enc.GetStatusDescription(decoderStatus)}";
                        response.Success = false;
                        response.ExceptionUid = Guid.NewGuid().ToString();
                        response.ResultCode = Constants.RC_MTE_DECODE_EXCEPTION;
                        return response;
                    }

                    Console.WriteLine($"Login response: {decodedResult}");
                    response.Data.DecoderState = dec.SaveStateB64();
                    response.Data.LoginMessage = decodedResult;
                }
            }
            catch (Exception ex)
            {
                response.Message = $"Exception during login: {ex.Message}";
                response.Success = false;
                response.ExceptionUid = Guid.NewGuid().ToString();
                response.ResultCode = Constants.RC_LOGIN_EXCEPTION;
            }
            return response;
        }

        #region MakeHttpCall (Async)
        /// <summary>
        /// Makes the HTTP call.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="method">The method.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authHeader">The Authorization header.</param>
        /// <returns>System.String.</returns>
        private static async Task<string> MakeHttpCall(string url, HttpMethod method, string clientId, string contentType, string payload = null, string authHeader = null)
        {
            //----------------------------------------------
            // Declare return payload string and initialize
            //----------------------------------------------
            string returnPayload;
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
                        { } when method == HttpMethod.Delete => await client.DeleteAsync(uri),
                        { } when method == HttpMethod.Get => await client.GetAsync(uri),
                        _ => await client.GetAsync(uri)
                    };
                }
                else
                {
                    //Console.WriteLine($"Sending: '{payload}'");

                    //-----------------------------------------------
                    // Set byte payload and content type for request
                    //-----------------------------------------------
                    byte[] bytePayload = Encoding.ASCII.GetBytes(payload);
                    ByteArrayContent byteContent = new ByteArrayContent(bytePayload, 0, bytePayload.Length);
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
                    var errorResponse = new ResponseModel<object>
                    {
                        Success = false,
                        Message = $"HttpResponse status was not okay, Message: {responseMessage.ReasonPhrase} -- Code: {responseMessage.StatusCode}",
                        ResultCode = Constants.RC_HTTP_ERROR,
                        Data = null
                    };
                    returnPayload = JsonSerializer.Serialize(errorResponse, Constants.JsonOptions);
                }
            }
            catch (Exception ex)
            {
                var errorResponse = new ResponseModel<object>
                {
                    Success = false,
                    Message = $"Exception sending Message: {ex.Message}",
                    ResultCode = Constants.RC_HTTP_ERROR,
                    Data = null
                };
                returnPayload = JsonSerializer.Serialize(errorResponse, Constants.JsonOptions);
            }
            return returnPayload;
        }
        #endregion
    }
}

