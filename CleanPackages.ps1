#Requires -Version 7.0


$ErrorActionPreference = 'Stop'

# ================
# EXIT CODES
# ================
# 0 = Success.
# 1 = Missing required build infrastructure.
# 2 = Failed to delete artifacts folder.

. (Join-Path -Path $PSScriptRoot -ChildPath 'Build.Properties.ps1')

if ([string]::IsNullOrWhiteSpace($ARTIFACTS_FOLDER_PATH)) {
    exit 1
}

try {
    # dotnet clean $args[0] --configuration Debug
    # dotnet clean $args[0] --configuration Release

    if (Test-Path -LiteralPath $ARTIFACTS_FOLDER_PATH) {
        Write-Host "Deleting `"$ARTIFACTS_FOLDER_PATH`" folder..."

        Remove-Item -LiteralPath $ARTIFACTS_FOLDER_PATH -Recurse -Force
    }

    exit 0
}
catch {
    Write-Error $_
    exit 2
}
