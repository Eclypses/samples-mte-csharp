using Eclypses.MTE;
using System;

namespace MteJailbreakTest
{
    class DecoderDevice
    {
        private MteStatus status;
        public MteStatus CallDecoderDevice(MteJail.Algo jailAlgorithm, string encodedInput, ulong nonce, string personal, out string decodedMessage)
        {
            //--------------------------------
            // default return string to empty
            //--------------------------------
            decodedMessage = string.Empty;

            //-----------------
            // Create Mte base
            //-----------------
            MteBase baseObj = new MteBase();
            if (!baseObj.InitLicense("YOUR_COMPANY", "YOUR_LICENSE"))
            {
                string company = Environment.GetEnvironmentVariable("MTE_COMPANY");
                string license = Environment.GetEnvironmentVariable("MTE_LICENSE");
                if (company == null || license == null ||
                    !baseObj.InitLicense(company, license))
                {
                    status = MteStatus.mte_status_license_error;
                    Console.Error.WriteLine("License initialization error ({0}): {1}",
                        baseObj.GetStatusName(status),
                        baseObj.GetStatusDescription(status));
                    return status;
                }
            }

            //------------------------
            // Create default decoder
            //------------------------
            MteDec decoder = new MteDec();

            //-------------------------------------------------------------------------
            // Create all-zero entropy for this demo. The nonce will also be set to 0.
            // This should never be done in real applications.
            //-------------------------------------------------------------------------
            int entropyBytes = baseObj.GetDrbgsEntropyMinBytes(decoder.GetDrbg());
            byte[] entropy = new byte[entropyBytes];

            //-------------------------
            // Instantiate the decoder.
            //-------------------------
            decoder.SetEntropy(entropy);
            //-------------------------------------
            // Set the device type and nonce seed. 
            // Use the jailbreak nonce callback.
            //-------------------------------------
            Cbs cb = new Cbs(decoder.GetInterop());
            cb.SetAlgo(jailAlgorithm);
            cb.SetNonceSeed(nonce);
            decoder.SetNonceCallback(cb);
            status = decoder.Instantiate(personal);
            if (status != MteStatus.mte_status_success)
            {
                Console.Error.WriteLine("Decoder instantiate error ({0}): {1}",
                    decoder.GetStatusName(status),
                    decoder.GetStatusDescription(status));
                return status;
            }

            //--------------------
            // Decode the message.
            //--------------------     
            decodedMessage = decoder.DecodeStrB64(encodedInput, out status);
            if (decoder.StatusIsError(status))
            {
                Console.Error.WriteLine("Decode error ({0}): {1}",
                    decoder.GetStatusName(status),
                    decoder.GetStatusDescription(status));
            }
            return status;
        }
    }
}
