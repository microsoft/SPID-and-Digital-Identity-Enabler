# ----------------------------------------------------------------------------------------------
#   Copyright (c) Microsoft Corporation. All rights reserved.
#   Licensed under the MIT License. See License.txt in the project root for license information.
# ----------------------------------------------------------------------------------------------

Param(
    [Parameter(Mandatory = $true)][string]$BasePath,
	[switch]$CustomImages,
	[string]$IllustrationFileName="illustration.png",
	[string]$LogoFileName="logo.png",
    [string]$ThemeName = "SPIDTheme",
	[switch]$SetActive,
	[string]$ThemeSourceName="default"
)

New-AdfsWebTheme -Name $ThemeName -SourceName $ThemeSourceName

Set-AdfsWebTheme -TargetName $ThemeName -AdditionalFileResource @{Uri = "/adfs/portal/images/idp/spid-ico-circle-bb.png"; path = "$BasePath\images\idp\spid-ico-circle-bb.png"}
Set-AdfsWebTheme -TargetName $ThemeName -AdditionalFileResource @{Uri = "/adfs/portal/images/idp/Infocert.png"; path = "$BasePath\images\idp\Infocert.png"}
Set-AdfsWebTheme -TargetName $ThemeName -AdditionalFileResource @{Uri = "/adfs/portal/images/idp/Postecom.png"; path = "$BasePath\images\idp\Postecom.png"}
Set-AdfsWebTheme -TargetName $ThemeName -AdditionalFileResource @{Uri = "/adfs/portal/images/idp/Sielte.png"; path = "$BasePath\images\idp\Sielte.png"}
Set-AdfsWebTheme -TargetName $ThemeName -AdditionalFileResource @{Uri = "/adfs/portal/images/idp/TIM.png"; path = "$BasePath\images\idp\TIM.png"}
Set-AdfsWebTheme -TargetName $ThemeName -AdditionalFileResource @{Uri = "/adfs/portal/images/idp/Aruba.png"; path = "$BasePath\images\idp\Aruba.png"}
Set-AdfsWebTheme -TargetName $ThemeName -AdditionalFileResource @{Uri = "/adfs/portal/images/idp/Namirial.png"; path = "$BasePath\images\idp\Namirial.png"}
Set-AdfsWebTheme -TargetName $ThemeName -AdditionalFileResource @{Uri = "/adfs/portal/images/idp/Register.png"; path = "$BasePath\images\idp\Register.png"}
Set-AdfsWebTheme -TargetName $ThemeName -AdditionalFileResource @{Uri = "/adfs/portal/images/idp/Intesa.png"; path = "$BasePath\images\idp\Intesa.png"}
Set-AdfsWebTheme -TargetName $ThemeName -AdditionalFileResource @{Uri = "/adfs/portal/images/idp/Lepida.png"; path = "$BasePath\images\idp\Lepida.png"}
Set-AdfsWebTheme -TargetName $ThemeName -AdditionalFileResource @{Uri = "/adfs/portal/images/idp/SPIDValidator.png"; path = "$BasePath\images\idp\SPIDValidator.png"}
Set-AdfsWebTheme -TargetName $ThemeName -AdditionalFileResource @{Uri = "/adfs/portal/images/idp/TeamSystem.png"; path = "$BasePath\images\idp\TeamSystem.png"}
Set-AdfsWebTheme -TargetName $ThemeName -AdditionalFileResource @{Uri = "/adfs/portal/images/idp/CIE.png"; path = "$BasePath\images\idp\CIE.png"}
Set-AdfsWebTheme -TargetName $ThemeName -AdditionalFileResource @{Uri = "/adfs/portal/images/idp/EIDAS.png"; path = "$BasePath\images\idp\EIDAS.png"}
Set-AdfsWebTheme -TargetName $ThemeName -AdditionalFileResource @{Uri = "/adfs/portal/images/idp/EtnaHitech.png"; path = "$BasePath\images\idp\EtnaHitech.png"}
Set-AdfsWebTheme -TargetName $ThemeName -AdditionalFileResource @{Uri = "/adfs/portal/images/idp/Infocamere.png"; path = "$BasePath\images\idp\Infocamere.png"}
Set-AdfsWebTheme -TargetName $ThemeName -AdditionalFileResource @{Uri = "/adfs/portal/images/idp/IntesiGroup.png"; path = "$BasePath\images\idp\IntesiGroup.png"}


Set-AdfsWebTheme -TargetName $ThemeName -AdditionalFileResource @{Uri = "/adfs/portal/images/spid-agid-logo-lb.png"; path = "$BasePath\images\spid-agid-logo-lb.png"}


Set-AdfsWebTheme -TargetName $ThemeName -AdditionalFileResource @{Uri = "/adfs/portal/script/onload.js"; path = "$BasePath\script\onload.js"} 
Set-AdfsWebTheme -TargetName $ThemeName -StyleSheet @{Locale = ""; Path = "$BasePath\css\style.css"}

if ($CustomImages) {
    Set-AdfsWebTheme -TargetName $ThemeName -Illustration @{path = "$BasePath\$IllustrationFileName"}
    Set-AdfsWebTheme -TargetName $ThemeName -Logo @{Locale = ""; path = "$BasePath\$LogoFileName"}
}
if ($SetActive) {
    Set-AdfsWebConfig -ActiveThemeName $ThemeName
}
