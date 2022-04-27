# ----------------------------------------------------------------------------------------------
#   Copyright (c) Microsoft Corporation. All rights reserved.
#   Licensed under the MIT License. See License.txt in the project root for license information.
# ----------------------------------------------------------------------------------------------

Import-Module -Name $PSScriptRoot\Create-B2CSigningKey.psm1
Import-Module -Name $PSScriptRoot\SetUp-B2CPolicies.psm1
function Configure-B2C {
    Param(
        [Parameter(Mandatory = $true)]
        [string]$b2cFolderLocation,
        [Parameter(Mandatory=$true)]
        [string]$tenantName,
        [Parameter(Mandatory=$true)]
        [string]$identityExperienceFrameworkAppId,
        [Parameter(Mandatory=$true)]
        [string]$proxyIdentityExperienceFrameworkAppId,
        [Parameter(Mandatory=$true)]
        [string]$storageAccountUrl,
        [Parameter(Mandatory=$true)]
        [string]$appInsightsKey,
        [Parameter(Mandatory=$true)]
        [string]$accessToken
    )
    
    Create-B2CSigningKey `
        -tenant $tenantName `
        -accessToken $accessToken

    SetUp-B2CPolicies `
        -b2cFolderLocation $b2cFolderLocation `
        -tenant $tenantName `
        -identityExperienceFrameworkAppId $identityExperienceFrameworkAppId `
        -proxyIdentityExperienceFrameworkAppId $proxyIdentityExperienceFrameworkAppId `
        -metadatasBlobStorageUrl "https://$storageAccountUrl/metadatas" `
        -customUiBlobStorageUrl "https://$storageAccountUrl/customui" `
        -appInsightsKey $appInsightsKey `
        -accessToken $accessToken
}
Export-ModuleMember -Name Configure-B2C