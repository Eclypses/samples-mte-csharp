using MteDemoTest.Helpers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MteDemoTest.Models
{
    /// <summary>
    /// Constants used in MTE Demo program
    /// </summary>
    public class Constants
    {
        //-------------------------------------
        // Set up Json to be not case sensative
        //-------------------------------------
        public static JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public static readonly Cache<string, string> MteClientState = new Cache<string, string>();
        public static int ExpireMinutes = 1440;
        public static int IVExpirationMinutes = 52560000;
        public static string IVKey = "2C251123-995B-4CC9-B2A2-94B904FF0025";
        public static string EncoderPrefix = "ENC_";
        public static string DecoderPrefix = "DEC_";
        public static readonly string ClientIdHeader = "x-client-id";
        public const string STR_SUCCESS = "SUCCESS";
        public const string RC_SUCCESS = "000";
        public const string RC_VALIDATION_ERROR = "100";
        
        public const string RC_MTE_ENCODE_CHUNK_ERROR = "103";
        public const string RC_MTE_ENCODE_EXCEPTION = "104";
        public const string RC_MTE_ENCODE_FINISH_ERROR = "105";
        public const string RC_MTE_DECODE_CHUNK_ERROR = "111";
        public const string RC_MTE_DECODE_EXCEPTION = "112";
        public const string RC_MTE_DECODE_FINISH_ERROR = "113";
        public const string RC_MTE_STATE_CREATION = "114";
        public const string RC_MTE_STATE_RETRIEVAL = "115";
        public const string RC_MTE_STATE_SAVE = "116";
        public const string RC_MTE_STATE_NOT_FOUND = "117";

        public const string RC_CONTROLLER_EXCEPTION = "200";
        public const string RC_REPOSITORY_EXCEPTION = "201";
        
    }
}
