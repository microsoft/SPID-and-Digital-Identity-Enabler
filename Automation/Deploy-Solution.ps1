# ----------------------------------------------------------------------------------------------
#   Copyright (c) Microsoft Corporation. All rights reserved.
#   Licensed under the MIT License. See License.txt in the project root for license information.
# ----------------------------------------------------------------------------------------------

Import-Module -Name $PSScriptRoot\SetUp-B2C.psm1
Import-Module -Name $PSScriptRoot\Deploy-Infrastructure.psm1

$settings = Get-Content -Path settings.json | ConvertFrom-Json

$subscriptionId                         = "9b964e07-d792-4ced-9ed1-2fac0991eb89"
$tenantName                             = "tommasostocchioutlook"
$tenantId                               = "9bd25ba3-e2bc-4965-986b-5062a04a550a"
$resourceGroupName                      = "rg-testspidautomation"
$b2cTenantName                          = "SPIDProxyTommaso"
$deployerClientId                       = $settings.deployerClientId
$b2cPoliciesUploadClientId              = $settings.b2cPoliciesUploadClientId
$b2cPoliciesUploadClientSecret          = $settings.b2cPoliciesUploadClientSecret
$b2cFolderLocation                      = "$PSScriptRoot\..\AAD B2C\CustomPolicies"
# These two should be inherited from above steps
$identityExperienceFrameworkAppId       = "ee1a430a-8708-4713-bd7f-1a3feb26a2a3"
$proxyIdentityExperienceFrameworkAppId  = "7a623a7b-6820-4f35-85e7-04eccbbbfa7f"

Deploy-Infrastructure `
    -subscriptionId $subscriptionId `
    -deployerClientId $deployerClientId `
    -tenantId $tenantId `
    -resourceGroupName $resourceGroupName `
    -b2cTenantName $b2cTenantName

# TODO: Retrieve appinsights key from the deployment

# Run SetUp-B2C.psm1
SetUp-B2C `
    -b2cFolderLocation $b2cFolderLocation `
    -tenantName $tenantName `
    -b2cPoliciesUploadClientId $b2cPoliciesUploadClientId `
    -b2cPoliciesUploadClientSecret $b2cPoliciesUploadClientSecret `
    -storageAccountUrl $storageAccountUrl `
    -identityExperienceFrameworkAppId $identityExperienceFrameworkAppId `
    -proxyIdentityExperienceFrameworkAppId $proxyIdentityExperienceFrameworkAppId `
    -applicationInsightsKey $applicationInsightsKey

# Set-Location $PSScriptRoot

# Run Deploy-Proxy.psm1

# Run Deploy-CustomUI.psm1

# Run Deploy-Metadatas.psm1