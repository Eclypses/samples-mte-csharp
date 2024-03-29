﻿using PackageCSharpECDHFW.Asn1;
using System;

namespace PackageCSharpECDHFW.EcdhHelpers
{
    //-----------------------------------------------------------------------------------
    // These methods are from .Net 6.0 runtime source code
    // They have been added so we can use the Handshake in .Net Framework 4.8 projects
    //-----------------------------------------------------------------------------------
    internal partial struct FieldID
    {
        internal string FieldType;
        internal ReadOnlyMemory<byte> Parameters;

        internal void Encode(AsnWriter writer)
        {
            Encode(writer, Asn1Tag.Sequence);
        }

        internal void Encode(AsnWriter writer, Asn1Tag tag)
        {
            writer.PushSequence(tag);

            try
            {
                writer.WriteObjectIdentifier(FieldType);
            }
            catch (ArgumentException e)
            {
                throw new Exception("Cryptography_Der_Invalid_Encoding", e);
            }
            try
            {
                writer.WriteEncodedValue(Parameters.Span);
            }
            catch (ArgumentException e)
            {
                throw new Exception("Cryptography_Der_Invalid_Encoding", e);
            }
            writer.PopSequence(tag);
        }

        internal static FieldID Decode(ReadOnlyMemory<byte> encoded, AsnEncodingRules ruleSet)
        {
            return Decode(Asn1Tag.Sequence, encoded, ruleSet);
        }

        internal static FieldID Decode(Asn1Tag expectedTag, ReadOnlyMemory<byte> encoded, AsnEncodingRules ruleSet)
        {
            try
            {
                AsnValueReader reader = new AsnValueReader(encoded.Span, ruleSet);

                DecodeCore(ref reader, expectedTag, encoded, out FieldID decoded);
                reader.ThrowIfNotEmpty();
                return decoded;
            }
            catch (AsnContentException e)
            {
                throw new Exception("Cryptography_Der_Invalid_Encoding", e);
            }
        }

        internal static void Decode(ref AsnValueReader reader, ReadOnlyMemory<byte> rebind, out FieldID decoded)
        {
            Decode(ref reader, Asn1Tag.Sequence, rebind, out decoded);
        }

        internal static void Decode(ref AsnValueReader reader, Asn1Tag expectedTag, ReadOnlyMemory<byte> rebind, out FieldID decoded)
        {
            try
            {
                DecodeCore(ref reader, expectedTag, rebind, out decoded);
            }
            catch (AsnContentException e)
            {
                throw new Exception("Cryptography_Der_Invalid_Encoding", e);
            }
        }

        private static void DecodeCore(ref AsnValueReader reader, Asn1Tag expectedTag, ReadOnlyMemory<byte> rebind, out FieldID decoded)
        {
            decoded = default;
            AsnValueReader sequenceReader = reader.ReadSequence(expectedTag);
            ReadOnlySpan<byte> rebindSpan = rebind.Span;
            int offset;
            ReadOnlySpan<byte> tmpSpan;

            decoded.FieldType = sequenceReader.ReadObjectIdentifier();
            tmpSpan = sequenceReader.ReadEncodedValue();
            decoded.Parameters = rebindSpan.Overlaps(tmpSpan, out offset) ? rebind.Slice(offset, tmpSpan.Length) : tmpSpan.ToArray();

            sequenceReader.ThrowIfNotEmpty();
        }
    }
}
