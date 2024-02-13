using System;
using System.IO;
using System.Linq;
using System.Text;
using Eclypses.MTE;

namespace testChunker
{
    class Program
    {
        private static MteMkeEnc _mkeEncoder;
        private static MteMkeDec _mkeDecoder;
        private static MteStatus _encoderStatus;
        private static MteStatus _decoderStatus;
        private static string _entropy = "";
        private static readonly ulong _decoderNonce = 0;
        private static readonly ulong _encoderNonce = 0;
        private static readonly string _identifier = "demo";

        private static string _encodedFileName = "encodedText";
        private static string _decodedFileName = "decodedText"; 
        private static int _bufferSize = 1024;
        static void Main(string[] args)
        {
            string fPath = string.Empty;
            //--------------------------------------------------
            // Prompt for path till we have one and it is valid
            //--------------------------------------------------
            while (string.IsNullOrWhiteSpace(fPath) || !File.Exists(fPath))
            {
                //------------------------------------
                // Prompting message for file to copy
                //------------------------------------
                Console.WriteLine("Please enter path to file\n");

                fPath = Console.ReadLine();
                //-------------------------------
                // Check to make sure file exits
                //-------------------------------
                if (!File.Exists(fPath))
                {
                    Console.WriteLine($"File at the path '{fPath}' does not exist");
                    return;
                }
            }
            //----------------------------------------
            // Set the correct extension for the file
            //----------------------------------------
            _encodedFileName = $"{_encodedFileName}{Path.GetExtension(fPath)}";
            _decodedFileName = $"{_decodedFileName}{Path.GetExtension(fPath)}";

            //------------------------
            // Create default MTE MKE
            //------------------------
            _mkeEncoder = new MteMkeEnc();

            //---------------------------------------------------------------------
            // Check how long entropy we need, set default and prompt if we need it
            //---------------------------------------------------------------------
            int entropyMinBytes = _mkeEncoder.GetDrbgsEntropyMinBytes(_mkeEncoder.GetDrbg());
            _entropy = (entropyMinBytes > 0) ? new String('0', entropyMinBytes) : _entropy;

            //--------------------------------
            // Set MTE values for the Encoder
            //--------------------------------
            _mkeEncoder.SetEntropy(Encoding.UTF8.GetBytes(_entropy));
            _mkeEncoder.SetNonce(_encoderNonce);

            //-------------------------
            // Initialize MKE Encoder
            //-------------------------
            _encoderStatus = _mkeEncoder.Instantiate(_identifier);
            if (_encoderStatus != MteStatus.mte_status_success)
            {
                throw new ApplicationException($"Failed to initialize the MKE Encoder engine. Status: " +
                    $"{_mkeEncoder.GetStatusName(_encoderStatus)} / {_mkeEncoder.GetStatusDescription(_encoderStatus)}");
            }

            //----------------------------
            // Initialize chunking session
            //----------------------------
            _encoderStatus = _mkeEncoder.StartEncrypt();
            if (_encoderStatus != MteStatus.mte_status_success)
            {
                throw new Exception("Failed to start encode chunk. Status: "
                                    + _mkeEncoder.GetStatusName(_encoderStatus) + " / "
                                    + _mkeEncoder.GetStatusDescription(_encoderStatus));
            }
            //-------------------------------------------------------
            // Before we start we want to delete any files
            // that are already there so we always create new ones
            //-------------------------------------------------------
            if (File.Exists(_encodedFileName))
            {
                File.Delete(_encodedFileName);
            }

            //------------------------------------
            // Read file in and encode using MKE
            //------------------------------------
            using (FileStream stream = File.OpenRead(fPath))
            using (FileStream writeStream = File.Create(_encodedFileName))
            {
                BinaryReader reader = new BinaryReader(stream);
                BinaryWriter writer = new BinaryWriter(writeStream);

                //-----------------------------------
                // Create a buffer to hold the bytes 
                //-----------------------------------
                byte[] buffer = new Byte[_bufferSize];
                int bytesRead;

                //------------------------------------------
                // While the read method returns bytes
                // Keep writing them to the output stream
                //------------------------------------------
                while ((bytesRead = stream.Read(buffer, 0, _bufferSize)) > 0)
                {
                    //-------------------------------
                    // Encode the data in place
                    // Encoded data put back in buffer
                    //-------------------------------
                    MteStatus chunkStatus = _mkeEncoder.EncryptChunk(buffer, 0, bytesRead);
                    if (chunkStatus != MteStatus.mte_status_success)
                    {
                        throw new Exception("Failed to encode chunk. Status: "
                                            + _mkeEncoder.GetStatusName(chunkStatus) + " / "
                                            + _mkeEncoder.GetStatusDescription(chunkStatus));
                    }
                    writeStream.Write(buffer, 0, bytesRead);
                }
                //-----------------------------
                // Finish the chunking session
                //-----------------------------
                byte[] finalEncodedChunk = _mkeEncoder.FinishEncrypt(out MteStatus finishStatus);
                if (finishStatus != MteStatus.mte_status_success)
                {
                    throw new Exception("Failed to finish encode chunk. Status: "
                                        + _mkeEncoder.GetStatusName(finishStatus) + " / "
                                        + _mkeEncoder.GetStatusDescription(finishStatus));
                }
                //----------------------------------
                // Append the final data to the file
                //----------------------------------
                writeStream.Write(finalEncodedChunk, 0, finalEncodedChunk.Length);
            }
            Console.WriteLine($"Successfully encoded file: {Path.GetFileName(fPath)}");
            //-------------------------------
            // Delete decoded file if exists
            //-------------------------------
            if (File.Exists(_decodedFileName))
            {
                File.Delete(_decodedFileName);
            }

            _mkeDecoder = new MteMkeDec();

            //-----------------
            // Refill entropy
            //-----------------
            _entropy = (entropyMinBytes > 0) ? new String('0', entropyMinBytes) : _entropy;

            //---------------------------------
            // Set MTE values for the Decoder
            //---------------------------------
            _mkeDecoder.SetEntropy(Encoding.UTF8.GetBytes(_entropy));
            _mkeDecoder.SetNonce(_decoderNonce);

            //------------------------
            // Instantiate the Decoder
            //------------------------
            _decoderStatus = _mkeDecoder.Instantiate(_identifier);
            if (_decoderStatus != MteStatus.mte_status_success)
            {
                throw new ApplicationException($"Failed to initialize the MTE decoder engine. Status: " +
                    $"{_mkeEncoder.GetStatusName(_encoderStatus)} / {_mkeEncoder.GetStatusDescription(_encoderStatus)}");
            }

            //-----------------------------
            // Initialize chunking session
            //-----------------------------
            _decoderStatus = _mkeDecoder.StartDecrypt();
            if (_decoderStatus != MteStatus.mte_status_success)
            {
                throw new Exception("Failed to start decode chunk. Status: "
                                    + _mkeEncoder.GetStatusName(_decoderStatus) + " / "
                                    + _mkeEncoder.GetStatusDescription(_decoderStatus));
            }

            //-----------------------------------
            // Stream encoded file in to Decoder
            //-----------------------------------
            using (FileStream stream = File.OpenRead(_encodedFileName))
            using (FileStream writeStream = File.Create(_decodedFileName))
            {
                BinaryReader reader = new BinaryReader(stream);
                BinaryWriter writer = new BinaryWriter(writeStream);

                //------------------------------------
                // Create a buffer to hold the bytes 
                //------------------------------------
                byte[] buffer = new Byte[_bufferSize];
                int bytesRead;

                //-----------------------------------------
                // While the read method returns bytes
                // Keep writing them to the output stream
                //-----------------------------------------
                while ((bytesRead = stream.Read(buffer, 0, _bufferSize)) > 0)
                {
                    byte[] decodedData = new byte[0];
                    if (bytesRead == buffer.Length)
                    {
                        decodedData = _mkeDecoder.DecryptChunk(buffer);
                    }
                    else
                    {
                        //------------------------------------------
                        // Find out what the decoded length will be
                        //------------------------------------------
                        var cipherBlocks = _mkeDecoder.GetCiphersBlockBytes(_mkeDecoder.GetCipher());
                        int buffBytes = bytesRead - cipherBlocks;
                        //----------------------------------
                        // Allocate buffer for decoded data
                        //----------------------------------
                        decodedData = new byte[buffBytes];
                        int decryptError = _mkeDecoder.DecryptChunk(buffer, 0, bytesRead, decodedData, 0);
                        if (decryptError < 0)
                        {
                            throw new ApplicationException("Error decoding data.");
                        }
                    }                    
                    writeStream.Write(decodedData, 0, decodedData.Length);
                }
                //------------------------------
                // Finish the chunking session
                //------------------------------
                byte[] finalDecodedChunk = _mkeDecoder.FinishDecrypt(out MteStatus finishStatus);
                if (finishStatus != MteStatus.mte_status_success)
                {
                    throw new Exception("Failed to finish decode chunk. Status: "
                                        + _mkeEncoder.GetStatusName(finishStatus) + " / "
                                        + _mkeEncoder.GetStatusDescription(finishStatus));
                }
                //------------------------------------------------------------------------
                // Check if there is additional bytes if not initialize empty byte array
                //------------------------------------------------------------------------
                if (finalDecodedChunk.Length <= 0) { finalDecodedChunk = new byte[0]; }
                //-----------------------------------
                // Append the final data to the file
                //-----------------------------------
                writeStream.Write(finalDecodedChunk, 0, finalDecodedChunk.Length);
            }
            Console.WriteLine("Successfully decoded the encoded file");
        }
    }
}
