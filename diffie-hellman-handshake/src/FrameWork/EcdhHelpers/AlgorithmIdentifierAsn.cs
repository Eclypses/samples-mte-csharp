using PackageCSharpECDHFW.Asn1;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace PackageCSharpECDHFW.EcdhHelpers
{
    //-----------------------------------------------------------------------------------
    // These methods are from .Net 6.0 runtime source code
    // They have been added so we can use the Handshake in .Net Framework 4.8 projects
    //-----------------------------------------------------------------------------------

    internal partial struct AlgorithmIdentifierAsn
    {
        internal static readonly ReadOnlyMemory<byte> ExplicitDerNull = new byte[] { 0x05, 0x00 };

        internal bool Equals(ref AlgorithmIdentifierAsn other)
        {
            if (Algorithm != other.Algorithm)
            {
                return false;
            }

            bool isNull = RepresentsNull(Parameters);
            bool isOtherNull = RepresentsNull(other.Parameters);

            if (isNull != isOtherNull)
            {
                return false;
            }

            if (isNull)
            {
                return true;
            }

            return Parameters!.Value.Span.SequenceEqual(other.Parameters!.Value.Span);
        }

        internal readonly bool HasNullEquivalentParameters()
        {
            return RepresentsNull(Parameters);
        }

        internal static bool RepresentsNull(ReadOnlyMemory<byte>? parameters)
        {
            if (parameters == null)
            {
                return true;
            }

            ReadOnlySpan<byte> span = parameters.Value.Span;

            if (span.Length != 2)
            {
                return false;
            }

            if (span[0] != 0x05)
            {
                return false;
            }

            return span[1] == 0;
        }

        internal string Algorithm;
        internal ReadOnlyMemory<byte>? Parameters;

        internal void Encode(AsnWriter writer)
        {
            Encode(writer, Asn1Tag.Sequence);
        }

        internal void Encode(AsnWriter writer, Asn1Tag tag)
        {
            writer.PushSequence(tag);

            try
            {
                writer.WriteObjectIdentifier(Algorithm);
            }
            catch (ArgumentException e)
            {
                throw new CryptographicException("Cryptography_Der_Invalid_Encoding", e);
            }

            if (Parameters.HasValue)
            {
                try
                {
                    writer.WriteEncodedValue(Parameters.Value.Span);
                }
                catch (ArgumentException e)
                {
                    throw new CryptographicException("Cryptography_Der_Invalid_Encoding", e);
                }
            }

            writer.PopSequence(tag);
        }

        internal static AlgorithmIdentifierAsn Decode(ReadOnlyMemory<byte> encoded, AsnEncodingRules ruleSet)
        {
            return Decode(Asn1Tag.Sequence, encoded, ruleSet);
        }

        internal static AlgorithmIdentifierAsn Decode(Asn1Tag expectedTag, ReadOnlyMemory<byte> encoded, AsnEncodingRules ruleSet)
        {
            try
            {
                AsnValueReader reader = new AsnValueReader(encoded.Span, ruleSet);

                DecodeCore(ref reader, expectedTag, encoded, out AlgorithmIdentifierAsn decoded);
                reader.ThrowIfNotEmpty();
                return decoded;
            }
            catch (AsnContentException e)
            {
                throw new CryptographicException("Cryptography_Der_Invalid_Encoding", e);
            }
        }

        internal static void Decode(ref AsnValueReader reader, ReadOnlyMemory<byte> rebind, out AlgorithmIdentifierAsn decoded)
        {
            Decode(ref reader, Asn1Tag.Sequence, rebind, out decoded);
        }

        internal static void Decode(ref AsnValueReader reader, Asn1Tag expectedTag, ReadOnlyMemory<byte> rebind, out AlgorithmIdentifierAsn decoded)
        {
            try
            {
                DecodeCore(ref reader, expectedTag, rebind, out decoded);
            }
            catch (AsnContentException e)
            {
                throw new CryptographicException("Cryptography_Der_Invalid_Encoding", e);
            }
        }

        private static void DecodeCore(ref AsnValueReader reader, Asn1Tag expectedTag, ReadOnlyMemory<byte> rebind, out AlgorithmIdentifierAsn decoded)
        {
            decoded = default;
            AsnValueReader sequenceReader = reader.ReadSequence(expectedTag);
            ReadOnlySpan<byte> rebindSpan = rebind.Span;
            int offset;
            ReadOnlySpan<byte> tmpSpan;

            decoded.Algorithm = sequenceReader.ReadObjectIdentifier();

            if (sequenceReader.HasData)
            {
                tmpSpan = sequenceReader.ReadEncodedValue();
                decoded.Parameters = rebindSpan.Overlaps(tmpSpan, out offset) ? rebind.Slice(offset, tmpSpan.Length) : tmpSpan.ToArray();
            }


            sequenceReader.ThrowIfNotEmpty();
        }

    }
}
