using System.Text.Json;
using System.Text.Json.Serialization;

namespace HandshakeSample.Models
{
    internal class Constants
    {
        //----------------------
        // MTE Client ID header
        //----------------------
        public static readonly string ClientIdHeader = "x-client-id";
        //---------------------------------------
        // Set Rest API URL and content settings
        //---------------------------------------
        // Use this URL to run with local CSharp API
        public static readonly string RestAPIName = "http://localhost:52603";
        // Use this URL to run with public CSharp API
        //public static readonly string RestAPIName = "https://dev-jumpstart-csharp.eclypses.com";
        public static readonly string HandshakeRoute = "/api/handshake";
        public static readonly string JsonContentType = "application/json";
        public static readonly string TextContentType = "text/plain";
        //-------------------------------------
        // Set up Json to be not case sensative
        //-------------------------------------
        public static JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
        //-------------------
        // Result Codes
        //-------------------
        public const string STR_SUCCESS = "SUCCESS";
        public const string RC_SUCCESS = "000";
        
        public const string RC_VALIDATION_ERROR = "100";
        
        public const string RC_MTE_ENCODE_ERROR = "110";
        
        public const string RC_MTE_DECODE_ERROR = "120";

        public const string RC_MTE_STATE_EXCEPTION = "130";
        public const string RC_MTE_STATE_CREATION_ERROR = "131";
        public const string RC_MTE_STATE_RETRIEVAL_ERROR = "132";
        public const string RC_MTE_STATE_SAVE_ERROR = "133";
        public const string RC_MTE_STATE_NOT_FOUND = "134";
        
        public const string RC_INVALID_NONCE = "140";
        public const string RC_INVALID_ENTROPY = "141";
        public const string RC_INVALID_PERSONAL = "142";

        public const string RC_HTTP_ERROR = "300";
        public const string RC_UPLOAD_EXCEPTION = "301";
        public const string RC_HANDSHAKE_EXCEPTION = "302";
        public const string RC_LOGIN_EXCEPTION = "303";
    }
}
