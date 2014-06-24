param(
    [Parameter(Mandatory=$true)]
    [string]$ConfigurationName,
    
    [Parameter(Mandatory=$true)]
    [string]$ProjectName,
    
    [Parameter(Mandatory=$true)]
    [string]$ProjectDir,
    
    [Parameter(Mandatory=$true)]
    [string]$TargetDir,
    
    [Parameter(Mandatory=$true)]
    [string]$TargetFileName,

    [string]$DestinationDirectory = "M:\deploy"
    )

Write-Output $ConfigurationName
Write-Output $ProjectDir
Write-Output $ProjectName
Write-Output $TargetDir
Write-Output $TargetFileName




$validConfigs = @("Release-Deploy", "Debug-Deploy")

if (!$validConfigs.Contains("$ConfigurationName"))
{
    Write-Output "Skipping because $ConfigurationName is not marked for deploy"
    Exit
}

$DestinationDirectory = Join-Path "$DestinationDirectory" "$ProjectName"

if (-not $(Test-Path "$DestinationDirectory"))
{
    mkdir "$DestinationDirectory"
    if (-not (Test-Path "$DestinationDirectory"))
    {
        Write-Output "Could not create $DestinationDirectory"
        Exit 1
    }
}
else 
{
    Get-ChildItem "$DestinationDirectory"/ -Recuse | Remove-Item -Force
}

Copy-Item -Path "$TargetDir\*"  -Destination "$DestinationDirectory"

$AppConfig = Join-Path "$ProjectDir" "App.config"
$AppConfigTarget = Join-Path "$DestinationDirectory" "$TargetFileName.config"
Copy-Item -Path "$AppConfig" -Destination "$AppConfigTarget"
