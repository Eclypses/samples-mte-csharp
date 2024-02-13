using MteConsoleMultipleClients.Helpers;

namespace MteConsoleMultipleClients.Models
{
    public class Constants
    {
        //-------------------------
        // Cache MTE State Storage
        //-------------------------
        public static readonly Cache<string, string> MteClientState = new Cache<string, string>();
        //----------------
        // Cache settings
        //----------------
        public const int ExpireMinutes = 1440;
        public const int IVExpirationMinutes = 52560000;
        public const string IVKey = "981F1947-2867-4AD8-8A39-10197E4A2D51";
        public const string EncoderPrefix = "ENC_";
        public const string DecoderPrefix = "DEC_";
        //----------------------
        // MTE Client ID header
        //----------------------
        public static readonly string ClientIdHeader = "x-client-id";
        //-------------------
        // Misc result codes
        //-------------------
        public const string STR_SUCCESS = "SUCCESS";
        public const string RC_SUCCESS = "000";
        public const string RC_VALIDATION_ERROR = "100";
        public const string RC_MTE_ENCODE_EXCEPTION = "104";
        public const string RC_MTE_DECODE_EXCEPTION = "112";
        public const string RC_MTE_STATE_CREATION = "114";
        public const string RC_MTE_STATE_RETRIEVAL = "115";
        public const string RC_MTE_STATE_SAVE = "116";
        public const string RC_MTE_STATE_NOT_FOUND = "117";
        public const string RC_HTTP_ERROR = "200";
    }
}
