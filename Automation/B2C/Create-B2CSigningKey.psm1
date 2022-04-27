# ----------------------------------------------------------------------------------------------
#   Copyright (c) Microsoft Corporation. All rights reserved.
#   Licensed under the MIT License. See License.txt in the project root for license information.
# ----------------------------------------------------------------------------------------------

function Create-B2CSigningKey {
    Param(
        [Parameter(Mandatory=$true)]
        [String]$tenant,
        [Parameter(Mandatory=$true)]
        [String]$accessToken
    )
    
    $certificateResult = New-SelfSignedCertificate `
        -KeyExportPolicy Exportable `
        -Subject "CN=$tenant.onmicrosoft.com" `
        -KeyAlgorithm RSA `
        -KeyLength 2048 `
        -KeyUsage DigitalSignature `
        -NotAfter (Get-Date).AddMonths(12) `
        -CertStoreLocation "Cert:\CurrentUser\My"

    $certPassword = ConvertTo-SecureString -String "1234" -Force -AsPlainText

    $certificates = Get-ChildItem -Path "Cert:\CurrentUser\My\$($certificateResult.Thumbprint)" 
    Export-PfxCertificate `
        -Cert $certificates[0] `
        -FilePath "$tenant.onmicrosoft.com.pfx" `
        -Password $certPassword

    $policyKeyName = "B2C_1A_SigningKey"
    $headers = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
    $headers.Add("Content-Type", 'application/json')
    $headers.Add("Authorization", 'Bearer ' + $accessToken)
    
    $apiUrl = "https://graph.microsoft.com/beta/trustFramework/keySets"
    $body = @{
        "id"=$policyKeyName
    } | ConvertTo-Json
    Invoke-RestMethod `
        -Uri $apiUrl `
        -Method POST `
        -Headers $headers `
        -Body $body

    $fileContentBytes = Get-Content "$tenant.onmicrosoft.com.pfx" -Encoding Byte
    $encodeCertificate = [System.Convert]::ToBase64String($fileContentBytes)

    $apiurl = "https://graph.microsoft.com/beta/trustFramework/keySets/$policyKeyName/uploadPkcs12"
    $body = @{
        "key" = $encodeCertificate
        "password" = "1234"
    } | ConvertTo-Json
    Invoke-RestMethod `
        -Uri $apiurl `
        -Method POST `
        -Headers $headers `
        -Body $body
}
Export-ModuleMember Create-B2CSigningKey