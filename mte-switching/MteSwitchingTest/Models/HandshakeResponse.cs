namespace MteSwitchingTest.Models
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
    }
}
