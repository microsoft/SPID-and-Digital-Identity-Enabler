/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Microsoft.SPID.Proxy.Models.Options;

public class FederatorOptions
{
    public string MetadataUrl { get; set; }
    public string SPIDEntityId { get; set; }
    public string EntityId { get; set; }
    public string FederatorAttributeConsumerServiceUrl { get; set; }
}