namespace MteConsoleUploadTest.Models
{
    public class UploadResponse
    {
        /// <summary>
        /// Response from server after upload
        /// </summary>
        public string ServerResponse { get; set; }
        /// <summary>
        /// Current Encoder state after upload
        /// </summary>
        public string EncoderState { get; set; }
        /// <summary>
        /// Current Decoder state after upload
        /// </summary>
        public string DecoderState { get; set; }
        /// <summary>
        /// Current seed count
        /// </summary>
        public ulong CurrentSeed { get; set; }
    }
}
