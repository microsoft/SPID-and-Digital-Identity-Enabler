# ----------------------------------------------------------------------------------------------
#   Copyright (c) Microsoft Corporation. All rights reserved.
#   Licensed under the MIT License. See License.txt in the project root for license information.
# ----------------------------------------------------------------------------------------------

[CmdletBinding()]
param (
    [Parameter()]
    [string]$SPIDProxyBaseUrl ="Unused for now"
)

$ErrorActionPreference = "Continue"

$json = Get-Content -Path "./Metadatas.json" -Raw | ConvertFrom-Json
$metadatas = $json.metadatas

$oldProtocols = [System.Net.ServicePointManager]::SecurityProtocol
$securityProtocols = [System.Net.SecurityProtocolType]::Tls -bor [System.Net.SecurityProtocolType]::Tls11 -bor [System.Net.SecurityProtocolType]::Tls12
[System.Net.ServicePointManager]::SecurityProtocol = $securityProtocols

$wc = New-Object System.Net.WebClient


foreach ($m in $metadatas) {
    $idpName = $m.providername.ToLower()
    
    $file = "$PWD/$idpName-metadata.xml"
    $wc.DownloadFile($m.providermetadata, $file)
    

    $xml = [xml](Get-Content -raw -Path $file)
    if ($null -eq $xml) {
        Write-Host "Metadata file for $idpName SKIPPED"
        continue
    }
    Write-Host "Metadata file for $idpName downloaded"
    $nsmgr = New-Object System.Xml.XmlNamespaceManager $xml.NameTable
    $nsmgr.AddNamespace('ds', 'http://www.w3.org/2000/09/xmldsig#')
		
    $removeNodes = $xml.SelectNodes("//ds:Signature", $nsmgr)
    $removeNodes += $xml.SelectNodes("//Signature", $nsmgr)
    
		
    foreach ($node in $removeNodes) {
        $node.ParentNode.RemoveChild($node) | Out-Null
    }
    $xml.Save($file)
}