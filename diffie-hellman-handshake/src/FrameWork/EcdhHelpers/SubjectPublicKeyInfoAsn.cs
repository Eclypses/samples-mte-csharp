using PackageCSharpECDHFW.Asn1;
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace PackageCSharpECDHFW.EcdhHelpers
{
    //-----------------------------------------------------------------------------------
    // These methods are from .Net 6.0 runtime source code
    // They have been added so we can use the Handshake in .Net Framework 4.8 projects
    //-----------------------------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential)]
    internal partial struct SubjectPublicKeyInfoAsn
    {
        internal AlgorithmIdentifierAsn Algorithm;
        internal ReadOnlyMemory<byte> SubjectPublicKey;

        internal void Encode(AsnWriter writer)
        {
            Encode(writer, Asn1Tag.Sequence);
        }

        internal void Encode(AsnWriter writer, Asn1Tag tag)
        {
            writer.PushSequence(tag);

            Algorithm.Encode(writer);
            writer.WriteBitString(SubjectPublicKey.Span, 0);
            writer.PopSequence(tag);
        }

        internal static SubjectPublicKeyInfoAsn Decode(ReadOnlyMemory<byte> encoded, AsnEncodingRules ruleSet)
        {
            return Decode(Asn1Tag.Sequence, encoded, ruleSet);
        }

        internal static SubjectPublicKeyInfoAsn Decode(Asn1Tag expectedTag, ReadOnlyMemory<byte> encoded, AsnEncodingRules ruleSet)
        {
            try
            {
                AsnValueReader reader = new AsnValueReader(encoded.Span, ruleSet);

                DecodeCore(ref reader, expectedTag, encoded, out SubjectPublicKeyInfoAsn decoded);
                reader.ThrowIfNotEmpty();
                return decoded;
            }
            catch (AsnContentException e)
            {
                throw new CryptographicException("Cryptography_Der_Invalid_Encoding", e);
            }
        }

        internal static void Decode(ref AsnValueReader reader, ReadOnlyMemory<byte> rebind, out SubjectPublicKeyInfoAsn decoded)
        {
            Decode(ref reader, Asn1Tag.Sequence, rebind, out decoded);
        }

        internal static void Decode(ref AsnValueReader reader, Asn1Tag expectedTag, ReadOnlyMemory<byte> rebind, out SubjectPublicKeyInfoAsn decoded)
        {
            try
            {
                DecodeCore(ref reader, expectedTag, rebind, out decoded);
            }
            catch (AsnContentException e)
            {
                throw new CryptographicException(".Cryptography_Der_Invalid_Encoding", e);
            }
        }

        private static void DecodeCore(ref AsnValueReader reader, Asn1Tag expectedTag, ReadOnlyMemory<byte> rebind, out SubjectPublicKeyInfoAsn decoded)
        {
            decoded = default;
            AsnValueReader sequenceReader = reader.ReadSequence(expectedTag);
            ReadOnlySpan<byte> rebindSpan = rebind.Span;
            int offset;
            ReadOnlySpan<byte> tmpSpan;

            AlgorithmIdentifierAsn.Decode(ref sequenceReader, rebind, out decoded.Algorithm);

            if (sequenceReader.TryReadPrimitiveBitString(out _, out tmpSpan))
            {
                decoded.SubjectPublicKey = rebindSpan.Overlaps(tmpSpan, out offset) ? rebind.Slice(offset, tmpSpan.Length) : tmpSpan.ToArray();
            }
            else
            {
                decoded.SubjectPublicKey = sequenceReader.ReadBitString(out _);
            }


            sequenceReader.ThrowIfNotEmpty();
        }
    }
}
