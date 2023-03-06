// Copyright (c) Eclypses Inc. All rights reserved.

namespace PackageCSharpECDH {
    public class SharedSecretModel {
        /// <summary>
        /// Gets or sets the public key for this machine.
        /// </summary>
        /// <value>The public key.</value>
        public byte[] PublicKey { get; set; }
        /// <summary>
        /// Gets or sets the partner public key.
        /// This should be provided by other side.
        /// </summary>
        /// <value>The partner public key.</value>
        public byte[] PartnerPublicKey { get; set; }
        /// <summary>
        /// Gets or sets the shared secret.
        /// </summary>
        /// <value>The shared secret.</value>
        public byte[] SharedSecret { get; set; }
    }
}
