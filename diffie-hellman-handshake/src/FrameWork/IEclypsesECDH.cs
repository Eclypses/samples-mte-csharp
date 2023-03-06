// Copyright (c) Eclypses Inc. All rights reserved.

using System.Security.Cryptography;

namespace PackageCSharpECDHFW
{
    public interface IEclypsesECDH
    {

        /// <summary>
        /// Clears the container.
        /// </summary>
        public void ClearContainer();
        /// <summary>
        /// Returns the global DH container.
        /// </summary>
        /// <returns>ECDiffieHellman.</returns>
        public ECDiffieHellman GetTheContainer();

        /// <summary>
        /// Gets the public key of passed in container OR
        /// If null returns global DH container public key
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>System.Byte[].</returns>
        public byte[] GetPublicKey(ECDiffieHellman container = null);

        /// <summary>
        /// Processes the partner public key and returns the following:
        /// byte[] PartnerPublicKey --> value passed in
        /// byte[] PublicKey --> this machines DH container's public key
        /// byte[] SharedSecret --> shared secret of the two returned keys
        /// </summary>
        /// <param name="partnerPublicKey">The partner public key.</param>
        /// <returns>SharedSecretModel.</returns>
        public SharedSecretModel ProcessPartnerPublicKey(byte[] partnerPublicKey);


    }
}
