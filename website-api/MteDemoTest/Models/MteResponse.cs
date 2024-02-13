using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eclypses.MTE;

namespace MteDemoTest.Models
{
    /// <summary>
    /// The Generic Mte Response 
    /// </summary>
    public class MteResponse
    {
        public byte[] Message { get; set; }
        public MteStatus  Status { get; set; }
    }

    /// <summary>
    /// Mte Response for Encoder
    /// </summary>
    public class MteEncoderResponse : MteResponse
    {
        public MteMkeEnc encoder { get; set; }
    }

    /// <summary>
    /// Mte Response for Decoder
    /// </summary>
    public class MteDecoderResponse : MteResponse
    {
        public MteMkeDec decoder { get; set; }
    }
}
