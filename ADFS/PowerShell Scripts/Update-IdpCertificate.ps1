# ----------------------------------------------------------------------------------------------
#   Copyright (c) Microsoft Corporation. All rights reserved.
#   Licensed under the MIT License. See License.txt in the project root for license information.
# ----------------------------------------------------------------------------------------------

param(
    [Parameter(Mandatory = $true)][string]$MetadataJsonPath
)

$json = Get-Content -Path "$MetadataJsonPath" -Raw | ConvertFrom-Json
$metadatas = $json.metadatas
$oldProtocols = [System.Net.ServicePointManager]::SecurityProtocol
$securityProtocols = [System.Net.SecurityProtocolType]::Tls11 -bor [System.Net.SecurityProtocolType]::Tls12
[System.Net.ServicePointManager]::SecurityProtocol = $securityProtocols

foreach($m in $metadatas){
    $r = Invoke-WebRequest -Uri "$($m.providermetadata)" -UseBasicParsing
    if(-not ($r.StatusCode -eq 200)){
        Write-Host "Unable to retrieve metadata for $($m.providername). Skipping..."
        continue
    }
    Write-Host "Downloaded metadata..."
    $xml = [xml]$r.Content
    $signingCerts = $xml.EntityDescriptor.IDPSSODescriptor.KeyDescriptor | Where-Object {$_.use -eq "signing"}
    $x509s = $signingCerts.KeyInfo.X509Data.X509Certificate
    
    $certs = @()
    foreach($c in $x509s){
        $bytes = [System.Convert]::FromBase64String($c)
        $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2 -ArgumentList @(,$bytes)
        $certs += $cert
    }

    if($certs.Length -lt 1){
        Write-Host "No certs found in $($m.providername) metadata"
        continue
    }

    Set-AdfsClaimsProviderTrust -TargetName $m.providername -TokenSigningCertificate $certs
    Write-Host "Certificate updated for $($m.providername)"
}


[System.Net.ServicePointManager]::SecurityProtocol = $oldProtocols