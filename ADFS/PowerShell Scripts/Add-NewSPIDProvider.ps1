# ----------------------------------------------------------------------------------------------
#   Copyright (c) Microsoft Corporation. All rights reserved.
#   Licensed under the MIT License. See License.txt in the project root for license information.
# ----------------------------------------------------------------------------------------------

param(
    [string]$ThemeName="SPIDTheme",
    [Parameter(Mandatory=$true)][string]$Path
)

if( -not (Test-Path -Path $Path\$ThemeName)){
    mkdir "$Path\$ThemeName" | Out-Null
}
Export-AdfsWebTheme -Name $ThemeName -DirectoryPath "$Path\$ThemeName"
$oldImages = Get-ChildItem -Path "$Path\$ThemeName\images\idp" -File | Select -ExpandProperty Name
Read-Host "Copy the images in $Path\$ThemeName\images\idp and Hit Enter...."
$newImages = Get-ChildItem -Path "$Path\$ThemeName\images\idp" -File

$newIdps = ""
foreach($img in $newImages){
    if($img -notin $oldImages){
        Set-AdfsWebTheme -TargetName $ThemeName -AdditionalFileResource @{Uri = "/adfs/portal/images/idp/$($img.Name)"; path = "$($img.FullName)"}
        $newIdps += ", `"$($img.BaseName)`""
    }
}

$onload = Get-Content -Path "$Path\$ThemeName\script\onload.js"
$onload = $onload -replace "(var idpsNames)(.*)(];)", "`$1`$2$newIdps`$3"
$onload | Set-Content -Path "$Path\$ThemeName\script\onload.js" -Force
Set-AdfsWebTheme -TargetName $ThemeName -AdditionalFileResource @{Uri = "/adfs/portal/script/onload.js"; path = "$Path\$ThemeName\script\onload.js"}
