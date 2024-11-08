#Requires -Version 7.0


$ErrorActionPreference = 'Stop'

. (Join-Path -Path $PSScriptRoot -ChildPath 'Build.Properties.ps1')

if ([string]::IsNullOrWhiteSpace($BUILD_CONFIGURATION_FOLDER_PATH)) {
    exit 1
}

$buildPackagesFilePath = Join-Path -Path $BUILD_CONFIGURATION_FOLDER_PATH -ChildPath 'BuildPackages.ps1'

& $buildPackagesFilePath @args
exit $LASTEXITCODE
