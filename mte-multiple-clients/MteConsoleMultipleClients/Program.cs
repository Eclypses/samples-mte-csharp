using Eclypses.MTE;
using MteConsoleMultipleClients.Helpers;
using MteConsoleMultipleClients.Models;
using PackageCSharpECDH;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MteConsoleMultipleClients
{
    internal class Program
    {
        //-----------------------------------------
        // Set max number of trips made per client
        //-----------------------------------------
        private const int MaxNumberOfTrips = 10;

        //------------------------------------------
        // Declare different possible content types
        //------------------------------------------
        private const string MteDataContentType = "application/octet-stream";
        private const string JsonContentType = "application/json";
        private const string TextContentType = "text/plain";

        // Max Seed Interval for Clients
        private static ulong _maxSeedInterval = 0;

        //------------------
        // Set Rest API URL
        //------------------
        // Use this URL to run locally against MteDemo API
        //private const string RestApiName = "http://localhost:52603";
        // Use this URL to run against Eclypses API
        private const string RestApiName = "https://dev-jumpstart-csharp.eclypses.com";

        //--------------------------------------
        // Encryption IV to put in memory cache
        //--------------------------------------
        private static string _encIV;
        
        //-------------------
        // Create AES Helper
        //-------------------
        private static readonly AesHelper Enc = new();

        // 
        private static JsonSerializerOptions _jsonOptions;

        public static async Task Main()
        {
            //-------------------
            // Create session IV 
            //-------------------
            _encIV = Guid.NewGuid().ToString();
            Constants.MteClientState.Store(Constants.IVKey, _encIV, TimeSpan.FromMinutes(Constants.IVExpirationMinutes));

            //-------------------------------------
            // Set up Json to be not case sensitive
            //-------------------------------------
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            _jsonOptions.Converters.Add(new JsonStringEnumConverter());


            //-----------------------------------------------
            // Prompt user to ask how many clients to create
            //-----------------------------------------------
            Console.WriteLine("How many clients? (Enter number between 1-50)");
            var clientNum = Convert.ToInt32(Console.ReadLine());

            //--------------------------------------
            // Create array for all conversationID's
            //--------------------------------------
            var clients = new Dictionary<int, string>();

            //-----------------------------------------
            // Run handshake and state for each client
            //-----------------------------------------
            for (var i = 1; i <= clientNum; i++)
            {
                //-----------------------------
                // Handshake client with server
                //-----------------------------
                var handshakeSuccessful = HandshakeWithServer(i, clients);
                if (handshakeSuccessful) continue;
                Console.WriteLine("Handshake unsuccessful!");
                throw new ApplicationException("Handshake unsuccessful!");
            }
            //----------------------------------------------------
            // Completed creating MTE states for number of clients
            //----------------------------------------------------
            Console.WriteLine($"Created MTE state for {clientNum}'s");

            while (true)
            {
                var rnd = new Random();
                //----------------------------------------------------
                // Iterate through clients and send out message async
                //----------------------------------------------------
                var tasks = new List<Task>();
                foreach (var lastTask in clients.Select(entry 
                             => new Task(() => { ContactServer(rnd, entry.Value, entry.Key, clients); })))
                {
                    tasks.Add(lastTask);
                    lastTask.Start();
                }
                Task.WaitAll(tasks.ToArray());

                //-----------------------------------------
                // End program or run contact server again
                //-----------------------------------------
                Console.WriteLine($"Completed sending messages to {clientNum} clients.");
                Console.WriteLine("Would you like to send additional messages to clients? (y/n)");
                var sendAdditional = Console.ReadLine();
                if (sendAdditional != null && sendAdditional.Equals("n", StringComparison.InvariantCultureIgnoreCase))
                {
                    break;
                }
            }
        }

        #region CreateMteStates
        /// <summary>
        /// Creates the MTE states.
        /// </summary>
        /// <param name="personal">The personal.</param>
        /// <param name="encoderEntropy">The Encoder entropy.</param>
        /// <param name="decoderEntropy">The Decoder entropy.</param>
        /// <param name="nonce">The nonce.</param>
        /// <returns>ResponseModel.</returns>
        private static ResponseModel CreateMteStates(string personal, byte[] encoderEntropy, byte[] decoderEntropy, ulong nonce)
        {
            var response = new ResponseModel();
            try
            {
                //--------------------
                // Create MTE Encoder 
                //--------------------
                var encoder = new MteEnc();
                encoder.SetEntropy(encoderEntropy);
                encoder.SetNonce(nonce);
                var status = encoder.Instantiate(personal);
                if (status != MteStatus.mte_status_success)
                {
                    Console.WriteLine($"Error creating Encoder: Status: {encoder.GetStatusName(status)} / {encoder.GetStatusDescription(status)}");
                    response.Message =
                        $"Error creating Encoder: Status: {encoder.GetStatusName(status)} / {encoder.GetStatusDescription(status)}";
                    response.ResultCode = Constants.RC_MTE_ENCODE_EXCEPTION;
                    response.Success = false;
                    return response;
                }
                //----------------------------------------
                // Get the Max Seed Interval if NOT set
                // This should be the same for ALL MTEs
                //----------------------------------------
                if (_maxSeedInterval <= 0)
                {
                    _maxSeedInterval = encoder.GetDrbgsReseedInterval(encoder.GetDrbg());
                }

                //------------------------
                // Save and encrypt state
                //------------------------
                var encoderState = encoder.SaveStateB64();
                var encryptedEncState = Enc.Encrypt(encoderState, personal, _encIV);
                Constants.MteClientState.Store($"{Constants.EncoderPrefix}{personal}", encryptedEncState, TimeSpan.FromMinutes(Constants.ExpireMinutes));

                //--------------------
                // Create MTE Decoder
                //--------------------
                var decoder = new MteDec();
                decoder.SetEntropy(decoderEntropy);
                decoder.SetNonce(nonce);
                status = decoder.Instantiate(personal);
                if (status != MteStatus.mte_status_success)
                {
                    Console.WriteLine($"Error creating Decoder: Status: {decoder.GetStatusName(status)} / {decoder.GetStatusDescription(status)}");
                    response.Message =
                        $"Error creating Decoder: Status: {decoder.GetStatusName(status)} / {decoder.GetStatusDescription(status)}";
                    response.ResultCode = Constants.RC_MTE_DECODE_EXCEPTION;
                    response.Success = false;
                    return response;
                }

                //------------------------
                // Save and encrypt state
                //------------------------
                var decodeState = decoder.SaveStateB64();
                var encryptedDecState = Enc.Encrypt(decodeState, personal, _encIV);

                Constants.MteClientState.Store($"{Constants.DecoderPrefix}{personal}", encryptedDecState, TimeSpan.FromMinutes(Constants.ExpireMinutes));
                response.Success = true;
                response.ResultCode = Constants.RC_SUCCESS;
                response.Message = Constants.STR_SUCCESS;
            }
            catch (Exception ex)
            {
                response.Message = $"Exception creating MTE state. Ex: {ex.Message}";
                response.ResultCode = Constants.RC_MTE_ENCODE_EXCEPTION;
                response.Success = false;
            }

            return response;
        }
        #endregion

        #region ContactServer (Async)
        /// <summary>
        /// Contacts the server.
        /// </summary>
        /// <param name="rnd">The random.</param>
        /// <param name="currentConversation">The current conversation.</param>
        /// <param name="clientNum">The i.</param>
        /// <param name="clients">The clients.</param>
        /// <exception cref="System.ApplicationException">Error restoring the Encoder MTE state for Client {i}: {encoder.GetStatusDescription(encoderStatus)}</exception>
        /// <exception cref="System.ApplicationException">Error restoring the Decoder MTE state for Client {i}: {decoder.GetStatusDescription(decoderStatus)}</exception>
        /// <exception cref="System.ApplicationException"></exception>
        private static Task ContactServer(Random rnd, 
                                          string currentConversation, 
                                          int clientNum,
                                          IDictionary<int, string> clients)
        {
            try
            {
                //-------------------------------------
                // Randomly select number of trips
                // between 1 and max number of trips
                //-------------------------------------
                var numberTrips = rnd.Next(1, MaxNumberOfTrips);

                //---------------------------------------
                // Send message selected number of trips
                //---------------------------------------
                for (var t = 0; t < numberTrips; t++)
                {
                    //-------------------------------------
                    // Get the current client Encoder state
                    //-------------------------------------
                    var encoderState = Constants.MteClientState.Get($"{Constants.EncoderPrefix}{currentConversation}");
                    var decryptedEncState = Enc.Decrypt(encoderState, currentConversation, _encIV);

                    //-------------------------------------
                    // Restore the Encoder ensure it works
                    //-------------------------------------
                    var encoder = new MteEnc();
                    var encoderStatus = encoder.RestoreStateB64(decryptedEncState);
                    if (encoderStatus != MteStatus.mte_status_success)
                    {
                        Console.WriteLine($"Error restoring the Encoder MTE state for Client {clientNum}: {encoder.GetStatusDescription(encoderStatus)}");
                        throw new ApplicationException($"Error restoring the Encoder MTE state for Client {clientNum}: {encoder.GetStatusDescription(encoderStatus)}");
                    }

                    //-------------------------------------
                    // Get the current client Decoder state
                    //-------------------------------------
                    var decoderState = Constants.MteClientState.Get($"{Constants.DecoderPrefix}{currentConversation}");
                    var decryptedDecState = Enc.Decrypt(decoderState, currentConversation, _encIV);

                    //-------------------------------------
                    // Restore the Decoder ensure it works
                    //-------------------------------------
                    var decoder = new MteDec();
                    var decoderStatus = decoder.RestoreStateB64(decryptedDecState);
                    if (decoderStatus != MteStatus.mte_status_success)
                    {
                        Console.WriteLine($"Error restoring the Decoder MTE state for Client {clientNum}: {decoder.GetStatusDescription(decoderStatus)}");
                        throw new ApplicationException($"Error restoring the Decoder MTE state for Client {clientNum}: {decoder.GetStatusDescription(decoderStatus)}");
                    }

                    //-------------------------
                    // Encode message to send
                    //-------------------------
                    var message = $"Hello from client {clientNum} for the {t + 1} time.";
                    var encodedPayload = encoder.EncodeB64(message, out MteStatus encodeStatus);
                    if(encodeStatus != MteStatus.mte_status_success)
                    {
                        Console.WriteLine($"Error encoding the message for Client {clientNum}: {encoder.GetStatusDescription(encodeStatus)}");
                        throw new ApplicationException($"Error encoding the message for Client {clientNum}: {encoder.GetStatusDescription(encodeStatus)}");
                    }
                    Console.WriteLine($"Sending message '{message}' to multi client server.");

                    //-----------------------------------------------------------
                    // Send encoded message to server, putting clientId in header
                    //-----------------------------------------------------------
                    var multiClientResponse =
                        MakeHttpCall($"{RestApiName}/api/multiclient", HttpMethod.Post, currentConversation, TextContentType, encodedPayload).Result;

                    //----------------------
                    // De-serialize response
                    //----------------------
                    var serverResponse =
                        JsonSerializer.Deserialize<ResponseModel<string>>(multiClientResponse, _jsonOptions);
                    if (!serverResponse.Success)
                    {
                        if (serverResponse.ResultCode.Equals(Constants.RC_MTE_STATE_NOT_FOUND,
                            StringComparison.InvariantCultureIgnoreCase))
                        {
                            //-------------------------------------------------------------------------
                            // The server does not have this client's state - we should "re-handshake"
                            //-------------------------------------------------------------------------
                            var handshakeIsSuccessful = HandshakeWithServer(clientNum, clients, currentConversation);
                            if (handshakeIsSuccessful) return Task.CompletedTask;
                            Console.WriteLine($"Error from server for client {clientNum}: {serverResponse.Message}");
                            throw new ApplicationException();
                        }
                    }
                    //------------------------
                    // Check Re-Seed Interval
                    //------------------------
                    var currentSeed = encoder.GetReseedCounter();
                    if (currentSeed > (_maxSeedInterval * .9))
                    {
                        //----------------------------------------
                        // If we have reached 90% -- Re-Handshake
                        //----------------------------------------
                        var handshakeIsSuccessful = HandshakeWithServer(clientNum, clients, currentConversation);
                        if (handshakeIsSuccessful) return Task.CompletedTask;
                        Console.WriteLine($"Error from server for client {clientNum}: {serverResponse.Message}");
                        throw new ApplicationException(
                            $"Error from server for client {clientNum}: {serverResponse.Message}");
                    }

                    //---------------------------------------------------
                    // If this was successful save the new Encoder state
                    //---------------------------------------------------
                    encoderState = encoder.SaveStateB64();
                    var encryptedEncState = Enc.Encrypt(encoderState, currentConversation, _encIV);
                    Constants.MteClientState.Store($"{Constants.EncoderPrefix}{currentConversation}", 
                                                   encryptedEncState, 
                                                   TimeSpan.FromMinutes(Constants.ExpireMinutes));

                    //-----------------------------
                    // Decode the incoming message
                    //-----------------------------
                    var decodedMessage = decoder.DecodeStrB64(serverResponse.Data, out decoderStatus);
                    if (decoderStatus != MteStatus.mte_status_success)
                    {
                        Console.WriteLine($"Error restoring the Decoder MTE state for Client {clientNum}: {decoder.GetStatusDescription(decoderStatus)}");
                        throw new ApplicationException($"Error restoring the Decoder MTE state for Client {clientNum}: {decoder.GetStatusDescription(decoderStatus)}");
                    }

                    //----------------------------------------
                    // If decode is successful save new state 
                    //----------------------------------------
                    decoderState = decoder.SaveStateB64();
                    var encryptedDecState = Enc.Encrypt(decoderState, currentConversation, _encIV);
                    Constants.MteClientState.Store($"{Constants.DecoderPrefix}{currentConversation}", 
                                                   encryptedDecState, 
                                                   TimeSpan.FromMinutes(Constants.ExpireMinutes));

                    //-------------------------------------
                    // Output incoming message from server
                    //-------------------------------------
                    Console.WriteLine($"Received '{decodedMessage}' from multi-client server.\n\n");
                    // Sleep between each call a random amount of time
                    // between 10 and 100 mill-seconds
                    Thread.Sleep(rnd.Next(0, 100));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            return Task.CompletedTask;
        }
        #endregion

        #region HandshakeWithServer
        /// <summary>
        /// Handshakes with the server.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="clients">The client dictionary</param>
        /// <param name="currentConversation">The current conversation GUID.</param>
        /// <returns><c>true</c> if success, <c>false</c> otherwise.</returns>
        private static bool HandshakeWithServer(int clientId, IDictionary<int, string> clients, string currentConversation = null)
        {
            try
            {
                Console.WriteLine($"Performing Handshake for Client {clientId}");

                //--------------------------------
                // create clientId for this client
                //--------------------------------
                var handshake = new HandshakeModel { ConversationIdentifier = Guid.NewGuid().ToString() };

                //----------------------------------------------------------
                // If current conversation GUID passed in update identifier
                //----------------------------------------------------------
                if (!string.IsNullOrWhiteSpace(currentConversation))
                {
                    handshake.ConversationIdentifier = currentConversation;
                }

                //-------------------------------------------------------------
                // Add client to dictionary list if this is a new conversation
                //-------------------------------------------------------------
                if (!clients.ContainsKey(clientId)) { 
                    clients.Add(clientId, handshake.ConversationIdentifier);
                }               

                //-------------------------------------------
                // Create Eclypses DH containers for handshake
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
                    MakeHttpCall($"{RestApiName}/api/handshake", HttpMethod.Post, handshake.ConversationIdentifier,
                        JsonContentType, JsonSerializer.Serialize(handshake, _jsonOptions)).Result;

                //---------------------------------------
                // De-serialize the result from handshake
                //---------------------------------------
                var response =
                    JsonSerializer.Deserialize<ResponseModel<HandshakeModel>>(handshakeResponse, _jsonOptions);

                //---------------------------------------
                // If handshake was not successful break
                //---------------------------------------
                if (response is { Success: false })
                {
                    Console.WriteLine($"Error making DH handshake for Client {clientId}: {response.Message}");
                    return false;
                }

                //----------------------
                // Create shared secret
                //----------------------
                if (response != null)
                {
                    var encoderSharedSecretModel = encoderEcdh.ProcessPartnerPublicKey(response.Data.ClientEncoderPublicKey);
                    var decoderSharedSecretModel = decoderEcdh.ProcessPartnerPublicKey(response.Data.ClientDecoderPublicKey);

                    //----------------------------------------------------------
                    // Create and store MTE Encoder and Decoder for this Client
                    //----------------------------------------------------------
                    var mteResponse = CreateMteStates(response.Data.ConversationIdentifier,
                        encoderSharedSecretModel.SharedSecret, 
                        decoderSharedSecretModel.SharedSecret,
                        Convert.ToUInt64(response.Data.Timestamp));

                    //----------------------------------------------------------
                    // Clear container to ensure key is different for each client
                    //----------------------------------------------------------
                    encoderEcdh.ClearContainer();
                    decoderEcdh.ClearContainer();

                    //-----------------------------------------
                    // If there was an error break out of loop
                    //-----------------------------------------
                    if (mteResponse.Success) return true;
                }

                if (response != null)
                    Console.WriteLine($"Error creating MTE states for Client {clientId}: {response.Message}");
                return false;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
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
        private static async Task<string> MakeHttpCall(string url, HttpMethod method, string clientId, string contentType, string payload = null)
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
                    //-------------------------------------------------
                    // Get the content header to determine content type
                    //-------------------------------------------------
                    var sentContentType = responseMessage.Content.Headers.ContentType;

                    //--------------------------------------------------
                    // If content type the MediaType MteDataContentType
                    //--------------------------------------------------
                    if (sentContentType is { MediaType: MteDataContentType })
                    {
                        //------------------------------------------------
                        // Get content length so we can set buffer length
                        //------------------------------------------------
                        var contentLength = responseMessage.Content.Headers.ContentLength ?? 0;
                        byte[] returnPayload2;
                        await using (var stream = responseMessage.Content.ReadAsStreamAsync().Result)
                        {
                            returnPayload2 = ReadByteStreamFully(stream, contentLength);
                        }
                        returnPayload = Encoding.UTF8.GetString(returnPayload2);
                    }
                    else
                    {
                        //------------------------------------------
                        // Use read as string if other content type
                        //------------------------------------------
                        returnPayload = await responseMessage.Content.ReadAsStringAsync();
                    }
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
                    returnPayload = JsonSerializer.Serialize(errorResponse, _jsonOptions);
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
                returnPayload = JsonSerializer.Serialize(errorResponse, _jsonOptions);
            }
            return returnPayload;
        } 
        #endregion

        #region ReadByteStreamFully

        /// <summary>Reads the byte stream fully.</summary>
        /// <param name="stream">The stream.</param>
        /// <param name="bufferLength">Length of buffer.</param>
        /// <returns>System.Byte[].</returns>
        private static byte[] ReadByteStreamFully(Stream stream, long bufferLength)
        {
            if(bufferLength == 0) { bufferLength = 32768; }
            // get length and give stream length it needs - save on memory
            var buffer = new byte[bufferLength];
            using var ms = new MemoryStream();
            while (true)
            {
                var read = stream.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                    return ms.ToArray();
                ms.Write(buffer, 0, read);
            }
        }
        #endregion
    }
}



