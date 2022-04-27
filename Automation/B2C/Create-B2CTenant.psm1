# ----------------------------------------------------------------------------------------------
#   Copyright (c) Microsoft Corporation. All rights reserved.
#   Licensed under the MIT License. See License.txt in the project root for license information.
# ----------------------------------------------------------------------------------------------

function Create-B2CTenant {
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

    $DeviceCodeRequestParams = @{
        Method = 'POST'
        Uri    = "https://login.microsoftonline.com/$tenantId/oauth2/v2.0/devicecode"
        Body   = @{
            client_id       = $deployerClientId
            scope           = "https://management.azure.com/user_impersonation"
        }
    }
    $DeviceCodeRequest = Invoke-RestMethod @DeviceCodeRequestParams
    Write-Host $DeviceCodeRequest.message -ForegroundColor Yellow
    
    Read-Host
    
    $TokenRequestParams = @{
        Method = 'POST'
        Uri    = "https://login.microsoftonline.com/$tenantId/oauth2/v2.0/token"
        Body   = @{
            client_id       = $deployerClientId
            grant_type      = "urn:ietf:params:oauth:grant-type:device_code"
            device_code     = $DeviceCodeRequest.device_code
        }
    }
    $TokenRequest = Invoke-RestMethod @TokenRequestParams
    $accessToken = $TokenRequest.access_token
    Write-Host $accessToken

    $b2cCreationRequestParams = @{
        Method  = 'PUT'
        Uri     = "https://management.azure.com/subscriptions/$subscriptionId/resourceGroups/$resourceGroupName/providers/Microsoft.AzureActiveDirectory/b2cDirectories/$($b2cTenantName)?api-version=2019-01-01-preview"
        Headers = @{
            'Content-Type' = 'application/json'
            'Authorization' = 'Bearer ' + $accessToken
        }
    }
    $b2cCreationRequestPayload = @{
        "location" = "europe"
        "properties" = @{
            "createTenantProperties" = @{
                "displayName" = "SPID Proxy Tommmaso"
                "countryCode" = "IT"
            }
        }
        "sku" = @{
            "name" = "Premium"
            "tier" = "P1"
        }
    }
    $b2cCreationRequest = Invoke-RestMethod @b2cCreationRequestParams -Body ($b2cCreationRequestPayload | ConvertTo-Json)
    Write-Host "Created B2C tenant with id: $($b2cCreationRequest.Id)"
}
Export-ModuleMember Create-B2CTenant