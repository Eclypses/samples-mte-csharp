using HandshakeSample.Models;
using PackageCSharpECDH;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HandshakeSample 
{
    internal class Program
    {
        // -------------------------------------------------------------------------
        // This simple program demonstraits diffie-Hellman key exchange with server
        // -------------------------------------------------------------------------
        // The purpose of the handshake is to create unique entropy for mte
        // in a secure manner for the encoder and decoder.
        // The "client" creates the personalization string or ConversationIdentifier.
        // the "server" creates the nonce in the form of a timestamp.
        // -------------------------------------------------------------------------
        static void Main(string[] args)
        {
            //--------------
            // Set clientId
            //--------------
            string clientId = Guid.NewGuid().ToString();

            //-----------------------
            // Handshake with server  
            //-----------------------
            ResponseModel<SharedSecrets> handshakeResponse = HandshakeWithServer(clientId);
            if (!handshakeResponse.Success)
            {
                Console.WriteLine($"Error during handshake: {handshakeResponse.Message}");
                //----------------------
                // Promot to end program
                //----------------------
                PromptToEnd();
                return;
            }
            //--------------------------------------------------------------
            // For demonstration purposes ONLY displaying resulting entropy
            //--------------------------------------------------------------
            Console.WriteLine($"Handshake successful!");
            Console.WriteLine($"Encoder Entropy: {handshakeResponse.Data.EncoderSharedSecret}");
            Console.WriteLine($"Decoder Entropy: {handshakeResponse.Data.DecoderSharedSecret}");
            //----------------------
            // Promot to end program
            //----------------------
            PromptToEnd();
        }

        #region HandshakeWithServer
        /// <summary>Handshakes the with server.</summary>
        /// <param name="clientId">The client identifier.</param>
        /// <returns>ResponseModel&lt;SharedSecrets&gt;.</returns>
        public static ResponseModel<SharedSecrets> HandshakeWithServer(string clientId)
        {
            ResponseModel<SharedSecrets> response = new() { Data = new SharedSecrets() };
            try
            {
                //--------------------------------
                // create clientId for this client
                //--------------------------------
                HandshakeModel handshake = new() { ConversationIdentifier = clientId };

                //-------------------------------------------
                // Create eclypses DH containers for handshake
                //------------------------------------------- 
                EclypsesECDH encoderEcdh = new EclypsesECDH();
                EclypsesECDH decoderEcdh = new EclypsesECDH();

                //-------------------------------------------
                // Get the public key to send to other side
                //-------------------------------------------
                handshake.ClientEncoderPublicKey = encoderEcdh.GetPublicKey();
                handshake.ClientDecoderPublicKey = decoderEcdh.GetPublicKey();

                //-------------------
                // Perform handshake
                //-------------------
                string handshakeResponse =
                    MakeHttpCall($"{Constants.RestAPIName}{Constants.HandshakeRoute}", HttpMethod.Post, handshake.ConversationIdentifier,
                        Constants.JsonContentType, JsonSerializer.Serialize(handshake, Constants.JsonOptions)).Result;

                //---------------------------------------
                // Deserialize the result from handshake
                //---------------------------------------
                ResponseModel<HandshakeModel> serverResponse = new();
                serverResponse = JsonSerializer.Deserialize<ResponseModel<HandshakeModel>>(handshakeResponse, Constants.JsonOptions);

                //---------------------------------------
                // If handshake was not successful break
                //---------------------------------------
                if (!serverResponse.Success)
                {
                    return response.ReturnWithResponseData<HandshakeModel>(serverResponse, response.Data);
                }

                //----------------------
                // Create shared secret
                //----------------------
                var encoderSharedSecretModel = encoderEcdh.ProcessPartnerPublicKey(serverResponse.Data.ClientEncoderPublicKey);
                var decoderSharedSecretModel = decoderEcdh.ProcessPartnerPublicKey(serverResponse.Data.ClientDecoderPublicKey);

                //-----------------------------------------------
                // Return the encoder and decoder entropy
                //-----------------------------------------------
                response.Data = new SharedSecrets
                {
                    EncoderSharedSecret = Convert.ToBase64String(encoderSharedSecretModel.SharedSecret),
                    DecoderSharedSecret = Convert.ToBase64String(decoderSharedSecretModel.SharedSecret)
                };
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
        private static async Task<string> MakeHttpCall(string url, HttpMethod method, string clientId, string contentType, string? payload = null, string? authHeader = null)
        {
            //----------------------------------------------
            // Declare return payload string and initialize
            //----------------------------------------------
            string returnPayload = string.Empty;
            try
            {
                //-----------------------------------------
                // Set URI and other default Http settings
                //-----------------------------------------
                Uri uri = new Uri($"{url}");
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
                        HttpMethod m when m == HttpMethod.Delete => await client.DeleteAsync(uri),
                        HttpMethod m when m == HttpMethod.Get => await client.GetAsync(uri),
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
                    ResponseModel<object> errorResponse = new ResponseModel<object>();
                    errorResponse.Success = false;
                    errorResponse.Message = $"HttpResponse status was not okay, Message: {responseMessage.ReasonPhrase} -- Code: {responseMessage.StatusCode}";
                    errorResponse.ResultCode = Constants.RC_HTTP_ERROR;
                    errorResponse.Data = null;
                    returnPayload = JsonSerializer.Serialize(errorResponse, Constants.JsonOptions);
                }
            }
            catch (Exception ex)
            {
                ResponseModel<object> errorResponse = new ResponseModel<object>();
                errorResponse.Success = false;
                errorResponse.Message = $"Exception sending Message: {ex.Message}";
                errorResponse.ResultCode = Constants.RC_HTTP_ERROR;
                errorResponse.Data = null;
                returnPayload = JsonSerializer.Serialize(errorResponse, Constants.JsonOptions);
            }
            return returnPayload;
        }
        #endregion

        #region PromptToEnd
        /// <summary>
        /// Gives user prompt to end program
        /// </summary>
        private static void PromptToEnd()
        {
            //----------------------
            // Promot to end program
            //----------------------
            Console.WriteLine($"Press enter to end program");
            Console.ReadLine();
        } 
        #endregion

    }
}
