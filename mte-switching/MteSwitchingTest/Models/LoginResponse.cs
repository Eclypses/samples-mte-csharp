namespace MteSwitchingTest.Models
{
    public class LoginResponse
    {
        /// <summary>
        /// Login message received from server
        /// </summary>
        public string LoginMessage { get; set; }
        /// <summary>
        /// Current encoder state after login
        /// </summary>
        public string EncoderState { get; set; }
        /// <summary>
        /// Current decoder state after login
        /// </summary>
        public string DecoderState { get; set; }
    }
}
