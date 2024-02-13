using System;

namespace HandshakeSample.Models
{
    /// <summary>
    /// Encoder and Decoder Shared Secrets
    /// </summary>
    internal class SharedSecrets
    {
        /// <summary>
        /// The Encoder Shared Secret
        /// </summary>
        public string EncoderSharedSecret { get; set; }
        /// <summary>
        /// The Decoder Shared Secret
        /// </summary>
        public string DecoderSharedSecret { get; set; }

        /// <summary>
        /// This method initializes the parameters for me
        /// </summary>
        public SharedSecrets()
        {
            this.EncoderSharedSecret = String.Empty;
            this.DecoderSharedSecret = String.Empty;
        }
    }
}
