/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Microsoft.SPID.Proxy.Models.Options;

public class IDPMetadatasOptions
{
	public IEnumerable<string> MetadataKeyPrefixes { get; set; }
	public Dictionary<string,string> MetadataMapping { get; set; }
    public int CacheAbsoluteExpirationInMins { get; set; } = 120;
}