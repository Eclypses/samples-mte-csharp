using MteSwitchingTest.Models;
using System;
using System.Windows.Forms;

namespace MteSwitchingTest
{
    internal class Program
    {
        //------------------
        // set if using MTE
        //------------------
        private const bool RC_USE_MTE = true;


        [STAThread]
        static void Main()
        {
            //--------------------------
            // Create Upload File class
            //--------------------------
            var uploadFile = new UploadFile();

            //-------------------------------------------------
            // Initialize Encoder and Decoder and set clientId
            //-------------------------------------------------
            var handshake = new HandshakeResponse { EncoderState = string.Empty, DecoderState = string.Empty };
            var clientId = Guid.NewGuid().ToString();

            //--------------------------------------
            // Handshake with server and create MTE 
            //--------------------------------------
            var handshakeResponse = uploadFile.HandshakeWithServer(clientId);
            if (!handshakeResponse.Success)
            {
                throw new ApplicationException($"Error trying to handshake with server: {handshakeResponse.Message}");
            }

            //-------------------------------
            // Set Decoder and Encoder state
            //-------------------------------
            handshake.DecoderState = handshakeResponse.Data.DecoderState;
            handshake.EncoderState = handshakeResponse.Data.EncoderState;
            Console.WriteLine($"En: {handshake.EncoderState}");
            Console.WriteLine($"De: {handshake.DecoderState}");

            var loginResponse = uploadFile.LoginToServer(clientId, handshake.EncoderState, handshake.DecoderState);
            if (!loginResponse.Success)
            {
                throw new ApplicationException($"Error trying to login: {loginResponse.Message}");
            }

            //-------------------------------
            // Set Decoder and Encoder state
            //-------------------------------
            handshake.EncoderState = loginResponse.Data.EncoderState;
            handshake.DecoderState = loginResponse.Data.DecoderState;


            //--------------
            // Set the JWT
            //--------------
            var jwt = loginResponse.access_token;

            //--------------------------------------------
            // Allow File Upload till user selects to end
            //--------------------------------------------
            while (true)
            {
                //--------------------------------
                // Prompt user for file to upload
                //--------------------------------
                var path = string.Empty;
                while (string.IsNullOrWhiteSpace(path))
                {
                    var dialog = new OpenFileDialog();
                    if (DialogResult.OK == dialog.ShowDialog())
                    {
                        path = dialog.FileName;
                    }
                }

                //---------------------
                // Send file to server
                //---------------------
                var uploadResponse = uploadFile.Send(path, RC_USE_MTE, handshake.EncoderState, handshake.DecoderState, clientId, jwt);

                //---------------------
                // If unsuccessful end
                //---------------------
                if (!uploadResponse.Success)
                {
                    throw new ApplicationException($"Error uploading file: {uploadResponse.Message}"); 
                }
                Console.WriteLine(uploadResponse.Data.ServerResponse);

                //-----------------
                // Update the JWT
                //-----------------
                jwt = uploadResponse.access_token;


                //--------------------------------------------------------
                // Update Encoder and Decoder states to be latest version
                //--------------------------------------------------------
                handshake.EncoderState = uploadResponse.Data.EncoderState;
                handshake.DecoderState = uploadResponse.Data.DecoderState;

                //--------------------------------------
                // Prompt to upload another file or not
                //--------------------------------------
                Console.WriteLine($"Would you like to upload an additional file? (y/n)");
                var sendAdditional = Console.ReadLine();
                if (sendAdditional != null && sendAdditional.Equals("n", StringComparison.InvariantCultureIgnoreCase))
                {
                    break;
                }
            }
        }
    }
}
