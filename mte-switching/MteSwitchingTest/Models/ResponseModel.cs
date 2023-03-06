using System;
using System.Text.Json.Serialization;

namespace MteSwitchingTest.Models
{
    public class ResponseModel<T> : ResponseModel
    {
        [JsonPropertyName("Data")]
        public T Data { get; set; }

        public ResponseModel(T data) : base()
        {
            Data = data;
        }

        public ResponseModel()
        {

        }
    }
    public class ResponseModel
    {
        [JsonPropertyName("Success")]
        public bool Success { get; set; }
        [JsonPropertyName("Message")]
        public string Message { get; set; }
        [JsonPropertyName("ResultCode")]
        public string ResultCode { get; set; }
        [JsonPropertyName("ExceptionUid")]
        public string ExceptionUid { get; set; }
        /// <summary>
        /// Gets or sets the token expires in date time for the JWT Token.
        /// </summary>
        /// <value>The token expires in.</value>
        public DateTimeOffset token_expires_in { get; set; }
        /// <summary>
        /// Gets or sets the access token (JWT).
        /// </summary>
        /// <value>The access token.</value>
        [JsonPropertyName("access_token")]
        public string access_token { get; set; }

        public ResponseModel()
        {
            Message = string.Empty;
            Success = true;
            ResultCode = Constants.RC_SUCCESS;
            ExceptionUid = string.Empty;
            token_expires_in = DateTimeOffset.FromUnixTimeSeconds(60);
            access_token = string.Empty;
        }
    }
}
