/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Microsoft.SPID.Proxy.Models.Extensions;

public static class FederatorRequestExtensions
{
    public static bool IsCIEOrEIDAS(this FederatorRequest federatorRequest)
    {
        return federatorRequest.IsCIE() || federatorRequest.IsEIDAS();
    }

    public static bool IsCIE(this FederatorRequest federatorRequest)
    {
        return federatorRequest.IdentityProvider.Equals("CIE", StringComparison.InvariantCultureIgnoreCase)
            || federatorRequest.IdentityProvider.Equals("CIETEST", StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool IsEIDAS(this FederatorRequest federatorRequest)
    {
        return federatorRequest.IdentityProvider.Equals("EIDAS", StringComparison.InvariantCultureIgnoreCase)
            || federatorRequest.IdentityProvider.Equals("EIDASTEST", StringComparison.InvariantCultureIgnoreCase);
    }

    public static int GetAttributeConsumingService(this FederatorRequest federatorRequest,
        int cieAttributeConsumerServiceValue,
        int eidasAttributeConsumerServiceValue,
        int attributeConsumerService)
    {
        if (federatorRequest.IsCIE())
        {
            return cieAttributeConsumerServiceValue;
        }
        else if (federatorRequest.IsEIDAS())
        {
            return eidasAttributeConsumerServiceValue;
        }
        else
        {
            return attributeConsumerService;
        }
    }
}
