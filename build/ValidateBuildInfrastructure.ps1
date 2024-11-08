#Requires -Version 7.0


# ================
# EXIT CODES
# ================
# 0 = Success.
# 1 = Missing required build infrastructure.
# 2 = Invalid command-line argument.
# 3 = Invalid build infrastructure configuration.
# 4 = Build infrastructure validation failed.


param(
    [string] $WorkspaceFolderPath = (Split-Path -Parent $PSScriptRoot),

    [string] $BuildConfigurationFolderName = 'build'
)

$ErrorActionPreference = 'Stop'

function Get-NormalizedFolderPath {
    param(
        [AllowNull()]
        [string] $Value
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return ''
    }

    return [System.IO.Path]::GetFullPath($Value).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar
    )
}

function Get-NormalizedScalarValue {
    param(
        [AllowNull()]
        [object] $Value
    )

    if ($null -eq $Value) {
        return ''
    }

    if ($Value -is [System.Array]) {
        return (@($Value) | ForEach-Object { [string] $_ }) -join ';'
    }

    return ([string] $Value).Trim()
}

function Get-NormalizedParameterListValue {
    param(
        [AllowNull()]
        [object] $Value
    )

    return (Get-ParameterListItems -Value $Value) -join ';'
}

function Get-ParameterListItems {
    param(
        [AllowNull()]
        [object] $Value
    )

    if ($null -eq $Value) {
        return @()
    }

    if ($Value -is [System.Array]) {
        $items = @($Value)
    }
    else {
        $items = @(([string] $Value) -split ';')
    }

    return @(
        $items |
            ForEach-Object { ([string] $_).Trim().Trim('"') } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )
}

function Get-CmdEnvironmentVariables {
    param(
        [Parameter(Mandatory = $true)]
        [string] $BuildPropertiesFilePath
    )

    $cmdExecutableFilePath = if ([string]::IsNullOrWhiteSpace($env:ComSpec)) { 'cmd.exe' } else { $env:ComSpec }
    $cmdCommand = "call `"$BuildPropertiesFilePath`" >NUL && set"
    $cmdOutput = & $cmdExecutableFilePath /D /S /C $cmdCommand

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build.Properties.cmd failed with exit code $LASTEXITCODE."
        exit 3
    }

    $variables = @{}

    foreach ($line in $cmdOutput) {
        $separatorIndex = $line.IndexOf('=')

        if ($separatorIndex -le 0) {
            continue
        }

        $name = $line.Substring(0, $separatorIndex)
        $value = $line.Substring($separatorIndex + 1)

        $variables[$name] = $value
    }

    return $variables
}

function Get-PowerShellVariableValue {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Name
    )

    $variable = Get-Variable -Name $Name -Scope Script -ErrorAction SilentlyContinue

    if ($null -eq $variable) {
        return $null
    }

    return $variable.Value
}

function Add-ComparisonDifference {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[string]] $Differences,

        [Parameter(Mandatory = $true)]
        [string] $Name,

        [AllowNull()]
        [string] $CmdValue,

        [AllowNull()]
        [string] $PowerShellValue
    )

    if ($CmdValue -cne $PowerShellValue) {
        $Differences.Add("$Name`n  .cmd = '$CmdValue'`n  .ps1 = '$PowerShellValue'")
    }
}

function Test-HasValue {
    param(
        [AllowNull()]
        [string] $Value
    )

    return -not [string]::IsNullOrWhiteSpace($Value)
}



if (-not $IsWindows) {
    Write-Host 'Skipping Windows-specific build infrastructure validation on non-Windows platforms.'
    exit 0
}

if (-not (Test-HasValue -Value $WorkspaceFolderPath)) {
    Write-Host "Workspace folder path must not be empty."
    exit 2
}

if (-not (Test-HasValue -Value $BuildConfigurationFolderName)) {
    Write-Host "Build configuration folder name must not be empty."
    exit 2
}

$WorkspaceFolderPath = Get-NormalizedFolderPath -Value $WorkspaceFolderPath

$BuildConfigurationFolderPath = Join-Path -Path $WorkspaceFolderPath -ChildPath $BuildConfigurationFolderName

if (-not (Test-Path -LiteralPath $BuildConfigurationFolderPath -PathType Container)) {
    Write-Host "Missing build configuration folder '$BuildConfigurationFolderPath'."
    exit 1
}

$requiredFilePaths = @(
    (Join-Path -Path $WorkspaceFolderPath -ChildPath 'Build.Properties.cmd')
    (Join-Path -Path $WorkspaceFolderPath -ChildPath 'Build.Properties.ps1')
    (Join-Path -Path $WorkspaceFolderPath -ChildPath 'BuildPackages.cmd')
    (Join-Path -Path $WorkspaceFolderPath -ChildPath 'BuildPackages.ps1')
    (Join-Path -Path $WorkspaceFolderPath -ChildPath 'CleanPackages.cmd')
    (Join-Path -Path $WorkspaceFolderPath -ChildPath 'CleanPackages.ps1')
    (Join-Path -Path $BuildConfigurationFolderPath -ChildPath 'BuildPackages.cmd')
    (Join-Path -Path $BuildConfigurationFolderPath -ChildPath 'BuildPackages.ps1')
    (Join-Path -Path $BuildConfigurationFolderPath -ChildPath 'PrepareNuGetLocalPackageSources.ps1')
)

foreach ($requiredFilePath in $requiredFilePaths) {
    if (-not (Test-Path -LiteralPath $requiredFilePath -PathType Leaf)) {
        Write-Host "Missing build configuration file '$requiredFilePath'."
        exit 1
    }
}

$buildPropertiesCmdFilePath = Join-Path -Path $WorkspaceFolderPath -ChildPath 'Build.Properties.cmd'
$buildPropertiesPowerShellFilePath = Join-Path -Path $WorkspaceFolderPath -ChildPath 'Build.Properties.ps1'

$cmdVariables = Get-CmdEnvironmentVariables -BuildPropertiesFilePath $buildPropertiesCmdFilePath

. $buildPropertiesPowerShellFilePath

$differences = [System.Collections.Generic.List[string]]::new()

$scalarVariableNames = @(
    'BUILD_CONFIGURATION_FOLDER_NAME'
    'SOURCE_CODE_FOLDER_NAME'
    'ARTIFACTS_FOLDER_NAME'
    'ARTIFACTS_OUTPUT_FOLDER_NAME'
    'ARTIFACTS_PACKAGE_OUTPUT_FOLDER_NAME'
    'ARTIFACTS_TEST_RESULTS_FOLDER_NAME'
    'PARAMETER___RUN_BUILD___DEFAULT'
    'PARAMETER___RUN_TEST___DEFAULT'
    'PARAMETER___RUN_PACKAGE___DEFAULT'
    'PARAMETER___VERSION___DEFAULT'
    'PARAMETER___CONFIGURATION___DEFAULT'
    'PARAMETER___FRAMEWORKS___DEFAULT'
    'PARAMETER___ENABLE_SOURCE_LINK___DEFAULT'
)

foreach ($variableName in $scalarVariableNames) {
    Add-ComparisonDifference `
        -Differences $differences `
        -Name $variableName `
        -CmdValue (Get-NormalizedScalarValue -Value $cmdVariables[$variableName]) `
        -PowerShellValue (Get-NormalizedScalarValue -Value (Get-PowerShellVariableValue -Name $variableName))
}

$folderPathVariableNames = @(
    'WORKSPACE_FOLDER_PATH'
    'BUILD_CONFIGURATION_FOLDER_PATH'
    'SOURCE_CODE_FOLDER_PATH'
    'ARTIFACTS_FOLDER_PATH'
    'ARTIFACTS_OUTPUT_FOLDER_PATH'
    'ARTIFACTS_PACKAGE_OUTPUT_FOLDER_PATH'
    'ARTIFACTS_TEST_RESULTS_FOLDER_PATH'
)

foreach ($variableName in $folderPathVariableNames) {
    Add-ComparisonDifference `
        -Differences $differences `
        -Name $variableName `
        -CmdValue (Get-NormalizedFolderPath -Value $cmdVariables[$variableName]) `
        -PowerShellValue (Get-NormalizedFolderPath -Value (Get-PowerShellVariableValue -Name $variableName))
}

$cmdBuildUnits = Get-NormalizedParameterListValue -Value $cmdVariables['BUILD_UNITS']
$powerShellBuildUnits = Get-NormalizedParameterListValue -Value (Get-PowerShellVariableValue -Name 'BUILD_UNITS')

Add-ComparisonDifference `
    -Differences $differences `
    -Name 'BUILD_UNITS' `
    -CmdValue $cmdBuildUnits `
    -PowerShellValue $powerShellBuildUnits

$buildUnits = Get-ParameterListItems -Value (Get-PowerShellVariableValue -Name 'BUILD_UNITS')
$buildUnitParameterNames = @(
    'BUILD_PARAMETERS'
    'TEST_PARAMETERS'
    'PACKAGE_PARAMETERS'
    'PACKAGE_NEV_PARAMETERS'
)

foreach ($buildUnit in $buildUnits) {
    foreach ($parameterName in $buildUnitParameterNames) {
        $cmdVariableName = "BUILD_UNIT___$buildUnit`___$parameterName"
        $cmdValue = Get-NormalizedParameterListValue -Value $cmdVariables[$cmdVariableName]

        $powerShellValue = $null
        if ($BUILD_UNIT_PARAMETERS.ContainsKey($buildUnit) -and $BUILD_UNIT_PARAMETERS[$buildUnit].ContainsKey($parameterName)) {
            $powerShellValue = $BUILD_UNIT_PARAMETERS[$buildUnit][$parameterName]
        }

        Add-ComparisonDifference `
            -Differences $differences `
            -Name $cmdVariableName `
            -CmdValue $cmdValue `
            -PowerShellValue (Get-NormalizedParameterListValue -Value $powerShellValue)
    }
}

if ($differences.Count -gt 0) {
    Write-Host 'Build infrastructure validation failed.'
    Write-Host ''

    foreach ($difference in $differences) {
        Write-Host $difference
        Write-Host ''
    }

    exit 4
}

Write-Host 'Build infrastructure validation succeeded.'
Write-Host "Workspace folder path:"
Write-Host "  $WorkspaceFolderPath"
Write-Host "Build configuration folder path:"
Write-Host "  $BuildConfigurationFolderPath"
Write-Host 'Build units:'

$powerShellBuildUnitItems = Get-ParameterListItems -Value $powerShellBuildUnits

foreach ($powerShellBuildUnit in $powerShellBuildUnitItems) {
    Write-Host "  $powerShellBuildUnit"
}
