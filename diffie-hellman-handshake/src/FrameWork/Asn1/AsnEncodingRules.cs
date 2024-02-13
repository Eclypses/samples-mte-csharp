// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace PackageCSharpECDHFW.Asn1
{
    //-----------------------------------------------------------------------------------
    // These methods are from .Net 6.0 runtime source code
    // They have been added so we can use the Handshake in .Net Framework 4.8 projects
    // This is specifically System.Formats.Asn1;
    //-----------------------------------------------------------------------------------

    /// <summary>
    ///   The encoding ruleset for an <see cref="AsnReader"/> or <see cref="AsnWriter"/>.
    /// </summary>
    // ITU-T-REC.X.680-201508 sec 4.
    public enum AsnEncodingRules
    {
        /// <summary>
        /// ITU-T X.690 Basic Encoding Rules
        /// </summary>
        BER,

        /// <summary>
        /// ITU-T X.690 Canonical Encoding Rules
        /// </summary>
        CER,

        /// <summary>
        /// ITU-T X.690 Distinguished Encoding Rules
        /// </summary>
        DER,
    }
}
