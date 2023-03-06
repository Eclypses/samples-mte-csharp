namespace MteConsoleUploadTest.Models
{
    /// <summary>
    /// Handshake Response
    /// </summary>
    public class HandshakeResponse
    {
        /// <summary>
        /// Holds the current encoder State
        /// </summary>
        public string EncoderState { get; set; }
        /// <summary>
        /// Holds the current decoder state
        /// </summary>
        public string DecoderState { get; set; }
        /// <summary>
        /// Holds the max DRBG reseed interval
        /// </summary>
        public ulong MaxSeed { get; set; }
    }
}
