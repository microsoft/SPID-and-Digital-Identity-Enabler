/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Microsoft.SPID.Proxy.Models.Options;

public class FederatorRequestValidationOptions
{
    public bool SkipSAMLRequestSignatureValidation { get; set; }
    public int MetadataCacheAbsoluteExpirationInMins { get; set; } = 240;
}
