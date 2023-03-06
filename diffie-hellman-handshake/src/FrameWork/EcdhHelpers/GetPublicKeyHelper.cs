using PackageCSharpECDHFW.Asn1;
using System;
using System.Security.Cryptography;

namespace PackageCSharpECDHFW.EcdhHelpers
{
    //-----------------------------------------------------------------------------------
    // These methods are from .Net 6.0 runtime source code
    // They have been added so we can use the Handshake in .Net Framework 4.8 projects
    //-----------------------------------------------------------------------------------
    public class GetPublicKeyHelper
    {
        #region TryExportSubjectPublicKeyInfo
        // This is the base method that we needed to call 
        // all other methods on in this class support this call

        /// <summary>Tries the export subject public key information.</summary>
        /// <param name="container">The container.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="bytesWritten">The bytes written.</param>
        /// <returns>
        ///   <c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool TryExportSubjectPublicKeyInfo(ECDiffieHellman container, Span<byte> destination, out int bytesWritten)
        {
            ECParameters ecParameters = container.ExportParameters(true);
            AsnWriter writer = WriteSubjectPublicKeyInfo(ecParameters);
            return writer.TryEncode(destination, out bytesWritten);
        } 
        #endregion

        private static void DetermineChar2Parameters(
            in ECParameters ecParameters,
            ref int m,
            ref int k1,
            ref int k2,
            ref int k3)
        {
            byte[] polynomial = ecParameters.Curve.Polynomial!;
            int lastIndex = polynomial.Length - 1;

            // The most significant byte needs a set bit, and the least significant bit must be set.
            if (polynomial[0] == 0 || (polynomial[lastIndex] & 1) != 1)
            {
                throw new CryptographicException("Cryptography_InvalidECCharacteristic2Curve");
            }

            for (int localBitIndex = 7; localBitIndex >= 0; localBitIndex--)
            {
                int test = 1 << localBitIndex;

                if ((polynomial[0] & test) == test)
                {
                    m = checked(8 * lastIndex + localBitIndex);
                }
            }

            // Find the other set bits. Since we've already found m and 0, there is either
            // one remaining (trinomial) or 3 (pentanomial).
            for (int inverseIndex = 0; inverseIndex < polynomial.Length; inverseIndex++)
            {
                int forwardIndex = lastIndex - inverseIndex;
                byte val = polynomial[forwardIndex];

                for (int localBitIndex = 0; localBitIndex < 8; localBitIndex++)
                {
                    int test = 1 << localBitIndex;

                    if ((val & test) == test)
                    {
                        int bitIndex = 8 * inverseIndex + localBitIndex;

                        if (bitIndex == 0)
                        {
                            // The bottom bit is always set, it's not considered a parameter.
                        }
                        else if (bitIndex == m)
                        {
                            break;
                        }
                        else if (k1 < 0)
                        {
                            k1 = bitIndex;
                        }
                        else if (k2 < 0)
                        {
                            k2 = bitIndex;
                        }
                        else if (k3 < 0)
                        {
                            k3 = bitIndex;
                        }
                        else
                        {
                            // More than pentanomial.
                            throw new CryptographicException("Cryptography_InvalidECCharacteristic2Curve");
                        }
                    }
                }
            }

            if (k3 > 0)
            {
                // Pentanomial
            }
            else if (k2 > 0)
            {
                // There is no quatranomial
                throw new CryptographicException("Cryptography_InvalidECCharacteristic2Curve");
            }
            else if (k1 > 0)
            {
                // Trinomial
            }
            else
            {
                // No smaller bases exist
                throw new CryptographicException("Cryptography_InvalidECCharacteristic2Curve");
            }
        }

        private static void WriteCurve(in ECCurve curve, AsnWriter writer)
        {
            writer.PushSequence();
            WriteFieldElement(curve.A!, writer);
            WriteFieldElement(curve.B!, writer);

            if (curve.Seed != null)
            {
                writer.WriteBitString(curve.Seed);
            }

            writer.PopSequence();
        }

        private static void WriteFieldElement(byte[] fieldElement, AsnWriter writer)
        {
            int start = 0;

            while (start < fieldElement.Length - 1 && fieldElement[start] == 0)
            {
                start++;
            }

            writer.WriteOctetString(fieldElement.AsSpan(start));
        }

        private static void WriteUncompressedBasePoint(in ECParameters ecParameters, AsnWriter writer)
        {
            int basePointLength = ecParameters.Curve.G.X!.Length * 2 + 1;

            // A NIST P-521 G will be at most 133 bytes (NIST 186-4 defines G.)
            // 256 should be plenty for all but very atypical uses.
            const int MaxStackAllocSize = 256;
            Span<byte> basePointBytes = stackalloc byte[MaxStackAllocSize];
            byte[]? rented = null;

            if (basePointLength > MaxStackAllocSize)
            {
                basePointBytes = rented = Helpers.Rent(basePointLength);
            }

            basePointBytes[0] = 0x04;
            ecParameters.Curve.G.X.CopyTo(basePointBytes.Slice(1));
            ecParameters.Curve.G.Y.CopyTo(basePointBytes.Slice(1 + ecParameters.Curve.G.X.Length));

            writer.WriteOctetString(basePointBytes.Slice(0, basePointLength));

            if (rented is not null)
            {
                // G contains public EC parameters that are not sensitive.
                Helpers.Return(rented, clearSize: 0);
            }
        }

        internal const string EcPrimeField = "1.2.840.10045.1.1";
        internal const string EcChar2Field = "1.2.840.10045.1.2";
        internal const string EcChar2TrinomialBasis = "1.2.840.10045.1.2.3.2";
        internal const string EcChar2PentanomialBasis = "1.2.840.10045.1.2.3.3";
        private static void WriteSpecifiedECDomain(ECParameters ecParameters, AsnWriter writer)
        {
            int m;
            int k1;
            int k2;
            int k3;
            m = k1 = k2 = k3 = -1;

            if (ecParameters.Curve.IsCharacteristic2)
            {
                DetermineChar2Parameters(ecParameters, ref m, ref k1, ref k2, ref k3);
            }

            // SpecifiedECDomain
            writer.PushSequence();
            {
                // version
                // We don't know if the seed (if present) is verifiably random (2).
                // We also don't know if the base point is verifiably random (3).
                // So just be version 1.
                writer.WriteInteger(1);

                // fieldId
                writer.PushSequence();
                {
                    if (ecParameters.Curve.IsPrime)
                    {
                        writer.WriteObjectIdentifier(EcPrimeField);
                        writer.WriteIntegerUnsigned(ecParameters.Curve.Prime);
                    }
                    else
                    {
                        //Debug.Assert(ecParameters.Curve.IsCharacteristic2);

                        // id
                        writer.WriteObjectIdentifier(EcChar2Field);

                        // Parameters (Characteristic-two)
                        writer.PushSequence();
                        {
                            // m
                            writer.WriteInteger(m);

                            if (k3 > 0)
                            {
                                writer.WriteObjectIdentifier(EcChar2PentanomialBasis);

                                writer.PushSequence();
                                {
                                    writer.WriteInteger(k1);
                                    writer.WriteInteger(k2);
                                    writer.WriteInteger(k3);

                                    writer.PopSequence();
                                }
                            }
                            else
                            {
                                //Debug.Assert(k2 < 0);
                                //Debug.Assert(k1 > 0);

                                writer.WriteObjectIdentifier(EcChar2TrinomialBasis);
                                writer.WriteInteger(k1);
                            }

                            writer.PopSequence();
                        }
                    }

                    writer.PopSequence();
                }

                // curve
                WriteCurve(ecParameters.Curve, writer);

                // base
                WriteUncompressedBasePoint(ecParameters, writer);

                // order
                writer.WriteIntegerUnsigned(ecParameters.Curve.Order);

                // cofactor
                if (ecParameters.Curve.Cofactor != null)
                {
                    writer.WriteIntegerUnsigned(ecParameters.Curve.Cofactor);
                }

                // hash is omitted.

                writer.PopSequence();
            }
        }
        private static void WriteEcParameters(ECParameters ecParameters, AsnWriter writer)
        {
            if (ecParameters.Curve.IsNamed)
            {
                Oid oid = ecParameters.Curve.Oid;

                // On Windows the FriendlyName is populated in places where the Value mightn't be.
                if (string.IsNullOrEmpty(oid.Value))
                {
                    //Debug.Assert(oid.FriendlyName != null);
                    oid = Oid.FromFriendlyName(oid.FriendlyName, OidGroup.All);
                }

                writer.WriteObjectIdentifier(oid.Value!);
            }
            else if (ecParameters.Curve.IsExplicit)
            {
                //Debug.Assert(ecParameters.Curve.IsPrime || ecParameters.Curve.IsCharacteristic2);
                WriteSpecifiedECDomain(ecParameters, writer);
            }
            else
            {
                throw new CryptographicException("Cryptography_CurveNotSupported");//, ecParameters.Curve.CurveType.ToString()));
            }
        }
        
        internal const string EcPublicKey = "1.2.840.10045.2.1";
        private static void WriteAlgorithmIdentifier(in ECParameters ecParameters, AsnWriter writer)
        {
            writer.PushSequence();

            writer.WriteObjectIdentifier(EcPublicKey);
            WriteEcParameters(ecParameters, writer);

            writer.PopSequence();
        }

        private static void WriteUncompressedPublicKey(in ECParameters ecParameters, AsnWriter writer)
        {
            int publicKeyLength = ecParameters.Q.X!.Length * 2 + 1;

            // A NIST P-521 Q will encode to 133 bytes: (521 + 7)/8 * 2 + 1.
            // 256 should be plenty for all but very atypical uses.
            const int MaxStackAllocSize = 256;
            Span<byte> publicKeyBytes = stackalloc byte[MaxStackAllocSize];
            byte[]? rented = null;

            if (publicKeyLength > MaxStackAllocSize)
            {
                publicKeyBytes = rented = Helpers.Rent(publicKeyLength);
            }

            publicKeyBytes[0] = 0x04;
            ecParameters.Q.X.CopyTo(publicKeyBytes.Slice(1));
            ecParameters.Q.Y.CopyTo(publicKeyBytes.Slice(1 + ecParameters.Q.X!.Length));

            writer.WriteBitString(publicKeyBytes.Slice(0, publicKeyLength));

            if (rented is not null)
            {
                // Q contains public EC parameters that are not sensitive.
                Helpers.Return(rented, clearSize: 0);
            }
        }
        internal static AsnWriter WriteSubjectPublicKeyInfo(ECParameters ecParameters)
        {
            ecParameters.Validate();

            // Since the public key format for EC keys is not ASN.1,
            // write the SPKI structure manually.

            AsnWriter writer = new AsnWriter(AsnEncodingRules.DER);

            // SubjectPublicKeyInfo
            writer.PushSequence();

            // algorithm
            WriteAlgorithmIdentifier(ecParameters, writer);

            // subjectPublicKey
            WriteUncompressedPublicKey(ecParameters, writer);

            writer.PopSequence();
            return writer;
        }

    }
}
