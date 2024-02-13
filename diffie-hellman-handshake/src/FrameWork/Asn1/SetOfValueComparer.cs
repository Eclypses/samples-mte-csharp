// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace PackageCSharpECDHFW.Asn1
{
    //-----------------------------------------------------------------------------------
    // These methods are from .Net 6.0 runtime source code
    // They have been added so we can use the Handshake in .Net Framework 4.8 projects
    // This is specifically System.Formats.Asn1;
    //-----------------------------------------------------------------------------------
    internal sealed class SetOfValueComparer : IComparer<ReadOnlyMemory<byte>>
    {
        internal static SetOfValueComparer Instance { get; } = new SetOfValueComparer();

        public int Compare(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y) =>
            Compare(x.Span, y.Span);

        internal static int Compare(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
        {
            int min = Math.Min(x.Length, y.Length);
            int diff;

            for (int i = 0; i < min; i++)
            {
                int xVal = x[i];
                byte yVal = y[i];
                diff = xVal - yVal;

                if (diff != 0)
                {
                    return diff;
                }
            }

            // The sorting rules (T-REC-X.690-201508 sec 11.6) say that the shorter one
            // counts as if it are padded with as many 0x00s on the right as required for
            // comparison.
            //
            // But, since a shorter definite value will have already had the length bytes
            // compared, it was already different.  And a shorter indefinite value will
            // have hit end-of-contents, making it already different.
            //
            // This is here because the spec says it should be, but no values are known
            // which will make diff != 0.
            diff = x.Length - y.Length;

            return diff;
        }
    }
}
