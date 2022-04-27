# ----------------------------------------------------------------------------------------------
#   Copyright (c) Microsoft Corporation. All rights reserved.
#   Licensed under the MIT License. See License.txt in the project root for license information.
# ----------------------------------------------------------------------------------------------

Import-Module -Name $PSScriptRoot\B2C\Create-B2CTenant.psm1

function Deploy-Infrastructure {
    Param(
        [Parameter(Mandatory=$true)]
        [string]$subscriptionId,
        [Parameter(Mandatory=$true)]
        [string]$tenantId,
        [Parameter(Mandatory=$true)]
        [string]$resourceGroupName,
        [Parameter(Mandatory=$true)]
        [string]$b2cTenantName,
        [Parameter(Mandatory=$true)]
        [string]$deployerClientId
    )

    # Deploy Bicep files
    az deployment sub create `
        --location westeurope `
        --template-file .\Bicep\main.bicep

    # Run Create-B2CTenant.ps1
    # Create-B2CTenant `
    #     -subscriptionId $subscriptionId `
    #     -deployerClientId $deployerClientId `
    #     -tenantId $tenantId `
    #     -resourceGroupName $resourceGroupName `
    #     -b2cTenantName $b2cTenantName

}
Export-ModuleMember Deploy-Infrastructure