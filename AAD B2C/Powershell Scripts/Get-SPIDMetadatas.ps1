# ----------------------------------------------------------------------------------------------
#   Copyright (c) Microsoft Corporation. All rights reserved.
#   Licensed under the MIT License. See License.txt in the project root for license information.
# ----------------------------------------------------------------------------------------------

[CmdletBinding()]
param (
    [Parameter(Mandatory = $true, HelpMessage = 'Base URL of SPID Proxy application')]
    [Alias('s')]
    [string]$SPIDProxyBaseUrl,

    [Parameter(HelpMessage = 'Base URL of secondary SPID Proxy application')]
    [Alias('a')]
    [string]$additionalSPIDProxyBaseUrl,

    [Parameter(HelpMessage = 'Path to .CER file. If omitted, a .CER file is sought in current directory')]
    [Alias('c')]
    [string]$certificateFilePath
)

# $VerbosePreference = 'Continue'
$metadataFolder = 'metadatas'

if([string]::IsNullOrWhiteSpace($certificateFilePath)){
    $f = Get-ChildItem '*.cer'
    if($f.Count -ne 1){
        Write-Error "File .CER not found"
        break
    }
    $certificateFilePath = $f.FullName
}

# $cert = (New-Object System.IO.StreamReader($certificateFilePath)).ReadToEnd()
$cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2 $certificateFilePath
$certBase64 = [System.Convert]::ToBase64String($cert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert))

# Write-Verbose $certBase64

if(!(Test-Path $metadataFolder)){
    New-Item -ItemType Directory $metadataFolder | Out-Null
}

$ErrorActionPreference = 'Continue'

$json = Get-Content -Path ".\Metadatas.json" -Raw | ConvertFrom-Json
$metadatas = $json.metadatas

# $oldProtocols = [System.Net.ServicePointManager]::SecurityProtocol
$securityProtocols = [System.Net.SecurityProtocolType]::Tls -bor [System.Net.SecurityProtocolType]::Tls11 -bor [System.Net.SecurityProtocolType]::Tls12
[System.Net.ServicePointManager]::SecurityProtocol = $securityProtocols

foreach ($m in $metadatas) {
    $idpName = $m.providername.ToLower()
    $supportsLegalSubject = ($m.supportsLegalSubject -eq $true) -and !([string]::IsNullOrWhiteSpace($additionalSPIDProxyBaseUrl))
    
    $file = "$PWD/$idpName-download.xml"
    Invoke-WebRequest -Uri $m.providermetadata -OutFile $file -Headers @{"User-Agent"="SPIDProxy/8.0.0"}

    $xml = [xml](Get-Content -Raw -Path $file)
    if ($null -eq $xml) {
        Write-Verbose "Metadata file for '$idpName' SKIPPED"
        continue
    }
    Write-Verbose "Metadata file for '$idpName' downloaded"

    foreach($isPG in ($false, $true)){

        if(!$isPG -or $supportsLegalSubject){

            $xml = [xml](Get-Content -Raw -Path $file)

            $nsmgr = New-Object System.Xml.XmlNamespaceManager $xml.NameTable
            $nsmgr.AddNamespace('ds', 'http://www.w3.org/2000/09/xmldsig#')
            $nsmgr.AddNamespace('md', 'urn:oasis:names:tc:SAML:2.0:metadata')
        
            $removeNodes = $xml.SelectNodes('//ds:Signature', $nsmgr)
            $removeNodes += $xml.SelectNodes('//Signature', $nsmgr)
            $removeNodes += $xml.SelectNodes('//md:SingleLogoutService', $nsmgr)
            $removeNodes += $xml.SelectNodes('//md:SingleSignOnService', $nsmgr)
                
            foreach ($node in $removeNodes) {
                $node.ParentNode.RemoveChild($node) | Out-Null
            }
        
            $IDPSSODescriptor = $xml.SelectNodes('/md:EntityDescriptor/md:IDPSSODescriptor', $nsmgr)
        
            $kd = $xml.CreateElement('md', 'KeyDescriptor', 'urn:oasis:names:tc:SAML:2.0:metadata')
            $kd.SetAttribute('use', 'signing')
            $ki = $kd.AppendChild($xml.CreateElement('ds:KeyInfo', 'http://www.w3.org/2000/09/xmldsig#'))
            $x509Data = $ki.AppendChild($xml.CreateElement('ds:X509Data', 'http://www.w3.org/2000/09/xmldsig#'))
            $x509Cert = $x509Data.AppendChild($xml.CreateElement('ds:X509Certificate', 'http://www.w3.org/2000/09/xmldsig#'))
            # $x509Cert.AppendChild($xml.CreateTextNode($cert)) | Out-Null
            $x509Cert.InnerText = $certBase64
        
            $KeyDescriptors = $IDPSSODescriptor.SelectNodes('md:KeyDescriptor', $nsmgr)
            if($null -ne $KeyDescriptors){
                $IDPSSODescriptor.InsertAfter($kd, $KeyDescriptors[$KeyDescriptors.Count - 1]) | Out-Null
            }
            else{
                $IDPSSODescriptor.AppendChild($kd) | Out-Null
            }
        
            $e = $xml.CreateElement('md', 'SingleLogoutService', 'urn:oasis:names:tc:SAML:2.0:metadata')
            $e.SetAttribute('Binding', 'urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect')
            if($isPG){
                $e.SetAttribute('Location', "$($additionalSPIDProxyBaseUrl)/proxy/index/$($idpName)_logout")
            }
            else{
                $e.SetAttribute('Location', "$($SPIDProxyBaseUrl)/proxy/index/$($idpName)_logout")
            }
            # $IDPSSODescriptor.AppendChild($e) | Out-Null
            $IDPSSODescriptor.InsertAfter($e, $kd) | Out-Null
        
            $e = $xml.CreateElement('md', 'SingleSignOnService', 'urn:oasis:names:tc:SAML:2.0:metadata')
            $e.SetAttribute('Binding', 'urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect')
            if($isPG){
                $e.SetAttribute('Location', "$($additionalSPIDProxyBaseUrl)/proxy/index/$($idpName)")
            }
            else{
                $e.SetAttribute('Location', "$($SPIDProxyBaseUrl)/proxy/index/$($idpName)")
            }
            $IDPSSODescriptor.InsertAfter($e, $kd) | Out-Null
        
            if($isPG){
                $xml.Save(".\$metadataFolder\$($idpName)pg-metadata.xml")
            }
            else{
                $xml.Save(".\$metadataFolder\$($idpName)-metadata.xml")
            }
        }
    }
}
