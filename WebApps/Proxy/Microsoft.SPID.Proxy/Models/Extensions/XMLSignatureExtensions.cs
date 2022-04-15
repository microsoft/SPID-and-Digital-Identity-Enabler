/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;

namespace Microsoft.SPID.Proxy.Models.Extensions;

public static class XMLSignatureExtensions
{
    /// <summary>
    /// Verifies the signature of the XmlDocument instance using the key enclosed with the signature.
    /// </summary>
    /// <param name="doc">The doc.</param>
    /// <returns><code>true</code> if the document's signature can be verified. <code>false</code> if the signature could
    /// not be verified.</returns>
    /// <exception cref="InvalidOperationException">if the XmlDocument instance does not contain a signed XML document.</exception>
    public static bool CheckSignature(this XmlDocument doc)
    {
        doc.CheckDocument();
        var signedXml = doc.RetrieveSignature();

        if (signedXml.SignatureMethod.Contains("rsa-sha256"))
        {
            // SHA256 keys must be obtained from message manually
            var trustedCertificates = doc.GetCertificates();
            foreach (var cert in trustedCertificates)
            {
                if (signedXml.CheckSignature(cert.GetRSAPublicKey()))
                {
                    return true;
                }
            }

            return false;
        }

        return signedXml.CheckSignature();
    }

    /// <summary>
    /// Verifies the signature of the XmlDocument instance using the key given as a parameter.
    /// </summary>
    /// <param name="doc">The doc.</param>
    /// <param name="alg">The algorithm.</param>
    /// <returns><code>true</code> if the document's signature can be verified. <code>false</code> if the signature could
    /// not be verified.</returns>
    /// <exception cref="InvalidOperationException">if the XmlDocument instance does not contain a signed XML document.</exception>
    public static bool CheckSignature(this XmlDocument doc, AsymmetricAlgorithm alg)
    {
        doc.CheckDocument();
        var signedXml = doc.RetrieveSignature();

        return signedXml.CheckSignature(alg);
    }

    /// <summary>
    /// Checks if a document contains a signature.
    /// </summary>
    /// <param name="doc">The doc.</param>
    /// <returns><c>true</c> if the specified doc is signed; otherwise, <c>false</c>.</returns>
    public static bool IsSigned(this XmlDocument doc)
    {
        doc.CheckDocument();
        var nodeList = doc.GetElementsByTagName(Saml20Constants.ElementNames.Signature, Saml20Constants.XMLDSIG);

        return nodeList.Count > 0;
    }

    /// <summary>
    /// Signs an XmlDocument with an xml signature using the signing certificate given as argument to the method.
    /// </summary>
    /// <param name="doc">The XmlDocument to be signed</param>
    /// <param name="id">The id of the topmost element in the XmlDocument</param>
    /// <param name="cert">The certificate used to sign the document</param>
    public static SignedXml SignDocument(this XmlDocument doc, string id, X509Certificate2 cert, string digestMethod)
    {
        var signedXml = new SignedXml(doc);
        signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;
        signedXml.SigningKey = cert.GetRSAPrivateKey();

        // Retrieve the value of the "ID" attribute on the root assertion element.
        var reference = new Reference("#" + id);

        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
        reference.AddTransform(new XmlDsigExcC14NTransform());

        signedXml.AddReference(reference);

        // Include the public key of the certificate in the assertion.
        signedXml.KeyInfo = new KeyInfo();
        signedXml.KeyInfo.AddClause(new KeyInfoX509Data(cert, X509IncludeOption.WholeChain));
        reference.DigestMethod = digestMethod;

        signedXml.ComputeSignature();

        return signedXml;
    }
}

internal static class XMLSignatureExtensionsUtils
{
    /// <summary>
    /// Do checks on the document given. Every public method accepting a XmlDocument instance as parameter should
    /// call this method before continuing.
    /// </summary>
    /// <param name="doc">The doc.</param>
    public static void CheckDocument(this XmlDocument doc)
    {
        if (doc == null)
        {
            throw new ArgumentNullException("doc");
        }

        if (!doc.PreserveWhitespace)
        {
            throw new InvalidOperationException("The XmlDocument must have its \"PreserveWhitespace\" property set to true when a signed document is loaded.");
        }
    }

    /// <summary>
    /// Gets the certificates.
    /// </summary>
    /// <param name="doc">The document.</param>
    /// <returns>List of <see cref="X509Certificate2"/>.</returns>
    public static List<X509Certificate2> GetCertificates(this XmlDocument doc)
    {
        var certificates = new List<X509Certificate2>();
        var nodeList = doc.GetElementsByTagName("ds:X509Certificate");
        if (nodeList.Count == 0)
        {
            nodeList = doc.GetElementsByTagName("X509Certificate");
        }

        foreach (XmlNode xn in nodeList)
        {
            try
            {
                var xc = new X509Certificate2(Convert.FromBase64String(xn.InnerText));
                certificates.Add(xc);
            }
            catch
            {
                // Swallow the certificate parse error
            }
        }

        return certificates;
    }

    /// <summary>
    /// Digs the &lt;Signature&gt; element out of the document.
    /// </summary>
    /// <param name="doc">The doc.</param>
    /// <returns>The <see cref="SignedXml"/>.</returns>
    /// <exception cref="InvalidOperationException">if the document does not contain a signature.</exception>
    public static SignedXml RetrieveSignature(this XmlDocument doc)
    {
        return RetrieveSignature(doc.DocumentElement);
    }


    /// <summary>
    /// Digs the &lt;Signature&gt; element out of the document.
    /// </summary>
    /// <param name="el">The element.</param>
    /// <returns>The <see cref="SignedXml"/>.</returns>
    /// <exception cref="InvalidOperationException">if the document does not contain a signature.</exception>
    public static SignedXml RetrieveSignature(this XmlElement el)
    {
        if (el.OwnerDocument.DocumentElement == null)
        {
            var doc = new XmlDocument() { PreserveWhitespace = true };
            doc.LoadXml(el.OuterXml);
            el = doc.DocumentElement;
        }

        SignedXml signedXml = new SignedXmlWithIdResolvement(el);
        var nodeList = el.GetElementsByTagName(Saml20Constants.ElementNames.Signature, Saml20Constants.XMLDSIG);
        if (nodeList.Count == 0)
        {
            throw new InvalidOperationException("Document does not contain a signature to verify.");
        }

        signedXml.LoadXml((XmlElement)nodeList[0]);

        // To support SHA256 for XML signatures, an additional algorithm must be enabled.
        // This is not supported in .Net versions older than 4.0. In older versions,
        // an exception will be raised if an SHA256 signature method is attempted to be used.
        if (signedXml.SignatureMethod.Contains("rsa-sha256"))
        {
            var addAlgorithmMethod = typeof(CryptoConfig).GetMethod("AddAlgorithm", BindingFlags.Public | BindingFlags.Static);
            if (addAlgorithmMethod == null)
            {
                throw new InvalidOperationException("This version of .Net does not support CryptoConfig.AddAlgorithm. Enabling sha256 not psosible.");
            }

            //addAlgorithmMethod.Invoke(null, new object[] { typeof(RSAPKCS1SHA256SignatureDescription), new[] { signedXml.SignatureMethod } });
        }

        // verify that the inlined signature has a valid reference uri
        VerifyReferenceUri(signedXml, el.GetAttribute("ID"));

        return signedXml;
    }

    /// <summary>
    /// Verifies that the reference uri (if any) points to the correct element.
    /// </summary>
    /// <param name="signedXml">the ds:signature element</param>
    /// <param name="id">the expected id referenced by the ds:signature element</param>
    public static SignedXml VerifyReferenceUri(this SignedXml signedXml, string id)
    {
        if (id == null)
        {
            throw new InvalidOperationException("Cannot match null id");
        }

        if (signedXml.SignedInfo.References.Count <= 0)
        {
            throw new InvalidOperationException("No references in Signature element");
        }

        var reference = (Reference)signedXml.SignedInfo.References[0];
        var uri = reference.Uri;

        // empty uri is okay - indicates that everything is signed
        if (!string.IsNullOrEmpty(uri))
        {
            if (!uri.StartsWith("#"))
            {
                throw new InvalidOperationException("Signature reference URI is not a document fragment reference. Uri = '" + uri + "'");
            }

            if (uri.Length < 2 || !id.Equals(uri.Substring(1)))
            {
                throw new InvalidOperationException("Rererence URI = '" + uri.Substring(1) + "' does not match expected id = '" + id + "'");
            }
        }

        return signedXml;
    }
}
