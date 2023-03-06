using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MteSwitchingTest.Models
{
    internal class Constants
    {
        //----------------------
        // MTE Client ID header
        //----------------------
        public static readonly string ClientIdHeader = "x-client-id";
        //------------------
        // Set Rest API URL
        //------------------
        // Use this to run locally against MteDemo API
        //public static readonly string RestAPIName = "http://localhost:52603";
        // Use this to run against public api
        public static readonly string RestAPIName = "https://dev-jumpstart-csharp.eclypses.com";

        public static readonly string JsonContentType = "application/json";
        public static readonly string TextContentType = "text/plain";
        public static readonly double ReSeedPercentage = .9;

        //-------------------------------------
        // Set up Json to be not case sensative
        //-------------------------------------
        public static JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        //-------------------
        // Misc result codes
        //-------------------
        public const string STR_SUCCESS = "SUCCESS";
        public const string RC_SUCCESS = "000";
        public const string RC_VALIDATION_ERROR = "100";
        public const string RC_MTE_ENCODE_EXCEPTION = "110";
        public const string RC_MTE_DECODE_EXCEPTION = "120";
        public const string RC_MTE_STATE_CREATION = "130";
        public const string RC_MTE_STATE_RETRIEVAL = "131";
        public const string RC_MTE_STATE_SAVE = "132";
        public const string RC_MTE_STATE_NOT_FOUND = "133";
        public const string RC_INVALID_NONCE = "140";

        public const string RC_HTTP_ERROR = "300";
        public const string RC_UPLOAD_EXCEPTION = "301";        
        public const string RC_HANDSHAKE_EXCEPTION = "302";
        public const string RC_LOGIN_EXCEPTION = "303";
    }
}
