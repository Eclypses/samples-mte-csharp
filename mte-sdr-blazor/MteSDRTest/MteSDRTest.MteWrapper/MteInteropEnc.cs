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

// <auto-generated />
#pragma warning disable 1591

namespace Eclypses.MTE.Interop {
  /// <summary>
  /// Class MteInterop
  ///
  /// This class contains the interop functions for MTE.
  ///
  /// This partial class contains the interop functions for MteEnc.
  /// </summary>
  public partial class MteInterop {
    /// <summary>
    /// Interop functions.
    /// </summary>
    public MTE_ENC_STATE_BYTES mte_enc_state_bytes;
    public MTE_ENC_STATE_INIT mte_enc_state_init;
    public MTE_ENC_INSTANTIATE mte_enc_instantiate;
    public MTE_ENC_RESEED_COUNTER mte_enc_reseed_counter;
    public MTE_ENC_SAVE_BYTES mte_enc_save_bytes;
    public MTE_ENC_SAVE_BYTES_B64 mte_enc_save_bytes_b64;
    public MTE_ENC_STATE_SAVE mte_enc_state_save;
    public MTE_ENC_STATE_SAVE_B64 mte_enc_state_save_b64;
    public MTE_ENC_STATE_RESTORE mte_enc_state_restore;
    public MTE_ENC_STATE_RESTORE_B64 mte_enc_state_restore_b64;
    public MTE_ENC_BUFF_BYTES mte_enc_buff_bytes;
    public MTE_ENC_BUFF_BYTES_B64 mte_enc_buff_bytes_b64;
    public MTE_ENC_ENCODE_A mte_enc_encode_a;
    public MTE_ENC_ENCODE_P mte_enc_encode_p;
    public MTE_ENC_ENCODE_B64_A mte_enc_encode_b64_a;
    public MTE_ENC_ENCODE_B64_P mte_enc_encode_b64_p;
    public MTE_ENC_UNINSTANTIATE mte_enc_uninstantiate;

    /// <summary>
    /// Load MteEnc parts.
    /// </summary>
    partial void LoadEnc(ILoader l) {
      BindDelegate(l, "mte_wrap_enc_state_bytes", out mte_enc_state_bytes);
      BindDelegate(l, "mte_wrap_enc_state_init", out mte_enc_state_init);
      BindDelegate(l, "mte_wrap_enc_instantiate", out mte_enc_instantiate);
      BindDelegate(l, "mte_enc_reseed_counter", out mte_enc_reseed_counter);
      BindDelegate(l, "mte_wrap_enc_save_bytes", out mte_enc_save_bytes);
      BindDelegate(l, "mte_wrap_enc_save_bytes_b64",
                   out mte_enc_save_bytes_b64);
      BindDelegate(l, "mte_enc_state_save", out mte_enc_state_save);
      BindDelegate(l, "mte_wrap_enc_state_save_b64",
                   out mte_enc_state_save_b64);
      BindDelegate(l, "mte_enc_state_restore", out mte_enc_state_restore);
      BindDelegate(l, "mte_wrap_enc_state_restore_b64",
                   out mte_enc_state_restore_b64);
      BindDelegate(l, "mte_wrap_enc_buff_bytes", out mte_enc_buff_bytes);
      BindDelegate(l, "mte_wrap_enc_buff_bytes_b64",
                   out mte_enc_buff_bytes_b64);
      BindDelegate(l, "mte_wrap_enc_encode", out mte_enc_encode_a);
      BindDelegate(l, "mte_wrap_enc_encode", out mte_enc_encode_p);
      BindDelegate(l, "mte_wrap_enc_encode_b64", out mte_enc_encode_b64_a);
      BindDelegate(l, "mte_wrap_enc_encode_b64", out mte_enc_encode_b64_p);
      BindDelegate(l, "mte_enc_uninstantiate", out mte_enc_uninstantiate);
    }
  }
}
#pragma warning restore 1591

