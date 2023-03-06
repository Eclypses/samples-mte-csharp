// Copyright (c) Eclypses Inc. All rights reserved.

using PackageCSharpECDHFW.EcdhHelpers;
using System;
using System.Security.Cryptography;

namespace PackageCSharpECDHFW
{
    public class EclypsesECDH : IEclypsesECDH
    {
        #region EclypsesECDH Constructor
        private ECDiffieHellman _theContainer;

        public EclypsesECDH()
        {
        }
        #endregion

        #region GetTheContainer
        /// <summary>
        /// Gets the DH NistP256 container
        /// If container not created yet it will be created
        /// </summary>
        /// <returns>ECDiffieHellman.</returns>
        public ECDiffieHellman GetTheContainer()
        {
            try
            {

                if (_theContainer == null)
                {
                    _theContainer = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
                }
                return _theContainer;
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region ClearContainer
        /// <summary>
        /// Method that will clear the container
        /// </summary>
        public void ClearContainer()
        {
            _theContainer = null;
        }
        #endregion

        #region GetPublicKey

        /// <summary>
        /// Gets the public key from the ECDiffieHellman container.
        /// If no container is passed in the global container is used
        /// If no global container has been created yet a new one is created
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>System.Byte[].</returns>
        /// <exception cref="ApplicationException">Exception exporting public key.</exception>
        public byte[] GetPublicKey(ECDiffieHellman container = null)
        {
            try
            {

                //------------------------------------------------
                // If no container passed in use global container
                //------------------------------------------------
                container ??= GetTheContainer();

                //------------------------
                // Get container key size
                //------------------------
                var containerKeySize = container.KeySize;

                //------------------------
                // set byte array
                //------------------------
                var someByteArray = new byte[containerKeySize];

                //----------------------------------
                // Export Public key from container
                //----------------------------------
                Span<byte> exportedPublicKey = someByteArray;
                GetPublicKeyHelper.TryExportSubjectPublicKeyInfo(container, exportedPublicKey, out var bytesWritten);

                //--------------------------------
                // return the public key as byte[]
                //--------------------------------
                return exportedPublicKey.Slice(0, bytesWritten).ToArray();


            }
            catch
            {
                throw;
            }

        }
        #endregion

        #region ProcessPartnerPublicKey
        /// <summary>
        /// Processes the partner public key and returns the following:
        /// byte[] PartnerPublicKey --> value passed in
        /// byte[] PublicKey --> this machines DH container's public key
        /// byte[] SharedSecret --> shared secret of the two returned keys
        /// 
        /// </summary>
        /// <param name="partnerPublicKey">The partner public key.</param>
        /// <returns>SharedSecretModel.</returns>
        /// <exception cref="ApplicationException">The partner public key cannot be null or empty.</exception>
        /// <exception cref="ApplicationException">Could not use the partner public key: {Convert.ToBase64String(partnerPublicKey)}</exception>
        public SharedSecretModel ProcessPartnerPublicKey(byte[] partnerPublicKey)
        {
            //_logger.Debug("Entering ProcessPartnerPublicKey method.");
            var response = new SharedSecretModel { PartnerPublicKey = partnerPublicKey };
            try
            {
                //------------------------------------------------------------
                // Check to make sure partner public key is not null or blank
                //------------------------------------------------------------
                if (partnerPublicKey == null || partnerPublicKey.Length <= 0)
                {
                    //_logger.Error("The partner public key cannot be null or empty.");
                    throw new ApplicationException("The partner public key cannot be null or empty.");
                }
                //------------------------------------------------
                // If no container has been created yet create one
                //------------------------------------------------
                _theContainer ??= GetTheContainer();

                response.PartnerPublicKey = partnerPublicKey;

                response.PublicKey = GetPublicKey();

                //-----------------------------
                // Create partner DH container
                //-----------------------------
                using var partnerContainer = ECDiffieHellman.Create();

                //--------------------------------
                // Import the partner's public key
                //--------------------------------
                ImportPublicKeyHelper.ImportSubjectPublicKeyInfo(partnerContainer, partnerPublicKey, out _);

                //--------------------------------
                // Derive shared secret
                //--------------------------------
                response.SharedSecret = _theContainer.DeriveKeyMaterial(partnerContainer.PublicKey);

                return response;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Could not use the partner public key: {Convert.ToBase64String(partnerPublicKey)}", ex);
            }
        }
        #endregion
    }
}
