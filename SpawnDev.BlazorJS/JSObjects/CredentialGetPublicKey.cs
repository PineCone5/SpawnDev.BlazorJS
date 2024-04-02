﻿using System.Text.Json.Serialization;

namespace SpawnDev.BlazorJS.JSObjects
{
    /// <summary>
    /// Options for CredentialsContainer.Get()<br />
    /// CredentialsContainer.Get() will return a Promise that resolves with an PublicKeyCredential instance matching the provided parameters. If a single credential cannot be unambiguously obtained, the Promise will resolve to null.
    /// https://developer.mozilla.org/en-US/docs/Web/API/CredentialsContainer/get#publickey_object_structure
    /// </summary>
    public class CredentialGetPublicKey
    {
        /// <summary>
        /// An array of objects defining a restricted list of the acceptable credentials for retrieval. 
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<CredentialGetPublicKeyAllowedCredential>? AllowCredentials { get; set; }
        /// <summary>
        /// A string specifying the relying party's preference for how the attestation statement (i.e., provision of verifiable evidence of the authenticity of the authenticator and its data) is conveyed during authentication. The value can be one of the following:<br />
        /// "none" - Specifies that the relying party is not interested in authenticator attestation. This might be to avoid additional user consent for round trips to the relying party server to relay identifying information, or round trips to an attestation certificate authority (CA), with the aim of making the authentication process smoother. If "none" is chosen as the attestation value, and the authenticator signals that it uses a CA to generate its attestation statement, the client app will replace it with a "None" attestation statement, indicating that no attestation statement is available.<br />
        /// "direct" - Specifies that the relying party wants to receive the attestation statement as generated by the authenticator.<br />
        /// "enterprise" - Specifies that the relying party wants to receive an attestation statement that may include uniquely identifying information. This is intended for controlled deployments within an enterprise where the organization wishes to tie registrations to specific authenticators.<br />
        /// "indirect" - Specifies that the relying party wants to receive a verifiable attestation statement, but it will allow the client to decide how to receive it. For example, the client could choose to replace the authenticator's assertion statement with one generated by an anonymization CA to protect user privacy.<br />
        /// If attestation is omitted, it will default to "none".
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Attestation { get; set; }
        /// <summary>
        /// An array of strings specifying the relying party's preference for the attestation statement format used by the authenticator. Values should be ordered from highest to lowest preference, and should be considered hints — the authenticator may choose to issue an attestation statement in a different format. For a list of valid formats, see WebAuthn Attestation Statement Format Identifiers.<br />
        /// If omitted, attestationFormats defaults to an empty array.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? AttestationFormats { get; set; }
        /// <summary>
        /// 16 byte challenge. Must be randomly generated on the server.<br />
        /// An ArrayBuffer, TypedArray, or DataView originating from the relying party's server and used as a cryptographic challenge. This value will be signed by the authenticator and the signature will be sent back as part of the AuthenticatorAssertionResponse.signature (available in the response property of the PublicKeyCredential object returned by a successful get() call).
        /// </summary>
        public Union<ArrayBuffer, TypedArray, DataView, byte[]> Challenge { get; set; }
        /// <summary>
        /// An object containing properties representing the input values for any requested extensions. These extensions are used to specific additional processing by the client or authenticator during the authentication process. Examples include dealing with legacy FIDO API credentials, and evaluating outputs from a pseudo-random function (PRF) associated with a credential.<br />
        /// Extensions are optional and different browsers may recognize different extensions. Processing extensions is always optional for the client: if a browser does not recognize a given extension, it will just ignore it. For information on using extensions, and which ones are supported by which browsers, see Web Authentication extensions.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Extensions { get; set; }
        /// <summary>
        /// A string that specifies the relying party's identifier (for example "login.example.org"). For security purposes:<br />
        /// The calling web app verifies that rpId matches the relying party's origin.<br />
        /// The authenticator verifies that rpId matches the rpId of the credential used for the authentication ceremony.<br />
        /// If rpId is omitted, it will default to the current origin's domain.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RpId { get; set; }
        /// <summary>
        /// A numerical hint, in milliseconds, indicating the time the relying party is willing to wait for the retrieval operation to complete. This hint may be overridden by the browser.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public uint? Timeout { get; set; }
        /// <summary>
        /// A string specifying the relying party's requirements for user verification of the authentication process. This verification is initiated by the authenticator, which will request the user to provide an available factor (for example a PIN or a biometric input of some kind).<br />
        /// The value can be one of the following:<br />
        /// "required" - The relying party requires user verification, and the operation will fail if it does not occur.<br />
        /// "preferred" - The relying party prefers user verification if possible, but the operation will not fail if it does not occur.<br />
        /// "discouraged" - The relying party does not want user verification, in the interests of making user interaction as smooth as possible.<br />
        /// If userVerification is omitted, it will default to "preferred".
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? UserVerification { get; set; }
        /// <summary>
        /// An array of strings providing hints as to what authentication UI the user-agent should provide for the user.<br />
        /// The values can be any of the following:<br />
        /// "security-key" - Authentication requires a separate dedicated physical device to provide the key.<br />
        /// "client-device" - The user authenticates using their own device, such as a phone.<br />
        /// "hybrid" - Authentication relies on a combination of authorization/authentication methods, potentially relying on both user and server-based mechanisms.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Hints { get; set; }
    }

}