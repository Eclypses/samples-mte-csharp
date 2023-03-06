// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.Serialization;

namespace PackageCSharpECDHFW.Asn1
{
    //-----------------------------------------------------------------------------------
    // These methods are from .Net 6.0 runtime source code
    // They have been added so we can use the Handshake in .Net Framework 4.8 projects
    // This is specifically System.Formats.Asn1;
    //-----------------------------------------------------------------------------------
    [Serializable]
    public class AsnContentException : Exception
    {
        public AsnContentException()
            : base("ContentException_DefaultMessage")
        {
        }

        public AsnContentException(string? message)
            : base(message ?? "ContentException_DefaultMessage")
        {
        }

        public AsnContentException(string? message, Exception? inner)
            : base(message ?? "ContentException_DefaultMessage", inner)
        {
        }

        protected AsnContentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
