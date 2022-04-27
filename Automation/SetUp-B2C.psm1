# ----------------------------------------------------------------------------------------------
#   Copyright (c) Microsoft Corporation. All rights reserved.
#   Licensed under the MIT License. See License.txt in the project root for license information.
# ----------------------------------------------------------------------------------------------

Import-Module -Name $PSScriptRoot\B2C\Configure-B2C.psm1

function SetUp-B2C {
    Param(
        [Parameter(Mandatory=$true)]
        [string]$b2cFolderLocation,
        [Parameter(Mandatory=$true)]
        [string]$tenantName,
        [Parameter(Mandatory = $true)]
        [string]$b2cPoliciesUploadClientId,
        [Parameter(Mandatory = $true)]
        [string]$b2cPoliciesUploadClientSecret,
        [Parameter(Mandatory = $true)]
        [string]$storageAccountUrl,
        [Parameter(Mandatory = $true)]
        [string]$identityExperienceFrameworkAppId,
        [Parameter(Mandatory = $true)]
        [string]$proxyIdentityExperienceFrameworkAppId,
        [Parameter(Mandatory = $true)]
        [string]$applicationInsightsKey
    )

    # Should do the same as IEFSetup
    # Since this inolves the creation of a few app registrations, it can't be done via az cli nor apis
    
    # Create App Registration for B2C Custom Policies Upload
    # Looks like this can't be done
    
    $body = @{
        grant_type = "client_credentials"; 
        scope = "https://graph.microsoft.com/.default"; 
        client_id = $b2cPoliciesUploadClientId; 
        client_secret = $b2cPoliciesUploadClientSecret 
    }
    $response = Invoke-RestMethod -Uri "https://login.microsoftonline.com/$tenantName.onmicrosoft.com/oauth2/v2.0/token" -Method Post -Body $body
    $accessToken = $response.access_token

    Configure-B2C `
        -b2cFolderLocation $b2cFolderLocation `
        -tenantName $tenantName `
        -metadatasBlobStorageUrl "https://$storageAccountUrl/metadatas" `
        -customUiBlobStorageUrl "https://$storageAccountUrl/customui" `
        -identityExperienceFrameworkAppId $identityExperienceFrameworkAppId `
        -proxyIdentityExperienceFrameworkAppId $proxyIdentityExperienceFrameworkAppId `
        -appInsightsKey $applicationInsightsKey `
        -accessToken $accessToken
}