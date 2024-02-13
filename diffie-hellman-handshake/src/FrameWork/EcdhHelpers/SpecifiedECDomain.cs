using PackageCSharpECDHFW.Asn1;
using System;

namespace PackageCSharpECDHFW.EcdhHelpers
{
    //-----------------------------------------------------------------------------------
    // These methods are from .Net 6.0 runtime source code
    // They have been added so we can use the Handshake in .Net Framework 4.8 projects
    //-----------------------------------------------------------------------------------
    internal partial struct SpecifiedECDomain
    {
        internal int Version;
        internal FieldID FieldID;
        internal CurveAsn Curve;
        internal ReadOnlyMemory<byte> Base;
        internal ReadOnlyMemory<byte> Order;
        internal ReadOnlyMemory<byte>? Cofactor;
        internal string? Hash;

        internal void Encode(AsnWriter writer)
        {
            Encode(writer, Asn1Tag.Sequence);
        }

        internal void Encode(AsnWriter writer, Asn1Tag tag)
        {
            writer.PushSequence(tag);

            writer.WriteInteger(Version);
            FieldID.Encode(writer);
            Curve.Encode(writer);
            writer.WriteOctetString(Base.Span);
            writer.WriteInteger(Order.Span);

            if (Cofactor.HasValue)
            {
                writer.WriteInteger(Cofactor.Value.Span);
            }


            if (Hash != null)
            {
                try
                {
                    writer.WriteObjectIdentifier(Hash);
                }
                catch (ArgumentException e)
                {
                    throw new Exception("Cryptography_Der_Invalid_Encoding", e);
                }
            }

            writer.PopSequence(tag);
        }

        internal static SpecifiedECDomain Decode(ReadOnlyMemory<byte> encoded, AsnEncodingRules ruleSet)
        {
            return Decode(Asn1Tag.Sequence, encoded, ruleSet);
        }

        internal static SpecifiedECDomain Decode(Asn1Tag expectedTag, ReadOnlyMemory<byte> encoded, AsnEncodingRules ruleSet)
        {
            try
            {
                AsnValueReader reader = new AsnValueReader(encoded.Span, ruleSet);

                DecodeCore(ref reader, expectedTag, encoded, out SpecifiedECDomain decoded);
                reader.ThrowIfNotEmpty();
                return decoded;
            }
            catch (AsnContentException e)
            {
                throw new Exception("Cryptography_Der_Invalid_Encoding", e);
            }
        }

        internal static void Decode(ref AsnValueReader reader, ReadOnlyMemory<byte> rebind, out SpecifiedECDomain decoded)
        {
            Decode(ref reader, Asn1Tag.Sequence, rebind, out decoded);
        }

        internal static void Decode(ref AsnValueReader reader, Asn1Tag expectedTag, ReadOnlyMemory<byte> rebind, out SpecifiedECDomain decoded)
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

        private static void DecodeCore(ref AsnValueReader reader, Asn1Tag expectedTag, ReadOnlyMemory<byte> rebind, out SpecifiedECDomain decoded)
        {
            decoded = default;
            AsnValueReader sequenceReader = reader.ReadSequence(expectedTag);
            ReadOnlySpan<byte> rebindSpan = rebind.Span;
            int offset;
            ReadOnlySpan<byte> tmpSpan;


            if (!sequenceReader.TryReadInt32(out decoded.Version))
            {
                sequenceReader.ThrowIfNotEmpty();
            }

            FieldID.Decode(ref sequenceReader, rebind, out decoded.FieldID);
            CurveAsn.Decode(ref sequenceReader, rebind, out decoded.Curve);

            if (sequenceReader.TryReadPrimitiveOctetString(out tmpSpan))
            {
                decoded.Base = rebindSpan.Overlaps(tmpSpan, out offset) ? rebind.Slice(offset, tmpSpan.Length) : tmpSpan.ToArray();
            }
            else
            {
                decoded.Base = sequenceReader.ReadOctetString();
            }

            tmpSpan = sequenceReader.ReadIntegerBytes();
            decoded.Order = rebindSpan.Overlaps(tmpSpan, out offset) ? rebind.Slice(offset, tmpSpan.Length) : tmpSpan.ToArray();

            if (sequenceReader.HasData && sequenceReader.PeekTag().HasSameClassAndValue(Asn1Tag.Integer))
            {
                tmpSpan = sequenceReader.ReadIntegerBytes();
                decoded.Cofactor = rebindSpan.Overlaps(tmpSpan, out offset) ? rebind.Slice(offset, tmpSpan.Length) : tmpSpan.ToArray();
            }


            if (sequenceReader.HasData && sequenceReader.PeekTag().HasSameClassAndValue(Asn1Tag.ObjectIdentifier))
            {
                decoded.Hash = sequenceReader.ReadObjectIdentifier();
            }


            sequenceReader.ThrowIfNotEmpty();
        }
    }
}
