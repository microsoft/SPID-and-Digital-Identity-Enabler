/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Microsoft.SPID.Proxy.Models.Options;

public class SPIDOptions
{
    public int DefaultSPIDL { get; set; }
    public List<int> ValidSPIDL { get; set; }
    public string SPIDLUri { get; set; }
    public List<string> ValidSPIDPurposeExtension { get; set; }
    public string PurposeName { get; set; }
    public string SpidLevelQueryStringParamName { get; set; }
    public int AssertionIssueInstantToleranceMins { get; set; }
    public string DefaultComparison{ get; set; }

    public string ComparisonQueryStringParamName { get; set; }
}
