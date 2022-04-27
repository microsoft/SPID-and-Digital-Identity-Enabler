# ----------------------------------------------------------------------------------------------
#   Copyright (c) Microsoft Corporation. All rights reserved.
#   Licensed under the MIT License. See License.txt in the project root for license information.
# ----------------------------------------------------------------------------------------------

# Taken from https://github.com/azure-ad-b2c/vscode-extension/blob/master/scripts/Build-CustomPolicies.ps1
function Build-B2CPolicies {
    Param(
        [Parameter(Mandatory = $true)]
        [string]$b2cFolderLocation
    )

    try{
        #Check if appsettings.json is existed under for root folder        
        $AppSettingsFile = Join-Path $b2cFolderLocation "appsettings.json"
    
        #Create app settings file with default values
        $AppSettingsJson = Get-Content -Raw -Path $AppSettingsFile | ConvertFrom-Json
    
        #Read all policy files from the root directory            
        $XmlPolicyFiles = Get-ChildItem -Path $b2cFolderLocation -Filter *.xml
        Write-Verbose "Files found: $XmlPolicyFiles"
    
        #Get the app settings                        
        $EnvironmentsRootPath = Join-Path $b2cFolderLocation "Environments"
    
        #Ensure environments folder exists
        if((Test-Path -Path $EnvironmentsRootPath -PathType Container) -ne $true)
        {
            New-Item -ItemType Directory -Force -Path $EnvironmentsRootPath | Out-Null
        }                                    
    
        #Iterate through environments  
        foreach($entry in $AppSettingsJson.Environments)
        {
            Write-Verbose "ENVIRONMENT: $($entry.Name)"
    
            if($null -eq $entry.PolicySettings){
                Write-Error "Can't generate '$($entry.Name)' environment policies. Error: Accepted PolicySettings element is missing. You may use old version of the appSettings.json file. For more information, see [App Settings](https://github.com/yoelhor/aad-b2c-vs-code-extension/blob/master/README.md#app-settings)"
            }
            else {
                $environmentRootPath = Join-Path $EnvironmentsRootPath $entry.Name
    
                if((Test-Path -Path $environmentRootPath -PathType Container) -ne $true)
                {
                    New-Item -ItemType Directory -Force -Path $environmentRootPath | Out-Null
                }    
    
                #Iterate through the list of settings
                foreach($file in $XmlPolicyFiles)
                {
                    Write-Verbose "FILE: $($entry.Name) - $file"
    
                    $policContent = Get-Content (Join-Path $b2cFolderLocation $file) | Out-String
    
                    #Replace the tenant name
                    $policContent = $policContent -replace "\{Settings:Tenant\}", $entry.Tenant
    
                    #Replace the rest of the policy settings
                    $policySettingsHash = @{}; #ugly hash conversion from psobject so we can access json properties via key
                    $entry.PolicySettings.psobject.properties | ForEach-Object{ $policySettingsHash[$_.Name] = $_.Value }
                    foreach($key in $policySettingsHash.Keys)
                    {
                        Write-Verbose "KEY: $key VALUE: $($policySettingsHash[$key])"
                        $policContent = $policContent -replace "\{Settings:$($key)\}", $policySettingsHash[$key]
                    }
    
                    #Save the  policy
                    $policContent | Set-Content ( Join-Path $environmentRootPath $file )            
                }
            }
    
            Write-Output "You policies successfully exported and stored under the Environment folder ($($entry.Name))."
        }
    }
    catch{
        Write-Error $_
    }

}

function Upload-B2CPolicies {
    Param(
        [Parameter(Mandatory = $true)]
        [string[]]$policyFiles,
        [Parameter(Mandatory = $true)]
        [string]$accessToken
    )

    $policiesToBeUploaded = @()

    foreach($policyFile in $policyFiles) {
        $policiesToBeUploaded = Queue-B2CPolicies -policiesToBeUploaded $policiesToBeUploaded -policyFile $policyFile
    }

    foreach($policy in $policiesToBeUploaded) {
        Upload-B2CPolicy -policy $policy -accessToken $accessToken
    }
}

function Queue-B2CPolicies {
    Param(
        [string[]]$policiesToBeUploaded,
        [Parameter(Mandatory = $true)]
        [string]$policyFile
    )

    $basePolicyId = Get-PolicyBaseId -policyFile $policyFile
        
    if($null -ne $basePolicyId) {
        $basePolicyFile = Get-PolicyFileFromId -policyId $basePolicyId
        if(-not $policiesToBeUploaded.Contains($basePolicyFile)) {
            $policiesToBeUploaded = Queue-B2CPolicies -policiesToBeUploaded $policiesToBeUploaded -policyFile $basePolicyFile
        }
    }
        
    if(-not $policiesToBeUploaded.Contains($policyFile)) {
        $policiesToBeUploaded += $policyFile
    }

    return $policiesToBeUploaded
}

function Upload-B2CPolicy {
    Param(
        [Parameter(Mandatory = $true)]
        [string]$policyFile,
        [Parameter(Mandatory = $true)]
        [string]$accessToken
    )

    $policyId = Get-PolicyId -policyFile $policyFile
    $apiUrl = "https://graph.microsoft.com/beta/trustFramework/policies/$policyId/`$value"
    $headers = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
    $headers.Add("Content-Type", 'application/xml')
    $headers.Add("Authorization", 'Bearer ' + $accessToken)
    $body = Get-Content ".\Environments\SPID\$policyFile"

    Invoke-RestMethod -Uri $apiUrl -Method Put -Headers $headers -Body $body
}

function Get-PolicyFileFromId {
    Param(
        [Parameter(Mandatory = $true)]
        [string]$policyId
    )

    $policyName = $policyId -replace "B2C_1A_", ""
    $policyName = $policyName + ".xml"

    return $policyName
}

function Get-PolicyFiles {
    $builtPolicies = Get-ChildItem -Path ".\Environments\SPID"

    return $builtPolicies
}

function Get-PolicyId {
    Param(
        [Parameter(Mandatory = $true)]
        [string]$policyFile
    )

    [xml]$policyInfo = Get-Content -Path ".\Environments\SPID\$policyFile"
    
    return $policyInfo.TrustFrameworkPolicy.PolicyId
}

function Get-PolicyBaseId {
    Param(
        [Parameter(Mandatory = $true)]
        [string]$policyFile
    )
    
    [xml]$policyInfo = Get-Content -Path ".\Environments\SPID\$policyFile"
    
    return $policyInfo.TrustFrameworkPolicy.BasePolicy.PolicyId
}

function Set-EnvironmentConfig {
    Param(
        [Parameter(Mandatory = $true)]
        [string]$b2cFolderLocation,
        [Parameter(Mandatory = $true)]
        [string]$tenant,
        [Parameter(Mandatory = $true)]
        [string]$identityExperienceFrameworkAppId,
        [Parameter(Mandatory = $true)]
        [string]$proxyIdentityExperienceFrameworkAppId,
        [Parameter(Mandatory = $true)]
        [string]$metadatasBlobStorageUrl,
        [Parameter(Mandatory = $true)]
        [string]$customUiBlobStorageUrl,
        [Parameter(Mandatory = $true)]
        [string]$appInsightsKey
    )

    [string]$b2cAppSettings = 'appsettings.json'
    [string]$content = Get-Content -Raw -Path $b2cAppSettings
    [PSCustomObject]$b2cSettingsObject = $content | ConvertFrom-Json

    $b2cSettingsObject.PoliciesFolder = $b2cFolderLocation
    $b2cSettingsObject.Environments[0].Name = "SPID"
    $b2cSettingsObject.Environments[0].Production = $true
    $b2cSettingsObject.Environments[0].Tenant = "$tenant.onmicrosoft.com"
    $b2cSettingsObject.Environments[0].PolicySettings.IdentityExperienceFrameworkAppId = $identityExperienceFrameworkAppId
    $b2cSettingsObject.Environments[0].PolicySettings.ProxyIdentityExperienceFrameworkAppId = $proxyIdentityExperienceFrameworkAppId
    $b2cSettingsObject.Environments[0].PolicySettings.MetadatasBlobStorageUrl = $metadatasBlobStorageUrl
    $b2cSettingsObject.Environments[0].PolicySettings.CustomUiBlobStorageUrl = $customUiBlobStorageUrl
    $b2cSettingsObject.Environments[0].PolicySettings.AppInsightsKey = $appInsightsKey

    $b2cSettingsObject | ConvertTo-Json -Depth 50 | Set-Content $b2cAppSettings
}

function SetUp-B2CPolicies {
    Param(
        [Parameter(Mandatory = $true)]
        [string]$b2cFolderLocation,
        [Parameter(Mandatory = $true)]
        [string]$tenant,
        [Parameter(Mandatory = $true)]
        [string]$identityExperienceFrameworkAppId,
        [Parameter(Mandatory = $true)]
        [string]$proxyIdentityExperienceFrameworkAppId,
        [Parameter(Mandatory = $true)]
        [string]$metadatasBlobStorageUrl,
        [Parameter(Mandatory = $true)]
        [string]$customUiBlobStorageUrl,
        [Parameter(Mandatory = $true)]
        [string]$appInsightsKey,
        [Parameter(Mandatory = $true)]
        [string]$accessToken
    )

    Set-Location $b2cFolderLocation

    Set-EnvironmentConfig `
        -b2cFolderLocation $b2cFolderLocation `
        -tenant $tenant `
        -identityExperienceFrameworkAppId $identityExperienceFrameworkAppId `
        -proxyIdentityExperienceFrameworkAppId $proxyIdentityExperienceFrameworkAppId `
        -metadatasBlobStorageUrl $metadatasBlobStorageUrl `
        -customUiBlobStorageUrl $customUiBlobStorageUrl `
        -appInsightsKey $appInsightsKey

    Build-B2CPolicies `
        -b2cFolderLocation $b2cFolderLocation

    $policyFiles = Get-PolicyFiles

    Upload-B2CPolicies `
        -policyFiles $policyFiles `
        -accessToken $accessToken
}
Export-ModuleMember  SetUp-B2CPolicies