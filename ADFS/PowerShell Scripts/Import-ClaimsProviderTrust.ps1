# ----------------------------------------------------------------------------------------------
#   Copyright (c) Microsoft Corporation. All rights reserved.
#   Licensed under the MIT License. See License.txt in the project root for license information.
# ----------------------------------------------------------------------------------------------

param(
    [Parameter(Mandatory = $true)][string]$ProxyAddress,
    [Parameter(Mandatory = $false)][string]$ClaimsNamespace,
    [Parameter(Mandatory = $false)][string[]]$IgnoreProviders = @(),
    [switch]$DontCreateClaimDescriptions,
    [switch]$DisableAfterCreation,
    [switch]$DontCreateAcceptanceTransformRules,
    [Parameter(Mandatory = $true)][string]$ProxyCertificateFilePath
)
if ($IgnoreProviders -isnot [string[]]) {
    $exc = New-Object System.ArgumentException "IgnoreProviders must be an array of string"
    Throw $exc
}

if (-not $ProxyAddress.StartsWith("https://")) {
    $exc = New-Object System.ArgumentException "ProxyAddress must be an https url"
    Throw $exc
}
if (!$DontCreateClaimDescriptions -or !$DontCreateAcceptanceTransformRules) {
    if (-not $ClaimsNamespace.StartsWith("http")) {
        $exc = New-Object System.ArgumentException "ClaimsNamespace must be an url"
        Throw $exc
    }
}

if(-not (Test-Path -Path $ProxyCertificateFilePath)){
    $exc = New-Object System.ArgumentException "The specified Certificate file doesn't exist"
    Throw $exc
}

$spidProxyCert=New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($ProxyCertificateFilePath)

$ErrorActionPreference = "Stop"
$json = Get-Content -Path "./Metadatas.json" -Raw | ConvertFrom-Json
$metadatas = $json.metadatas

$oldProtocols = [System.Net.ServicePointManager]::SecurityProtocol
$securityProtocols = [System.Net.SecurityProtocolType]::Tls11 -bor [System.Net.SecurityProtocolType]::Tls12
[System.Net.ServicePointManager]::SecurityProtocol = $securityProtocols

$wc = New-Object System.Net.WebClient

foreach ($m in $metadatas) {
    $idpName = $m.providername
    if ($IgnoreProviders -contains $idpName) {
        Write-Host "Skipping importing $idpName provider" -ForegroundColor Yellow
        continue
    }
    
    $file = "$PWD/metadata.xml"
    $wc.DownloadFile($m.providermetadata, $file)
    Write-Host "Metadata file for $idpName downloaded"
    if ($idpName -eq "TIM") {
        $xml = [xml](Get-Content -raw -Path $file)
        $nsmgr = New-Object System.Xml.XmlNamespaceManager $xml.NameTable
		
        $nsmgr.AddNamespace('ds', 'http://www.w3.org/2000/09/xmldsig#')
		
        $removeNodes = $xml.SelectNodes("//ds:Signature", $nsmgr)
        $removeNodes += $xml.SelectNodes("//ds:X509IssuerSerial", $nsmgr)
        $removeNodes += $xml.SelectNodes("//ds:X509SubjectName", $nsmgr)
		
        foreach ($node in $removeNodes) {
            $node.ParentNode.RemoveChild($node)
        }
        $xml.Save($file)
    }
    Add-ADFSClaimsProviderTrust -Name $idpName -MetadataFile $file -RequiredNameIdFormat "urn:oasis:names:tc:SAML:2.0:nameid-format:transient" -SigningCertificateRevocationCheck None -SignedSamlRequestsRequired $true -AllowCreate $false -SamlAuthenticationRequestParameters UrlWithProtocolBinding -SamlAuthenticationRequestProtocolBinding Post
	
    Write-Host "$idpName Claims Provider Added" -ForegroundColor Green
	
    $FullProxyAddres = $ProxyAddress + "/proxy/index/" + $idpName
	
    $OriEndPoints = Get-AdfsClaimsProviderTrust -Name $idpName | Select-Object -ExpandProperty Samlendpoints
	
    $ProxyEndPoint = New-ADFSSamlEndpoint -Binding "Redirect" -Protocol "SAMLSingleSignOn" -Uri $FullProxyAddres
	
    $OriginalLogoutEndpoint = $OriEndPoints | Where-Object { ($_.Protocol -eq 'SAMLLogout') -and ($_.Binding -eq 'Redirect') }
    $ProxyLogoutEndpoint = New-ADFSSamlEndpoint -Binding 'Redirect' -Protocol 'SAMLLogout' -Uri $FullProxyAddres -ResponseUri $OriginalLogoutEndpoint.ResponseLocation
	
    $NewEndPoints = ($OriEndPoints | Where-Object { -not (($_.Protocol -eq 'SAMLSingleSignOn') -and ($_.Binding -eq 'Redirect')) }) + $ProxyEndPoint
    $NewEndPoints = ($NewEndPoints | Where-Object { -not (($_.Protocol -eq 'SAMLLogout') -and ($_.Binding -eq 'Redirect')) }) + $ProxyLogoutEndpoint
	
    $OriginalCerts = @(Get-AdfsClaimsProviderTrust -Name $idpName | Select-Object -ExpandProperty TokenSigningCertificates)
    $NewCerts = $OriginalCerts + $spidProxyCert

    Set-AdfsClaimsProviderTrust -TargetName $idpName -SamlEndpoint $NewEndPoints -TokenSigningCertificate $NewCerts
	
    Write-Host "SAMLSingleSignOn Redirect Endpoint edited for $idpName : $FullProxyAddres" -ForegroundColor Green
    Write-Host "SAMLLogout Redirect Endpoint edited for $idpName : $FullProxyAddres" -ForegroundColor Green
    Write-Host "TokenSigningCertificates edited for $idpName : $NewCerts" -ForegroundColor Green
        
    if (!$DontCreateAcceptanceTransformRules) {
        $RuleFilePath = "./Base_TransformRules.txt"
        if (test-path($RuleFilePath)) {
            $RuleFileContent = Get-Content $RuleFilePath
            $RuleFileContent = $RuleFileContent -replace '#\{ClaimTypeNamespace\}#', $ClaimsNamespace
            $RuleFileContent = $RuleFileContent -replace '#\{IdpIssuer\}#', $idpName
            $RuleFileContent | Set-Content 'temp_transform_rule.txt' -Force
            $TempRuleFile = Get-Item 'temp_transform_rule.txt'
            Set-AdfsClaimsProviderTrust -TargetName $idpName -AcceptanceTransformRulesFile $TempRuleFile.FullName 
		
            $TempRuleFile | Remove-Item -Force
            Write-Host "ClaimsTransformRule imported for $idpName" -ForegroundColor Green
        }
        else {
            Write-Warning "!!!! Rule File $RuleFilePath doesn't exist !!!!"  
        }
    }
    if ($DisableAfterCreation) {
        Disable-AdfsClaimsProviderTrust -TargetName $idpName
        Write-Host "$idpName Disabled"
    }
}#end foreach
$wc.Dispose();
[System.Net.ServicePointManager]::SecurityProtocol = $oldProtocols
if (Test-Path -Path $file) {
    Remove-Item -Path $file -Force
}

if ($DontCreateClaimDescriptions) {
    EXIT
}
Write-Host "Creating Claims Description"

Add-ADFSClaimDescription -Name SPIDCodiceFiscale -ShortName spidcodicefiscale -ClaimType $ClaimsNamespace/fiscalnumber
Add-ADFSClaimDescription -Name SPIDSesso -ShortName spidsesso -ClaimType $ClaimsNamespace/gender
Add-ADFSClaimDescription -Name SPIDDataNascita -ShortName spiddatanascita -ClaimType $ClaimsNamespace/dateofbirth
Add-ADFSClaimDescription -Name SPIDLuogoNascita -ShortName spidluogonascita -ClaimType $ClaimsNamespace/placeofbirth
Add-ADFSClaimDescription -Name SPIDProvinciaNascita -ShortName spidprovincianascita -ClaimType $ClaimsNamespace/countyofbirth
Add-ADFSClaimDescription -Name SPIDDocumentoIdentita -ShortName spiddocidentita -ClaimType $ClaimsNamespace/idcard
Add-ADFSClaimDescription -Name SPIDIndirizzoResidenza -ShortName spidindirizzoresidenza -ClaimType $ClaimsNamespace/address
Add-ADFSClaimDescription -Name SPIDDomicilioDigitale -ShortName spiddomiciliodigitale -ClaimType $ClaimsNamespace/digitaladdress
Add-ADFSClaimDescription -Name SPIDScadenzaIdentita -ShortName spidscadenzaidentita -ClaimType $ClaimsNamespace/expirationdate
Add-ADFSClaimDescription -Name SPIDNumeroCellulare -ShortName spidnumerocell -ClaimType $ClaimsNamespace/mobilephone
Add-ADFSClaimDescription -Name SPIDNomeAzienda -ShortName spidnomeazienda -ClaimType $ClaimsNamespace/companyname
Add-ADFSClaimDescription -Name SPIDSedeLegale -ShortName spidsedelegale -ClaimType $ClaimsNamespace/registeredoffice
Add-ADFSClaimDescription -Name SPIDPartitaIVA -ShortName spidpartitaiva -ClaimType $ClaimsNamespace/ivacode
Add-ADFSClaimDescription -Name SPIDAccount -ShortName spidaccount -ClaimType $ClaimsNamespace/spidaccount
Add-ADFSClaimDescription -Name SPIDIssuer -ShortName spidissuer -ClaimType $ClaimsNamespace/spidissuer

Write-Host "Claims Description created" -ForegroundColor Green