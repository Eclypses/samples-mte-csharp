using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MteSwitchingTest.Models
{
    internal class HandshakeModel
    {
        /// <summary>
        /// Calculated on the server, sent back to the client and used for Nonce
        /// </summary>
        public string Timestamp { get; set; }
        //
        // Session identifier determined by the client
        // used as PK for storing the MTE STATE as well as looking up the shared secret
        //
        public string ConversationIdentifier { get; set; }
        //
        // Diffie Hellman public key of the client Encoder
        // This should be used for server Decoder
        //
        public byte[] ClientEncoderPublicKey { get; set; }


        // Diffie Hellman public key of the client Decoder
        // This should be used for server Encoder
        public byte[] ClientDecoderPublicKey { get; set; }
    }
}
