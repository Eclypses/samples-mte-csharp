using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MteConsoleUploadTest.Models
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
        // This should be used for server decoder
        //
        public byte[] ClientEncoderPublicKey { get; set; }


        // Diffie Hellman public key of the client decoder
        // This should be used for server encoder
        public byte[] ClientDecoderPublicKey { get; set; }
    }
}
