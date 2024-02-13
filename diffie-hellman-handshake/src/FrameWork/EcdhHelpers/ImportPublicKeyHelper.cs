using PackageCSharpECDHFW.Asn1;
using System;
using System.Collections;
using System.Security.Cryptography;

namespace PackageCSharpECDHFW.EcdhHelpers
{
    public class ImportPublicKeyHelper
    {
        //-----------------------------------------------------------------------------------
        // These methods are from .Net 6.0 runtime source code
        // They have been added so we can use the Handshake in .Net Framework 4.8 projects
        //-----------------------------------------------------------------------------------

        #region ImportSubjectPublicKeyInfo
        /// <summary>
        /// Imports the public key from an X.509 SubjectPublicKeyInfo structure after decryption,
        /// replacing the keys for this object
        /// </summary>
        /// <param name="container">The ECDiffieHellman container</param>
        /// <param name="source">The bytes of an X.509 SubjectPublicKeyInfo structure in the ASN.1-DER encoding.</param>
        /// <param name="bytesRead">
        /// When this method returns, contains a value that indicates the number
        /// of bytes read from <paramref name="source" />. This parameter is treated as uninitialized.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// A derived class has not provided an implementation for <see cref="ImportParameters" />.
        /// </exception>
        /// <exception cref="CryptographicException">
        /// <p>
        ///   The contents of <paramref name="source" /> do not represent an
        ///   ASN.1-DER-encoded X.509 SubjectPublicKeyInfo structure.
        /// </p>
        /// <p>-or-</p>
        /// <p>
        ///   The contents of <paramref name="source" /> indicate the key is for an algorithm
        /// other than the algorithm represented by this instance.
        /// </p>
        /// <p>-or-</p>
        /// <p>
        ///   The contents of <paramref name="source" /> represent the key in a format that is not supported.
        /// </p>
        /// <p>-or-</p>
        /// <p>The algorithm-specific key import failed.</p>
        /// </exception>
        /// <remarks>
        /// This method only supports the binary (DER) encoding of SubjectPublicKeyInfo.
        /// If the value is Base64-encoded, the caller must Base64-decode the contents before calling this method.
        /// If this value is PEM-encoded, <see cref="ImportFromPem" /> should be used.
        /// </remarks>
        public static void ImportSubjectPublicKeyInfo(ECDiffieHellman container,
            ReadOnlySpan<byte> source,
            out int bytesRead)
        {
            KeyFormatHelper.ReadSubjectPublicKeyInfo<ECParameters>(
                s_validOids,
                source,
                FromECPublicKey,
                out int localRead,
                out ECParameters key);

            container.ImportParameters(key);
            bytesRead = localRead;
        }
        internal const string EcPublicKey = "1.2.840.10045.2.1";
        private static readonly string[] s_validOids =
        {
            EcPublicKey,
            // ECDH and ECMQV are not valid in this context.
        }; 
        #endregion

        #region FromECPublicKey
        /// <summary>Froms the ec public key.</summary>
        /// <param name="key">The key.</param>
        /// <param name="algId">The alg identifier.</param>
        /// <param name="ret">The ret.</param>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Cryptography_Der_Invalid_Encoding</exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Cryptography_NotValidPublicOrPrivateKey</exception>
        internal static void FromECPublicKey(
            ReadOnlyMemory<byte> key,
            in AlgorithmIdentifierAsn algId,
            out ECParameters ret)
        {
            if (algId.Parameters == null)
            {
                throw new CryptographicException("Cryptography_Der_Invalid_Encoding");
            }

            ReadOnlySpan<byte> publicKeyBytes = key.Span;

            if (publicKeyBytes.Length == 0)
            {
                throw new CryptographicException("Cryptography_Der_Invalid_Encoding");
            }

            // Implementation limitation.
            // 04 (Uncompressed ECPoint) is almost always used.
            if (publicKeyBytes[0] != 0x04)
            {
                throw new CryptographicException("Cryptography_NotValidPublicOrPrivateKey");
            }

            // https://www.secg.org/sec1-v2.pdf, 2.3.4, #3 (M has length 2 * CEIL(log2(q)/8) + 1)
            if ((publicKeyBytes.Length & 0x01) != 1)
            {
                throw new CryptographicException("Cryptography_Der_Invalid_Encoding");
            }

            int fieldWidth = publicKeyBytes.Length / 2;

            ECDomainParameters domainParameters = ECDomainParameters.Decode(
                algId.Parameters.Value,
                AsnEncodingRules.DER);

            ret = new ECParameters
            {
                Curve = GetCurve(domainParameters),
                Q =
                {
                    X = publicKeyBytes.Slice(1, fieldWidth).ToArray(),
                    Y = publicKeyBytes.Slice(1 + fieldWidth).ToArray(),
                },
            };

            ret.Validate();
        }

        #region GetCurve
        // Elliptic Curve curve identifiers
        private static volatile Oid? _secp256R1Oid;
        private static volatile Oid? _secp384R1Oid;
        private static volatile Oid? _secp521R1Oid;
        internal const string Secp256R1 = "1.2.840.10045.3.1.7";
        internal const string Secp384R1 = "1.3.132.0.34";
        internal const string Secp521R1 = "1.3.132.0.35";
        internal static Oid Secp256R1Oid => _secp256R1Oid ??= new Oid(Secp256R1, nameof(ECCurve.NamedCurves.nistP256));
        internal static Oid Secp384R1Oid => _secp384R1Oid ??= new Oid(Secp384R1, nameof(ECCurve.NamedCurves.nistP384));
        internal static Oid Secp521R1Oid => _secp521R1Oid ??= new Oid(Secp521R1, nameof(ECCurve.NamedCurves.nistP521));


        /// <summary>Gets the curve.</summary>
        /// <param name="domainParameters">The domain parameters.</param>
        /// <returns>ECCurve.</returns>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Cryptography_ECC_NamedCurvesOnly</exception>
        private static ECCurve GetCurve(ECDomainParameters domainParameters)
        {
            if (domainParameters.Specified.HasValue)
            {
                return GetSpecifiedECCurve(domainParameters.Specified.Value);
            }

            if (domainParameters.Named == null)
            {
                throw new CryptographicException("Cryptography_ECC_NamedCurvesOnly");
            }

            var curveOid = domainParameters.Named switch
            {
                Secp256R1 => Secp256R1Oid,
                Secp384R1 => Secp384R1Oid,
                Secp521R1 => Secp521R1Oid,
                _ => new Oid(domainParameters.Named, null)
            };

            return ECCurve.CreateFromOid(curveOid);
        }
        #endregion

        #region GetSpecifiedECCurve
        /// <summary>Gets the specified ec curve.</summary>
        /// <param name="specifiedParameters">The specified parameters.</param>
        /// <returns>ECCurve.</returns>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Cryptography_Der_Invalid_Encoding</exception>
        private static ECCurve GetSpecifiedECCurve(SpecifiedECDomain specifiedParameters)
        {
            try
            {
                return GetSpecifiedECCurveCore(specifiedParameters);
            }
            catch (AsnContentException e)
            {
                throw new CryptographicException("Cryptography_Der_Invalid_Encoding", e);
            }
        }
        #endregion

        #region GetSpecifiedECCurveCore
        internal const string EcPrimeField = "1.2.840.10045.1.1";
        internal const string EcChar2Field = "1.2.840.10045.1.2";
        internal const string EcChar2TrinomialBasis = "1.2.840.10045.1.2.3.2";
        internal const string EcChar2PentanomialBasis = "1.2.840.10045.1.2.3.3";
        private const int MaxFieldBitSize = 661;
        /// <summary>Gets the specified ec curve core.</summary>
        /// <param name="specifiedParameters">The specified parameters.</param>
        /// <returns>ECCurve.</returns>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Cryptography_Der_Invalid_Encoding</exception>
        private static ECCurve GetSpecifiedECCurveCore(SpecifiedECDomain specifiedParameters)
        {
            // sec1-v2 C.3:
            //
            // Versions 1, 2, and 3 are defined.
            // 1 is just data, 2 and 3 mean that a seed is required (with different reasons for why,
            // but they're human-reasons, not technical ones).
            if (specifiedParameters.Version < 1 || specifiedParameters.Version > 3)
            {
                throw new CryptographicException("Cryptography_Der_Invalid_Encoding");
            }

            if (specifiedParameters.Version > 1 && !specifiedParameters.Curve.Seed.HasValue)
            {
                throw new CryptographicException("Cryptography_Der_Invalid_Encoding");
            }

            byte[] primeOrPoly;
            bool prime;

            switch (specifiedParameters.FieldID.FieldType)
            {
                case EcPrimeField:
                    prime = true;
                    AsnReader primeReader = new AsnReader(specifiedParameters.FieldID.Parameters, AsnEncodingRules.BER);
                    ReadOnlySpan<byte> primeValue = primeReader.ReadIntegerBytes().Span;
                    primeReader.ThrowIfNotEmpty();

                    if (primeValue[0] == 0)
                    {
                        primeValue = primeValue.Slice(1);
                    }

                    if (primeValue.Length > (MaxFieldBitSize / 8))
                    {
                        throw new CryptographicException("Cryptography_Der_Invalid_Encoding");
                    }

                    primeOrPoly = primeValue.ToArray();
                    break;
                case EcChar2Field:
                    prime = false;
                    AsnReader char2Reader = new AsnReader(specifiedParameters.FieldID.Parameters, AsnEncodingRules.BER);
                    AsnReader innerReader = char2Reader.ReadSequence();
                    char2Reader.ThrowIfNotEmpty();

                    // Characteristic-two ::= SEQUENCE
                    // {
                    //     m INTEGER, -- Field size
                    //     basis CHARACTERISTIC-TWO.&id({BasisTypes}),
                    //     parameters CHARACTERISTIC-TWO.&Type({BasisTypes}{@basis})
                    // }

                    if (!innerReader.TryReadInt32(out int m) || m > MaxFieldBitSize || m < 0)
                    {
                        throw new CryptographicException("Cryptography_Der_Invalid_Encoding");
                    }

                    int k1;
                    int k2 = -1;
                    int k3 = -1;

                    switch (innerReader.ReadObjectIdentifier())
                    {
                        case EcChar2TrinomialBasis:
                            // Trinomial ::= INTEGER
                            if (!innerReader.TryReadInt32(out k1) || k1 >= m || k1 < 1)
                            {
                                throw new CryptographicException("Cryptography_Der_Invalid_Encoding");
                            }

                            break;
                        case EcChar2PentanomialBasis:
                            // Pentanomial ::= SEQUENCE
                            // {
                            //     k1 INTEGER, -- k1 > 0
                            //     k2 INTEGER, -- k2 > k1
                            //     k3 INTEGER -- k3 > k2
                            // }
                            AsnReader pentanomialReader = innerReader.ReadSequence();

                            if (!pentanomialReader.TryReadInt32(out k1) ||
                                !pentanomialReader.TryReadInt32(out k2) ||
                                !pentanomialReader.TryReadInt32(out k3) ||
                                k1 < 1 ||
                                k2 <= k1 ||
                                k3 <= k2 ||
                                k3 >= m)
                            {
                                throw new CryptographicException("Cryptography_Der_Invalid_Encoding");
                            }

                            pentanomialReader.ThrowIfNotEmpty();

                            break;
                        default:
                            throw new CryptographicException("Cryptography_Der_Invalid_Encoding");
                    }

                    innerReader.ThrowIfNotEmpty();

                    BitArray poly = new BitArray(m + 1);
                    poly.Set(m, true);
                    poly.Set(k1, true);
                    poly.Set(0, true);

                    if (k2 > 0)
                    {
                        poly.Set(k2, true);
                        poly.Set(k3, true);
                    }

                    primeOrPoly = new byte[(m + 7) / 8];
                    poly.CopyTo(primeOrPoly, 0);
                    Array.Reverse(primeOrPoly);
                    break;
                default:
                    throw new CryptographicException("Cryptography_Der_Invalid_Encoding");
            }

            ECCurve curve;

            if (prime)
            {
                curve = new ECCurve
                {
                    CurveType = ECCurve.ECCurveType.PrimeShortWeierstrass,
                    Prime = primeOrPoly,
                };
            }
            else
            {
                curve = new ECCurve
                {
                    CurveType = ECCurve.ECCurveType.Characteristic2,
                    Polynomial = primeOrPoly,
                };
            }

            curve.A = ToUnsignedIntegerBytes(specifiedParameters.Curve.A, primeOrPoly.Length);
            curve.B = ToUnsignedIntegerBytes(specifiedParameters.Curve.B, primeOrPoly.Length);
            curve.Order = ToUnsignedIntegerBytes(specifiedParameters.Order, primeOrPoly.Length);

            ReadOnlySpan<byte> baseSpan = specifiedParameters.Base.Span;

            // We only understand the uncompressed point encoding, but that's almost always what's used.
            if (baseSpan[0] != 0x04 || baseSpan.Length != 2 * primeOrPoly.Length + 1)
            {
                throw new CryptographicException("Cryptography_Der_Invalid_Encoding");
            }

            curve.G.X = baseSpan.Slice(1, primeOrPoly.Length).ToArray();
            curve.G.Y = baseSpan.Slice(1 + primeOrPoly.Length).ToArray();

            if (specifiedParameters.Cofactor.HasValue)
            {
                curve.Cofactor = ToUnsignedIntegerBytes(specifiedParameters.Cofactor.Value);
            }

            return curve;
        }
        #endregion

        #region ToUnsignedIntegerBytes
        /// <summary>Converts to unsignedintegerbytes.</summary>
        /// <param name="memory">The memory.</param>
        /// <param name="length">The length.</param>
        /// <returns>System.Byte[].</returns>
        /// <exception cref="System.Security.Cryptography.CryptographicException">Cryptography_Der_Invalid_Encoding</exception>
        internal static byte[] ToUnsignedIntegerBytes(ReadOnlyMemory<byte> memory, int length)
        {
            if (memory.Length == length)
            {
                return memory.ToArray();
            }

            ReadOnlySpan<byte> span = memory.Span;

            if (memory.Length == length + 1)
            {
                if (span[0] == 0)
                {
                    return span.Slice(1).ToArray();
                }
            }

            if (span.Length > length)
            {
                throw new CryptographicException("Cryptography_Der_Invalid_Encoding");
            }

            byte[] target = new byte[length];
            span.CopyTo(target.AsSpan(length - span.Length));
            return target;
        }
        #endregion

        #region ToUnsignedIntegerBytes
        /// <summary>Converts to unsignedintegerbytes.</summary>
        /// <param name="memory">The memory.</param>
        /// <returns>System.Byte[].</returns>
        internal static byte[] ToUnsignedIntegerBytes(ReadOnlyMemory<byte> memory)
        {
            ReadOnlySpan<byte> span = memory.Span;

            if (span.Length > 1 && span[0] == 0)
            {
                return span.Slice(1).ToArray();
            }

            return span.ToArray();
        }
        #endregion 
        #endregion
    }
}
