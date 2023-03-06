using Eclypses.MTE;
using System;

namespace MteJailbreakTest
{
    class EncoderDevice
    {
        private MteStatus status;
        public MteStatus CallEncoderDevice(MteJail.Algo jailAlgorithm, string input, ulong nonce, string personal, out string encodedMessage)
        {
            encodedMessage = string.Empty;
            //-------------------------------------------------------------------------
            // Initialize MTE license. If a license code is not required (e.g., trial
            // mode), this can be skipped. This demo attempts to load the license
            // info from the environment if required.
            //-------------------------------------------------------------------------
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

            //----------------------
            // Output original data.
            //----------------------
            Console.WriteLine("Original data: {0}", input);

            //----------------------------------
            // Create the encoder and decoder.
            //----------------------------------
            MteEnc encoder = new MteEnc();

            //-------------------------------------------------------------------------
            // Create all-zero entropy for this demo. The nonce will also be set to 0.
            // This should never be done in real applications.
            //-------------------------------------------------------------------------
            int entropyBytes = baseObj.GetDrbgsEntropyMinBytes(encoder.GetDrbg());
            byte[] entropy = new byte[entropyBytes];

            //---------------------------
            // Instantiate the encoder.
            //---------------------------
            encoder.SetEntropy(entropy);

            //--------------------
            // Jailbreak callback
            //--------------------
            Cbs cb = new Cbs(encoder.GetInterop());
            cb.SetAlgo(jailAlgorithm);
            cb.SetNonceSeed(nonce);
            encoder.SetNonceCallback(cb);

            status = encoder.Instantiate(personal);
            if (status != MteStatus.mte_status_success)
            {
                Console.Error.WriteLine("Encoder instantiate error ({0}): {1}",
                                        encoder.GetStatusName(status),
                                        encoder.GetStatusDescription(status));
                return status;
            }

            //-------------------
            // Encode the input.
            //-------------------
            encodedMessage = encoder.EncodeB64(input, out status);
            if (status != MteStatus.mte_status_success)
            {
                Console.Error.WriteLine("Encode error ({0}): {1}",
                                        encoder.GetStatusName(status),
                                        encoder.GetStatusDescription(status));
                return status;
            }

            //-----------------
            // return success
            //-----------------
            return status;
        }
    }
}
