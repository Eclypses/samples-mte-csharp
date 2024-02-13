// Copyright (c) Eclypses Inc. All rights reserved.

using System;
using System.Security.Cryptography;
using Serilog;
using Serilog.Events;

namespace PackageCSharpECDH {
    public class EclypsesECDH : IEclypsesECDH {
        #region EclypsesECDH Constructor
        private readonly ILogger _logger;
        private ECDiffieHellman _theContainer;

        public EclypsesECDH(LogEventLevel logLevel = LogEventLevel.Error) {
            //-----------------------------------------
            // Set up logging based on what passed in
            //-----------------------------------------
            _logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.RollingFile(AppDomain.CurrentDomain.BaseDirectory + "DHLog-{Date}.txt", logLevel, shared: true)
                .Enrich.FromLogContext()
                .CreateLogger();
        }
        #endregion

        #region GetTheContainer
        /// <summary>
        /// Gets the DH NistP256 container
        /// If container not created yet it will be created
        /// </summary>
        /// <returns>ECDiffieHellman.</returns>
        public ECDiffieHellman GetTheContainer() {
            _logger.Debug("Entering GetContainer method.");
            try {

                if (_theContainer == null) {
                    _logger.Debug("Container is null - creating new DH container");
                    _theContainer = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
                }
                return _theContainer;
            } catch (Exception ex) {
                _logger.Error($"Exception getting the global DH container: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region ClearContainer
        /// <summary>
        /// Method that will clear the container
        /// </summary>
        public void ClearContainer() {
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
        public byte[] GetPublicKey(ECDiffieHellman container = null) {
            _logger.Debug("Entering GetPublicKey method.");
            try {
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
                _logger.Debug($"Container key size {someByteArray.Length}");

                //----------------------------------
                // Export Public key from container
                //----------------------------------
                Span<byte> exportedPublicKey = someByteArray;

                _logger.Debug("Try Exporting subject public key.");
                container.TryExportSubjectPublicKeyInfo(exportedPublicKey, out var bytesWritten);

                _logger.Debug("Return byte[] of exported public key.");
                return exportedPublicKey.Slice(0, bytesWritten).ToArray();

            } catch (Exception ex) {
                _logger.Error($"Exception exporting public key: {ex.Message}");
                throw;
            }

        }
        #endregion

        #region CreateSharedSecret
        /// <summary>
        /// Creates the shared secret using byte[] of partner public key
        /// If no container is passed in, use global container
        /// </summary>
        /// <param name="partnerPublicKeyArray">The partner public key array.</param>
        /// <param name="theContainer">The container.</param>
        /// <returns>System.String of the shared secret based on the container and partner's public key.</returns>
        /// <exception cref="ApplicationException">You must Create the Key Container prior to requesting a shared secret.</exception>
        public byte[] CreateSharedSecret(byte[] partnerPublicKeyArray, ECDiffieHellman theContainer = null) {
            try {
                _logger.Debug("Entering CreateSharedSecret method.");
                //------------------------------------------------
                // If no container passed in use global container
                //------------------------------------------------
                theContainer ??= GetTheContainer();
                _logger.Debug("Check for container, if none use global.");

                //------------------------------------------------
                // Export the current private key (we need it later)
                //------------------------------------------------
                byte[] privateKey = theContainer.ExportECPrivateKey();
                _logger.Debug($"Exported private key as base64 string: {Convert.ToBase64String(privateKey)}");

                //----------------------------------
                // Import the partner's public key
                //----------------------------------
                theContainer.ImportSubjectPublicKeyInfo(partnerPublicKeyArray, out int _);
                _logger.Debug("Imported partner's public key.");

                //---------------------------------------
                // Convert it into a "public key" object.
                //---------------------------------------
                var publicKey = theContainer.PublicKey;
                _logger.Debug($"Converted it into public key object.");

                //--------------------------------------------------------------------
                // Import the private key to ensure it is this endpoint's private key
                //--------------------------------------------------------------------
                theContainer.ImportECPrivateKey(privateKey, out int _);
                _logger.Debug("Imported the private key to ensure it is this endpoint's private key");

                //--------------------------
                // Zero out the private key 
                //--------------------------
                privateKey = new byte[privateKey.Length];

                //-----------------------------
                // Generate the shared secret
                //-----------------------------
                return theContainer.DeriveKeyMaterial(publicKey);
            } catch (Exception ex) {
                _logger.Error($"Exception creating secret key, {ex.Message}");
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
        public SharedSecretModel ProcessPartnerPublicKey(byte[] partnerPublicKey) {
            _logger.Debug("Entering ProcessPartnerPublicKey method.");
            var response = new SharedSecretModel { PartnerPublicKey = partnerPublicKey };
            try {
                //------------------------------------------------------------
                // Check to make sure partner public key is not null or blank
                //------------------------------------------------------------
                if (partnerPublicKey == null || partnerPublicKey.Length <= 0) {
                    _logger.Error("The partner public key cannot be null or empty.");
                    throw new ApplicationException("The partner public key cannot be null or empty.");
                }
                //------------------------------------------------
                // If no container has been created yet create one
                //------------------------------------------------
                _theContainer ??= GetTheContainer();

                //------------------------------------------
                // Export public key of global DH container
                //------------------------------------------
                response.PublicKey = _theContainer.ExportSubjectPublicKeyInfo();
                _logger.Debug($"Partner public key as base64 string: {Convert.ToBase64String(response.PartnerPublicKey)}");
                _logger.Debug($"This public key as base64 string: {Convert.ToBase64String(response.PublicKey)}");

                //-----------------------------
                // Create partner DH container
                //-----------------------------
                using var partnerContainer = ECDiffieHellman.Create();
                partnerContainer.ImportSubjectPublicKeyInfo(partnerPublicKey, out _);
                //--------------------------------
                // Derive shared secret
                //--------------------------------
                response.SharedSecret = _theContainer.DeriveKeyMaterial(partnerContainer.PublicKey);
                _logger.Debug($"The shared secret as base64 string: {Convert.ToBase64String(response.SharedSecret)}");

                return response;
            } catch (Exception ex) {
                _logger.Error($"Could not use the partner public key: {Convert.ToBase64String(partnerPublicKey)}");
                throw new ApplicationException($"Could not use the partner public key: {Convert.ToBase64String(partnerPublicKey)}", ex);
            }
        }
        #endregion
    }
}
