using System;
using Eclypses.MTE;

namespace MteJailbreakTest
{
    class Program
    {
        static int Main()
        {
            // Status.
            MteStatus status;

            //----------
            // Input.
            //----------
            string input = "hello";

            //-------------------------
            // Personalization string.
            //-------------------------
            string personal = "demo";

            MteJail.Algo mteJailAlgorithm;

            MteBase baseObj = new MteBase();

            //-----------
            // set Nonce
            //-----------
            ulong nonce = 123;
            int timesToRun = 2;
            for (int i = 0; i < timesToRun; i++)
            {
                //-----------------------------
                // use none for first time
                // all other times use different
                //-----------------------------
                mteJailAlgorithm = i == 0 ? MteJail.Algo.aNone : MteJail.Algo.aIosX86_64Sim;

                //---------------------
                // Call Encoder device
                //---------------------
                EncoderDevice encoder = new EncoderDevice();
                MteStatus encoderStatus = encoder.CallEncoderDevice(mteJailAlgorithm, input, nonce, personal, out string encodedMessage);
                if (encoderStatus != MteStatus.mte_status_success)
                {
                    //------------
                    // error end
                    //------------
                    return (int)encoderStatus;
                }

                //---------------------
                // Display the message.
                //---------------------
                Console.WriteLine("Base64 message: {0}", encodedMessage);

                DecoderDevice decoder = new DecoderDevice();
                status = decoder.CallDecoderDevice(mteJailAlgorithm, encodedMessage, nonce, personal,
                    out string decodedMessage);

                if (status != MteStatus.mte_status_success)
                {
                    //------------------------------------------------
                    // If this specific error happens after first run
                    // we know the encoder device has been jail broken
                    //------------------------------------------------
                    if (status == MteStatus.mte_status_token_does_not_exist && i > 0)
                    {
                        Console.Error.WriteLine("Paired device has been compromised, possible jail broken device.");
                        return -1;
                    }
                    Console.Error.WriteLine("Decode warning ({0}): {1}",
                        baseObj.GetStatusName(status),
                        baseObj.GetStatusDescription(status));
                }
                //--------------------------
                // Output the decoded data.
                //--------------------------
                Console.WriteLine("Decoded data: {0}", decodedMessage);

                //-----------------------------------------------------
                // Compare the decoded data against the original data.
                //-----------------------------------------------------
                if (decodedMessage == input)
                {
                    Console.WriteLine("The original data and decoded data match.");
                }
                else
                {
                    Console.WriteLine("The original data and decoded data DO NOT match.");
                    return -1;
                }

            }

            Console.WriteLine("Complete, press enter to end...");
            Console.ReadLine();
            //-----------
            // Success.
            //-----------
            return 0;
        }
    }
}
