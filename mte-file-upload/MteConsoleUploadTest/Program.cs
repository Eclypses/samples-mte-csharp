using MteConsoleUploadTest.Models;
using System;
using System.Windows.Forms;

namespace MteConsoleUploadTest
{
    class Program
    {
        //------------------
        // Set if using MTE
        //------------------
        private const bool _useMte = true;
        private static ulong _maxSeed = 0;
        

        [STAThread]
        static void Main()
        {
            //--------------------------
            // Create Upload File class
            //--------------------------
            UploadFile uploadFile = new UploadFile();

            //-------------------------------------------------
            // Initialize Encoder and Decoder and set clientId
            //-------------------------------------------------
            HandshakeResponse handshake = new HandshakeResponse { EncoderState = String.Empty, DecoderState = String.Empty };
            string clientId = Guid.NewGuid().ToString();

            //--------------------------------------
            // Handshake with server and create MTE 
            //--------------------------------------
            ResponseModel<HandshakeResponse> handshakeResponse = uploadFile.HandshakeWithServer(clientId);
            if (!handshakeResponse.Success)
            {
                throw new ApplicationException($"Error trying to handshake with server: {handshakeResponse.Message}");
            }
            //--------------------------
            // Set the MaxSeed Interval
            //--------------------------
            _maxSeed = handshakeResponse.Data.MaxSeed;
            //-------------------------------
            // Set Decoder and Encoder state
            //-------------------------------
            handshake.DecoderState = handshakeResponse.Data.DecoderState;
            handshake.EncoderState = handshakeResponse.Data.EncoderState;

            //--------------------------------------------
            // Allow File Upload till user selects to end
            //--------------------------------------------
            while (true)
            {
                //--------------------------------
                // Prompt user for file to upload
                //--------------------------------
                string path = string.Empty;
                while (string.IsNullOrWhiteSpace(path))
                {
                    OpenFileDialog dialog = new OpenFileDialog();
                    if (DialogResult.OK == dialog.ShowDialog())
                    {
                        path = dialog.FileName;
                    }
                }

                //---------------------
                // Send file to server
                //---------------------
                ResponseModel<UploadResponse> uploadResponse = uploadFile.Send(path, _useMte, handshake.EncoderState, handshake.DecoderState, clientId);
                if (!uploadResponse.Success)
                {
                    //---------------------
                    // If unsuccessful end
                    //---------------------
                    throw new ApplicationException($"Error uploading file: {uploadResponse.Message}"); 
                }
                Console.WriteLine(uploadResponse.Data.ServerResponse);

                //-------------------------
                // Check current seed life
                //-------------------------
                if (uploadResponse.Data.CurrentSeed > (_maxSeed * 0.9))
                {
                    handshakeResponse = uploadFile.HandshakeWithServer(clientId);
                    if (!handshakeResponse.Success)
                    {
                        throw new ApplicationException($"Error trying to handshake to reseed MTE: {handshakeResponse.Message}");
                    }
                    //--------------------------------------------------------
                    // Update Encoder and Decoder states to be latest version
                    //--------------------------------------------------------
                    handshake.EncoderState = handshakeResponse.Data.EncoderState;
                    handshake.DecoderState = handshakeResponse.Data.DecoderState;
                }
                else{
                    //--------------------------------------------------------
                    // Update Encoder and Decoder states to be latest version
                    //--------------------------------------------------------
                    handshake.EncoderState = uploadResponse.Data.EncoderState;
                    handshake.DecoderState = uploadResponse.Data.DecoderState;
                }


                //--------------------------------------
                // Prompt to upload another file or not
                //--------------------------------------
                Console.WriteLine($"Would you like to upload an additional file? (y/n)");
                string sendAdditional = Console.ReadLine();
                if (sendAdditional != null && sendAdditional.Equals("n", StringComparison.InvariantCultureIgnoreCase))
                {
                    break;
                }
            }
        }
    }
}
