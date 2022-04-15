/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Microsoft.SPID.Proxy.Models;

public class Saml20Constants
{
    //
    // Summary:
    //     SAML Version
    public const string Version = "2.0";
    //
    // Summary:
    //     The XML namespace of the SAML 2.0 assertion schema.
    public const string ASSERTION = "urn:oasis:names:tc:SAML:2.0:assertion";
    //
    // Summary:
    //     The XML namespace of the SAML 2.0 protocol schema
    public const string PROTOCOL = "urn:oasis:names:tc:SAML:2.0:protocol";
    //
    // Summary:
    //     The XML namespace of the SAML 2.0 metadata schema
    public const string METADATA = "urn:oasis:names:tc:SAML:2.0:metadata";
    //
    // Summary:
    //     The XML namespace of XmlDSig
    public const string XMLDSIG = "http://www.w3.org/2000/09/xmldsig#";
    //
    // Summary:
    //     The XML namespace of XmlEnc
    public const string XENC = "http://www.w3.org/2001/04/xmlenc#";
    //
    // Summary:
    //     The default value of the Format property for a NameID element
    public const string DEFAULTNAMEIDFORMAT = "urn:oasis:names:tc:SAML:1.0:nameid-format:unspecified";
    //
    // Summary:
    //     The mime type that must be used when publishing a metadata document.
    public const string METADATA_MIMETYPE = "application/samlmetadata+xml";
    //
    // Summary:
    //     A mandatory prefix for translating arbitrary saml2.0 claim names to saml1.1 attributes
    public const string DKSAML20_CLAIMTYPE_PREFIX = "dksaml20/";
    //
    // Summary:
    //     All the namespaces defined and reserved by the SAML 2.0 standard
    public static readonly string[] SAML_NAMESPACES;

    public Saml20Constants() { }

    public static class ElementNames
    {
        public const string Signature = "Signature";
    }
    //
    // Summary:
    //     Formats of nameidentifiers
    public static class NameIdentifierFormats
    {
        //
        // Summary:
        //     urn for Unspecified name identifier format
        public const string Unspecified = "urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified";
        //
        // Summary:
        //     urn for Email name identifier format
        public const string Email = "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress";
        //
        // Summary:
        //     urn for X509SubjectName name identifier format
        public const string X509SubjectName = "urn:oasis:names:tc:SAML:1.1:nameid-format:X509SubjectName";
        //
        // Summary:
        //     urn for Windows name identifier format
        public const string Windows = "urn:oasis:names:tc:SAML:1.1:nameid-format:WindowsDomainQualifiedName";
        //
        // Summary:
        //     urn for Kerberos name identifier format
        public const string Kerberos = "urn:oasis:names:tc:SAML:2.0:nameid-format:kerberos";
        //
        // Summary:
        //     urn for Entity name identifier format
        public const string Entity = "urn:oasis:names:tc:SAML:2.0:nameid-format:entity";
        //
        // Summary:
        //     urn for Persistent name identifier format
        public const string Persistent = "urn:oasis:names:tc:SAML:2.0:nameid-format:persistent";
        //
        // Summary:
        //     urn for Transient name identifier format
        public const string Transient = "urn:oasis:names:tc:SAML:2.0:nameid-format:transient";
    }
    //
    // Summary:
    //     Protocol bindings
    public static class ProtocolBindings
    {
        //
        // Summary:
        //     HTTP Redirect protocol binding
        public const string HTTP_Redirect = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect";
        //
        // Summary:
        //     HTTP Post protocol binding
        public const string HTTP_Post = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST";
        //
        // Summary:
        //     HTTP Artifact protocol binding
        public const string HTTP_Artifact = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Artifact";
        //
        // Summary:
        //     HTTP SOAP protocol binding
        public const string HTTP_SOAP = "urn:oasis:names:tc:SAML:2.0:bindings:SOAP";
    }
    //
    // Summary:
    //     Subject confirmation methods
    public static class SubjectConfirmationMethods
    {
        //
        // Summary:
        //     Holder of key confirmation method
        public const string HolderOfKey = "urn:oasis:names:tc:SAML:2.0:cm:holder-of-key";
    }
    //
    // Summary:
    //     Logout reasons
    public static class Reasons
    {
        //
        // Summary:
        //     Specifies that the message is being sent because the principal wishes to terminate
        //     the indicated session.
        public const string User = "urn:oasis:names:tc:SAML:2.0:logout:user";
        //
        // Summary:
        //     Specifies that the message is being sent because an administrator wishes to terminate
        //     the indicated session for that principal.
        public const string Admin = "urn:oasis:names:tc:SAML:2.0:logout:admin";
    }
    //
    // Summary:
    //     Status codes
    public static class StatusCodes
    {
        //
        // Summary:
        //     The request succeeded.
        public const string Success = "urn:oasis:names:tc:SAML:2.0:status:Success";
        //
        // Summary:
        //     An entity that has no knowledge of a particular attribute profile has been presented
        //     with an attribute drawn from that profile.
        public const string UnknownAttrProfile = "urn:oasis:names:tc:SAML:2.0:status:UnknownAttrProfile";
        //
        // Summary:
        //     The response message would contain more elements than the SAML responder is able
        //     to return.
        public const string TooManyResponses = "urn:oasis:names:tc:SAML:2.0:status:TooManyResponses";
        //
        // Summary:
        //     The resource value provided in the request message is invalid or unrecognized.
        public const string ResourceNotRecognized = "urn:oasis:names:tc:SAML:2.0:status:ResourceNotRecognized";
        //
        // Summary:
        //     The SAML responder cannot process the request because the protocol version specified
        //     in the request message is too low.
        public const string RequestVersionTooLow = "urn:oasis:names:tc:SAML:2.0:status:RequestVersionTooLow";
        //
        // Summary:
        //     The SAML responder cannot process the request because the protocol version specified
        //     in the request message is a major upgrade from the highest protocol version supported
        //     by the responder.
        public const string RequestVersionTooHigh = "urn:oasis:names:tc:SAML:2.0:status:RequestVersionTooHigh";
        //
        // Summary:
        //     The SAML responder cannot process any requests with the protocol version specified
        //     in the request.
        public const string RequestVersionDeprecated = "urn:oasis:names:tc:SAML:2.0:status:RequestVersionDeprecated";
        //
        // Summary:
        //     The SAML responder or SAML authority does not support the request.
        public const string RequestUnsupported = "urn:oasis:names:tc:SAML:2.0:status:RequestUnsupported";
        //
        // Summary:
        //     The SAML responder or SAML authority is able to process the request but has chosen
        //     not to respond. This status code MAY be used when there is concern about the
        //     security context of the request message or the sequence of request messages received
        //     from a particular requester.
        public const string RequestDenied = "urn:oasis:names:tc:SAML:2.0:status:RequestDenied";
        //
        // Summary:
        //     Indicates that a responding provider cannot authenticate the principal directly
        //     and is not permitted to proxy the request further.
        public const string ProxyCountExceeded = "urn:oasis:names:tc:SAML:2.0:status:ProxyCountExceeded";
        //
        // Summary:
        //     The responding provider does not recognize the principal specified or implied
        //     by the request.
        public const string UnknownPrincipal = "urn:oasis:names:tc:SAML:2.0:status:UnknownPrincipal";
        //
        // Summary:
        //     Used by a session authority to indicate to a session participant that it was
        //     not able to propagate logout to all other session participants.
        public const string PartialLogout = "urn:oasis:names:tc:SAML:2.0:status:PartialLogout";
        //
        // Summary:
        //     Indicates the responding provider cannot authenticate the principal passively,
        //     as has been requested.
        public const string NoPassive = "urn:oasis:names:tc:SAML:2.0:status:NoPassive";
        //
        // Summary:
        //     Used by an intermediary to indicate that none of the supported identity provider
        //     <Loc> elements in an <IDPList> can be resolved or that none of the supported
        //     identity providers are available.
        public const string NoAvailableIDP = "urn:oasis:names:tc:SAML:2.0:status:NoAvailableIDP";
        //
        // Summary:
        //     The specified authentication context requirements cannot be met by the responder.
        public const string NoAuthnContext = "urn:oasis:names:tc:SAML:2.0:status:NoAuthnContext";
        //
        // Summary:
        //     The responding provider cannot or will not support the requested name identifier
        //     policy.
        public const string InvalidNameIdPolicy = "urn:oasis:names:tc:SAML:2.0:status:InvalidNameIDPolicy";
        //
        // Summary:
        //     Unexpected or invalid content was encountered within a <saml:Attribute> or <saml:AttributeValue>
        //     element.
        public const string InvalidAttrNameOrValue = "urn:oasis:names:tc:SAML:2.0:status:InvalidAttrNameOrValue";
        //
        // Summary:
        //     The responding provider was unable to successfully authenticate the principal.
        public const string AuthnFailed = "urn:oasis:names:tc:SAML:2.0:status:AuthnFailed";
        //
        // Summary:
        //     The SAML responder could not process the request because the version of the request
        //     message was incorrect.
        public const string VersionMismatch = "urn:oasis:names:tc:SAML:2.0:status:VersionMismatch";
        //
        // Summary:
        //     The request could not be performed due to an error on the part of the SAML responder
        //     or SAML authority.
        public const string Responder = "urn:oasis:names:tc:SAML:2.0:status:Responder";
        //
        // Summary:
        //     The request could not be performed due to an error on the part of the requester.
        public const string Requester = "urn:oasis:names:tc:SAML:2.0:status:Requester";
        //
        // Summary:
        //     Used by an intermediary to indicate that none of the identity providers in an
        //     <IDPList> are supported by the intermediary.
        public const string NoSupportedIDP = "urn:oasis:names:tc:SAML:2.0:status:NoSupportedIDP";
        //
        // Summary:
        //     The SAML responder cannot properly fulfill the request using the protocol binding
        //     specified in the request.
        public const string UnsupportedBinding = "urn:oasis:names:tc:SAML:2.0:status:UnsupportedBinding";
    }
}