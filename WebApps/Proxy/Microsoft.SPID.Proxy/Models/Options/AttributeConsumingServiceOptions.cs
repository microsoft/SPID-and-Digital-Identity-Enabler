/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Microsoft.SPID.Proxy.Models.Options;

public class AttributeConsumingServiceOptions
{
    public int AttributeConsumingServiceDefaultValue { get; set; }
    public bool UpdateAssertionConsumerServiceUrl { get; set; }
    public List<string> ValidACS { get; set; }
    public List<string> CIEValidACS { get; set; }
	public string AttrConsServIndexQueryStringParamName { get; set; }
	public string CIEAttrConsServIndexQueryStringParamName { get; set; }

	public int CIEAttributeConsumingService { get; set; }
    public int EIDASAttributeConsumingService { get; set; }
    public bool DisableACSFromReferer { get; set; }
}
