# ----------------------------------------------------------------------------------------------
#   Copyright (c) Microsoft Corporation. All rights reserved.
#   Licensed under the MIT License. See License.txt in the project root for license information.
# ----------------------------------------------------------------------------------------------

param(
    [Parameter(Mandatory = $true)][string]$MetadataJsonPath,
    [Parameter(Mandatory = $true)][string]$UpdateIdpScriptPath,
    [int32]$MaxResultCount=10    
)
# Change these three variables to whatever you want
$jobname = "Update SPID IdP certificates"
$argumentlist =  @("$MetadataJsonPath")
$repeat = (New-TimeSpan -Minutes 15)
$cred = Get-Credential -Message "Enter credentials for scheduled jobs (must have administrator privileges)"

$trigger = New-JobTrigger -Once -At (Get-Date).Date -RepeatIndefinitely -RepetitionInterval $repeat
$options = New-ScheduledJobOption -RunElevated -ContinueIfGoingOnBattery -StartIfOnBattery -RequireNetwork
Register-ScheduledJob -Credential $cred -FilePath $UpdateIdpScriptPath -ArgumentList $argumentlist -Name $jobname -Trigger $trigger -ScheduledJobOption $options -MaxResultCount $MaxResultCount