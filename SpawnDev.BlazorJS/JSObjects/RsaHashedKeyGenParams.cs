﻿namespace SpawnDev.BlazorJS.JSObjects
{
    /// <summary>
    /// The RsaHashedKeyGenParams dictionary of the Web Crypto API represents the object that should be passed as the algorithm parameter into SubtleCrypto.generateKey(), when generating any RSA-based key pair: that is, when the algorithm is identified as any of RSASSA-PKCS1-v1_5, RSA-PSS, or RSA-OAEP.
    /// </summary>
    public class RsaHashedKeyGenParams : KeyGenParams
    {
        /// <summary>
        /// A string. This should be set to RSASSA-PKCS1-v1_5, RSA-PSS, or RSA-OAEP, depending on the algorithm you want to use.
        /// </summary>
        public override string Name { get; set; }
        /// <summary>
        /// A Number. The length in bits of the RSA modulus. This should be at least 2048: see for example see SP 800-131A Rev. 2. Some organizations are now recommending that it should be 4096.
        /// </summary>
        public int ModulusLength { get; set; }
        /// <summary>
        /// A Uint8Array. The public exponent. Unless you have a good reason to use something else, specify 65537 here ([0x01, 0x00, 0x01]).
        /// </summary>
        public Union<Uint8Array, byte[]> PublicExponent { get; set; }
        /// <summary>
        /// A string representing the name of the digest function to use. You can pass any of SHA-256, SHA-384, or SHA-512 here.
        /// </summary>
        public string Hash { get; set; }
    }
}
