/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Microsoft.SPID.Proxy.Services.Implementations;

public class IDPService : IIDPService
{
    private readonly IConfiguration _configuration;

    public IDPService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetIDPUrl(string idpName)
    {
        return _configuration[$"idpUrls:{idpName}"];
    }
}
