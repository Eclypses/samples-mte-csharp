using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MteDemoTest.Models
{
    public class HandshakeModel
    {
        /// <summary>
        /// Calculated on the server, sent back to the client and used for Nonce
        /// </summary>
        [JsonPropertyName("Timestamp")]
        public string Timestamp { get; set; }
        //
        // Session identifier determined by the client - used as PK for storing the MTE STATE as well as looking up the shared secret
        //
        [JsonPropertyName("ConversationIdentifier")]
        public string ConversationIdentifier { get; set; }
        //
        // Diffie Hellman public key of the client Encoder
        // This should be used for server decoder
        //
        [JsonPropertyName("ClientEncoderPublicKey")]
        public byte[] ClientEncoderPublicKey { get; set; }

        /// <summary>Gets or sets the client decoder public key</summary>
        /// <value>The client decoder public key.</value>
        /// This should be used for server encoder
        [JsonPropertyName("ClientDecoderPublicKey")]
        public byte[] ClientDecoderPublicKey { get; set; }
    }
}
