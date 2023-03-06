// THIS SOFTWARE MAY NOT BE USED FOR PRODUCTION. Otherwise,
// The MIT License (MIT)
//
// Copyright (c) Eclypses, Inc.
//
// All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using Eclypses.MTE;
using System;

namespace MTE {
  class demoCsSeq {
    static int Main() {
      // Status.
      MteStatus status;

      // Inputs.
      string[] inputs = {
        "message 0",
        "message 1",
        "message 2",
        "message 3"
      };

      // Personalization string.
      string personal = "demo";

      // Initialize MTE license. If a license code is not required (e.g., trial
      // mode), this can be skipped. This demo attempts to load the license
      // info from the environment if required.
      MteBase baseObj = new MteBase();
      if (!baseObj.InitLicense("YOUR_COMPANY", "YOUR_LICENSE")) {
        string company = Environment.GetEnvironmentVariable("MTE_COMPANY");
        string license = Environment.GetEnvironmentVariable("MTE_LICENSE");
        if (company == null || license == null ||
            !baseObj.InitLicense(company, license)) {
          status = MteStatus.mte_status_license_error;
          Console.Error.WriteLine("License init error ({0}): {1}",
                                  baseObj.GetStatusName(status),
                                  baseObj.GetStatusDescription(status));
          return (int)status;
        }
      }

      // Create the encoder.
      MteEnc encoder = new MteEnc();

      // Create all-zero entropy for this demo. The nonce will also be set to 0.
      // This should never be done in real applications.
      int entropyBytes = baseObj.GetDrbgsEntropyMinBytes(encoder.GetDrbg());
      byte[] entropy = new byte[entropyBytes];

      // Instantiate the encoder.
      encoder.SetEntropy(entropy);
      encoder.SetNonce(0);
      status = encoder.Instantiate(personal);
      if (status != MteStatus.mte_status_success) {
        Console.Error.WriteLine("Encoder instantiate error ({0}): {1}",
                                encoder.GetStatusName(status),
                                encoder.GetStatusDescription(status));
        return (int)status;
      }

      // Encode the inputs.
      string[] encodings = new string[inputs.Length];
      for (int i = 0; i < inputs.Length; ++i) {
        encodings[i] = encoder.EncodeB64(inputs[i], out status);
        if (status != MteStatus.mte_status_success) {
          Console.Error.WriteLine("Encode error ({0}): {1}",
                                  encoder.GetStatusName(status),
                                  encoder.GetStatusDescription(status));
          return (int)status;
        }
        Console.WriteLine("Encode #{0}: {1} -> {2}",
                          i,
                          inputs[i],
                          encodings[i]);
      }

      // Create decoders with different sequence windows.
      MteDec decoderV = new MteDec(0, 0);
      MteDec decoderF = new MteDec(0, 2);
      MteDec decoderA = new MteDec(0, -2);

      // Instantiate the decoders.
      decoderV.SetEntropy(entropy);
      decoderV.SetNonce(0);
      status = decoderV.Instantiate(personal);
      if (status == MteStatus.mte_status_success) {
        decoderF.SetEntropy(entropy);
        decoderF.SetNonce(0);
        status = decoderF.Instantiate(personal);
        if (status == MteStatus.mte_status_success) {
          decoderA.SetEntropy(entropy);
          decoderA.SetNonce(0);
          status = decoderA.Instantiate(personal);
        }
      }
      if (status != MteStatus.mte_status_success) {
        Console.Error.WriteLine("Decoder instantiate error ({0}): {1}",
                                decoderV.GetStatusName(status),
                                decoderV.GetStatusDescription(status));
        return (int)status;
      }

      // Save the async decoder state.
      byte[] dsaved = decoderA.SaveState();

      // String to decode to.
      string decoded;

      // Create the corrupt version of message #2.
      char first = encodings[2][0];
      ++first;
      string corrupt =
        encodings[2].Substring(1).Insert(0, new string(first, 1));

      // Decode in verification-only mode.
      Console.WriteLine("\nVerification-only mode (sequence window = 0):");
      decoded = decoderV.DecodeStrB64(encodings[0], out status);
      Console.WriteLine("Decode #0: {0}, {1}",
                        decoderV.GetStatusName(status),
                        decoded);
      decoded = decoderV.DecodeStrB64(encodings[0], out status);
      Console.WriteLine("Decode #0: {0}, {1}",
                        decoderV.GetStatusName(status),
                        decoded);
      decoded = decoderV.DecodeStrB64(encodings[2], out status);
      Console.WriteLine("Decode #2: {0}, {1}",
                        decoderV.GetStatusName(status),
                        decoded);
      decoded = decoderV.DecodeStrB64(encodings[1], out status);
      Console.WriteLine("Decode #1: {0}, {1}",
                        decoderV.GetStatusName(status),
                        decoded);
      decoded = decoderV.DecodeStrB64(encodings[2], out status);
      Console.WriteLine("Decode #2: {0}, {1}",
                        decoderV.GetStatusName(status),
                        decoded);
      decoded = decoderV.DecodeStrB64(encodings[3], out status);
      Console.WriteLine("Decode #3: {0}, {1}",
                        decoderV.GetStatusName(status),
                        decoded);

      // Decode in forward-only mode.
      Console.WriteLine("\nForward-only mode (sequence window = 2):");
      decoded = decoderF.DecodeStrB64(encodings[0], out status);
      Console.WriteLine("Decode #0: {0}, {1}",
                        decoderF.GetStatusName(status),
                        decoded);
      decoded = decoderF.DecodeStrB64(encodings[0], out status);
      Console.WriteLine("Decode #0: {0}, {1}",
                        decoderF.GetStatusName(status),
                        decoded);
      decoded = decoderF.DecodeStrB64(corrupt, out status);
      Console.WriteLine("Corrupt #2: {0}, {1}",
                        decoderF.GetStatusName(status),
                        decoded);
      decoded = decoderF.DecodeStrB64(encodings[2], out status);
      Console.WriteLine("Decode #2: {0}, {1}",
                        decoderF.GetStatusName(status),
                        decoded);
      decoded = decoderF.DecodeStrB64(encodings[1], out status);
      Console.WriteLine("Decode #1: {0}, {1}",
                        decoderF.GetStatusName(status),
                        decoded);
      decoded = decoderF.DecodeStrB64(encodings[2], out status);
      Console.WriteLine("Decode #2: {0}, {1}",
                        decoderF.GetStatusName(status),
                        decoded);
      decoded = decoderF.DecodeStrB64(encodings[3], out status);
      Console.WriteLine("Decode #3: {0}, {1}",
                        decoderF.GetStatusName(status),
                        decoded);

      // Decode in async mode.
      Console.WriteLine("\nAsync mode (sequence window = -2):");
      decoded = decoderA.DecodeStrB64(encodings[0], out status);
      Console.WriteLine("Decode #0: {0}, {1}",
                        decoderA.GetStatusName(status),
                        decoded);
      decoded = decoderA.DecodeStrB64(encodings[0], out status);
      Console.WriteLine("Decode #0: {0}, {1}",
                        decoderA.GetStatusName(status),
                        decoded);
      decoded = decoderA.DecodeStrB64(corrupt, out status);
      Console.WriteLine("Corrupt #2: {0}, {1}",
                        decoderA.GetStatusName(status),
                        decoded);
      decoded = decoderA.DecodeStrB64(encodings[2], out status);
      Console.WriteLine("Decode #2: {0}, {1}",
                        decoderA.GetStatusName(status),
                        decoded);
      decoded = decoderA.DecodeStrB64(encodings[2], out status);
      Console.WriteLine("Decode #2: {0}, {1}",
                        decoderA.GetStatusName(status),
                        decoded);
      decoded = decoderA.DecodeStrB64(encodings[1], out status);
      Console.WriteLine("Decode #1: {0}, {1}",
                        decoderA.GetStatusName(status),
                        decoded);
      decoded = decoderA.DecodeStrB64(encodings[2], out status);
      Console.WriteLine("Decode #2: {0}, {1}",
                        decoderA.GetStatusName(status),
                        decoded);
      decoded = decoderA.DecodeStrB64(encodings[3], out status);
      Console.WriteLine("Decode #3: {0}, {1}",
                        decoderA.GetStatusName(status),
                        decoded);

      // Restore and decode again in a different order.
      decoderA.RestoreState(dsaved);
      Console.WriteLine("\nAsync mode (sequence window = -2):");
      decoded = decoderA.DecodeStrB64(encodings[3], out status);
      Console.WriteLine("Decode #3: {0}, {1}",
                        decoderA.GetStatusName(status),
                        decoded);
      decoded = decoderA.DecodeStrB64(encodings[0], out status);
      Console.WriteLine("Decode #0: {0}, {1}",
                        decoderA.GetStatusName(status),
                        decoded);
      decoded = decoderA.DecodeStrB64(encodings[2], out status);
      Console.WriteLine("Decode #2: {0}, {1}",
                        decoderA.GetStatusName(status),
                        decoded);
      decoded = decoderA.DecodeStrB64(encodings[1], out status);
      Console.WriteLine("Decode #1: {0}, {1}",
                        decoderA.GetStatusName(status),
                        decoded);

      // Success.
      return 0;
    }
  }
}

