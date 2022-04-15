/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.IO.Compression;
using System.Text;

namespace Microsoft.SPID.Proxy.Models.Extensions;

public static class SAMLRequestResponseExtensions
{
    public static string DecodeSamlResponse(this string samlResponse)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(samlResponse));
    }

    public static string EncodeSamlResponse(this XmlDocument samlResponse)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(samlResponse.InnerXml));
    }

    public static string DecodeSamlRequest(this string samlRequest)
    {
        using var input = new MemoryStream(Convert.FromBase64String(samlRequest));
        using var unzip = new DeflateStream(input, CompressionMode.Decompress);
        using var reader = new StreamReader(unzip, Encoding.UTF8);

        return reader.ReadToEnd();
    }

    public static XmlDocument ToXmlDocument(this string decodedSaml)
    {
        XmlDocument doc = new XmlDocument()
        {
            PreserveWhitespace = true
        };

        doc.LoadXml(decodedSaml);
        return doc;
    }

    public static string EncodeSamlRequest(this string samlRequest)
    {
        var bytes = Encoding.UTF8.GetBytes(samlRequest);

        string middle;
        using (var output = new MemoryStream())
        {
            using (var zip = new DeflateStream(output, CompressionMode.Compress))
            {
                zip.Write(bytes, 0, bytes.Length);
            }

            middle = Convert.ToBase64String(output.ToArray());
            return middle.UpperCaseUrlEncode();
        }
    }
}
