/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Security.Cryptography.Xml;

namespace Microsoft.SPID.Proxy.Models;

/// <summary>
/// Signed XML with Id Resolvement class.
/// </summary>
public class SignedXmlWithIdResolvement : SignedXml
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SignedXmlWithIdResolvement"/> class.
    /// </summary>
    /// <param name="document">The document.</param>
    public SignedXmlWithIdResolvement(XmlDocument document) : base(document) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SignedXmlWithIdResolvement"/> class from the specified <see cref="T:System.Xml.XmlElement"/> object.
    /// </summary>
    /// <param name="elem">The <see cref="T:System.Xml.XmlElement"/> object to use to initialize the new instance of <see cref="T:System.Security.Cryptography.Xml.SignedXml"/>.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// The <paramref name="elem"/> parameter is null.
    /// </exception>
    public SignedXmlWithIdResolvement(XmlElement elem) : base(elem) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SignedXmlWithIdResolvement"/> class.
    /// </summary>
    public SignedXmlWithIdResolvement() { }
}