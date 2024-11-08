#Requires -Version 7.0


# ================
# TERMINOLOGY
# ================
#
#     exit code                         = script/subroutine failure boundary.
#     $args                             = all original command-line arguments.
#     splatted array invocation         = safe external command argument passing.
#     Join-Path                         = file/folder path composition.
#     [System.IO.Path] helpers          = file-name/path parsing.
#
# ================
# EXIT CODES
# ================
# 0  = Success.
# 1  = Missing required build infrastructure.
# 2  = Invalid command-line argument.
# 3  = Build unit build parameters are not defined.
# 4  = Build framework selection failed.
# 5  = Build input file path resolution failed.
# 6  = dotnet build failed.
# 7  = Test framework selection failed.
# 8  = Test input file path resolution failed.
# 9  = dotnet test failed.
# 10 = Package input file path resolution failed.
# 11 = dotnet pack failed.
# 12 = Explicit.NuGet.Versions build failed.
# 13 = Explicit.NuGet.Versions execution failed.


$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ARTIFACTS_FOLDER_PATH)) {
    exit 1
}

function Test-HasValue {
    param(
        [AllowNull()]
        [string] $Value
    )

    return -not [string]::IsNullOrWhiteSpace($Value)
}

function Split-ParameterList {
    param(
        [AllowNull()]
        [object] $Value
    )

    if ($null -eq $Value) {
        return @()
    }

    if ($Value -is [System.Array]) {
        return @($Value | Where-Object { -not [string]::IsNullOrWhiteSpace([string] $_) })
    }

    return @(([string] $Value) -split ';' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
}

function Split-Frameworks {
    param(
        [AllowNull()]
        [string] $Frameworks
    )

    return @(Split-ParameterList -Value $Frameworks)
}

function Split-BuildOrTestParameter {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Parameter
    )

    $parts = $Parameter -split '\|', 2

    return [pscustomobject] @{
        InputFileName = $parts[0].Trim().Trim('"')
        Frameworks = if ($parts.Count -gt 1) { $parts[1].Trim().Trim('"') } else { '' }
    }
}

function Split-PackageParameter {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Parameter
    )

    $parts = $Parameter -split '\|', 2
    return $parts[0].Trim().Trim('"')
}

function Get-FileNameWithoutExtension {
    param(
        [Parameter(Mandatory = $true)]
        [string] $FileName
    )

    return [System.IO.Path]::GetFileNameWithoutExtension($FileName)
}

function Get-FileExtension {
    param(
        [Parameter(Mandatory = $true)]
        [string] $FileName
    )

    return [System.IO.Path]::GetExtension($FileName)
}

function Join-FolderPath {
    param(
        [Parameter(Mandatory = $true)]
        [string] $BaseFolderPath,

        [Parameter(Mandatory = $true)]
        [string[]] $ChildPath
    )

    $folderPath = $BaseFolderPath

    foreach ($childPathItem in $ChildPath) {
        $folderPath = Join-Path -Path $folderPath -ChildPath $childPathItem
    }

    return $folderPath
}

function Invoke-ExternalCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string] $FilePath,

        [Parameter(Mandatory = $true)]
        [string[]] $ArgumentList,

        [Parameter(Mandatory = $true)]
        [int] $FailureExitCode,

        [switch] $SuppressStandardOutput
    )

    if ($SuppressStandardOutput) {
        & $FilePath @ArgumentList 1>$null
    }
    else {
        & $FilePath @ArgumentList
    }

    if ($LASTEXITCODE -ne 0) {
        exit $FailureExitCode
    }
}

function Set-ArgumentValue {
    param(
        [Parameter(Mandatory = $true)]
        [string] $ArgumentName,

        [AllowNull()]
        [string] $ArgumentValue
    )

    if ([string]::IsNullOrWhiteSpace($ArgumentValue)) {
        Write-Host "Missing value for $ArgumentName."
        exit 2
    }

    if ($ArgumentValue.StartsWith('--', [System.StringComparison]::Ordinal)) {
        Write-Host "Missing value for $ArgumentName."
        exit 2
    }

    return $ArgumentValue
}

function Initialize-Parameters {
    $script:PARAMETER___RUN_BUILD = $PARAMETER___RUN_BUILD___DEFAULT
    if (Test-HasValue -Value $script:ARGUMENT___RUN_BUILD) {
        $script:PARAMETER___RUN_BUILD = $script:ARGUMENT___RUN_BUILD
    }

    $script:PARAMETER___RUN_TEST = $PARAMETER___RUN_TEST___DEFAULT
    if (Test-HasValue -Value $script:ARGUMENT___RUN_TEST) {
        $script:PARAMETER___RUN_TEST = $script:ARGUMENT___RUN_TEST
    }

    $script:PARAMETER___RUN_PACKAGE = $PARAMETER___RUN_PACKAGE___DEFAULT
    if (Test-HasValue -Value $script:ARGUMENT___RUN_PACKAGE) {
        $script:PARAMETER___RUN_PACKAGE = $script:ARGUMENT___RUN_PACKAGE
    }

    $script:PARAMETER___VERSION = $PARAMETER___VERSION___DEFAULT
    if (Test-HasValue -Value $script:ARGUMENT___VERSION) {
        $script:PARAMETER___VERSION = $script:ARGUMENT___VERSION
    }

    $script:PARAMETER___CONFIGURATION = $PARAMETER___CONFIGURATION___DEFAULT
    if (Test-HasValue -Value $script:ARGUMENT___CONFIGURATION) {
        $script:PARAMETER___CONFIGURATION = $script:ARGUMENT___CONFIGURATION
    }

    $script:PARAMETER___FRAMEWORKS = $null
    if (Test-HasValue -Value $script:ARGUMENT___FRAMEWORKS) {
        $script:PARAMETER___FRAMEWORKS = $script:ARGUMENT___FRAMEWORKS
    }

    $script:PARAMETER___ENABLE_SOURCE_LINK = $PARAMETER___ENABLE_SOURCE_LINK___DEFAULT
    if (Test-HasValue -Value $script:ARGUMENT___ENABLE_SOURCE_LINK) {
        $script:PARAMETER___ENABLE_SOURCE_LINK = $script:ARGUMENT___ENABLE_SOURCE_LINK
    }
}

function Parse-Arguments {
    $argumentIndex = 0
    $scriptArguments = @($args)

    while ($argumentIndex -lt $scriptArguments.Count) {
        $argument = [string] $scriptArguments[$argumentIndex]

        if ($argument -ieq '--no-build') {
            $script:ARGUMENT___RUN_BUILD = 'false'
            $argumentIndex++
            continue
        }

        if ($argument -ieq '--no-test') {
            $script:ARGUMENT___RUN_TEST = 'false'
            $argumentIndex++
            continue
        }

        if ($argument -ieq '--no-package') {
            $script:ARGUMENT___RUN_PACKAGE = 'false'
            $argumentIndex++
            continue
        }

        if ($argument -ieq '--version') {
            $script:ARGUMENT___VERSION = Set-ArgumentValue -ArgumentName $argument -ArgumentValue $scriptArguments[$argumentIndex + 1]
            $argumentIndex += 2
            continue
        }

        if ($argument -ieq '--configuration') {
            $script:ARGUMENT___CONFIGURATION = Set-ArgumentValue -ArgumentName $argument -ArgumentValue $scriptArguments[$argumentIndex + 1]
            $argumentIndex += 2
            continue
        }

        if ($argument -ieq '--frameworks') {
            $script:ARGUMENT___FRAMEWORKS = Set-ArgumentValue -ArgumentName $argument -ArgumentValue $scriptArguments[$argumentIndex + 1]
            $argumentIndex += 2
            continue
        }

        if ($argument -ieq '--disable-source-link') {
            $script:ARGUMENT___ENABLE_SOURCE_LINK = 'false'
            $argumentIndex++
            continue
        }

        Write-Host "Unknown argument `"$argument`"."
        Write-Host 'Supported arguments are --no-build, --no-test, --no-package, --version, --configuration, --frameworks, and --disable-source-link.'
        exit 2
    }
}

function Get-BuildUnitParameters {
    param(
        [Parameter(Mandatory = $true)]
        [string] $BuildUnit
    )

    if ($null -eq $BUILD_UNIT_PARAMETERS -or -not $BUILD_UNIT_PARAMETERS.ContainsKey($BuildUnit)) {
        Write-Host "Build unit `"$BuildUnit`" is invalid because no build parameters defined."
        exit 3
    }

    $parameters = $BUILD_UNIT_PARAMETERS[$BuildUnit]

    if ($null -eq $parameters.BUILD_PARAMETERS -or @($parameters.BUILD_PARAMETERS).Count -eq 0) {
        Write-Host "Build unit `"$BuildUnit`" is invalid because no build parameters defined."
        exit 3
    }

    return $parameters
}

function Select-Frameworks {
    param(
        [Parameter(Mandatory = $true)]
        [string] $AvailableFrameworks,

        [Parameter(Mandatory = $true)]
        [string] $RequestedFrameworks
    )

    $availableFrameworkItems = Split-Frameworks -Frameworks $AvailableFrameworks
    $requestedFrameworkItems = Split-Frameworks -Frameworks $RequestedFrameworks
    $selectedFrameworkItems = New-Object System.Collections.Generic.List[string]

    foreach ($requestedFramework in $requestedFrameworkItems) {
        $exactMatchFound = $false

        foreach ($availableFramework in $availableFrameworkItems) {
            if ([string]::Equals($availableFramework, $requestedFramework, [System.StringComparison]::OrdinalIgnoreCase)) {
                $selectedFrameworkItems.Add($availableFramework)
                $exactMatchFound = $true
            }
        }

        if (-not $exactMatchFound) {
            foreach ($availableFramework in $availableFrameworkItems) {
                if ($availableFramework.StartsWith($requestedFramework, [System.StringComparison]::OrdinalIgnoreCase)) {
                    $selectedFrameworkItems.Add($availableFramework)
                }
            }
        }
    }

    if ($selectedFrameworkItems.Count -eq 0) {
        Write-Host 'No matching frameworks found.'
        Write-Host "Available frameworks: `"$AvailableFrameworks`""
        Write-Host "Requested frameworks: `"$RequestedFrameworks`""
        return $null
    }

    return $selectedFrameworkItems.ToArray()
}

function Resolve-BuildInputFilePath {
    param(
        [Parameter(Mandatory = $true)]
        [string] $InputFileName
    )

    $fileExtension = Get-FileExtension -FileName $InputFileName
    $fileNameWithoutExtension = Get-FileNameWithoutExtension -FileName $InputFileName

    if ($fileExtension -iin @('.slnx', '.sln')) {
        return Join-Path -Path $WORKSPACE_FOLDER_PATH -ChildPath $InputFileName
    }

    if ($fileExtension -iin @('.csproj', '.fsproj', '.vbproj')) {
        return Join-FolderPath -BaseFolderPath $SOURCE_CODE_FOLDER_PATH -ChildPath @($fileNameWithoutExtension, $InputFileName)
    }

    Write-Host "Unsupported build input file name `"$InputFileName`"."
    Write-Host 'Supported build input file extensions are .slnx, .sln, .csproj, .fsproj, and .vbproj.'
    return $null
}

function Resolve-TestInputFilePath {
    param(
        [Parameter(Mandatory = $true)]
        [string] $InputFileName,

        [Parameter(Mandatory = $true)]
        [string] $Framework
    )

    $fileExtension = Get-FileExtension -FileName $InputFileName
    $fileNameWithoutExtension = Get-FileNameWithoutExtension -FileName $InputFileName

    if ($fileExtension -ieq '.dll') {
        return Join-FolderPath -BaseFolderPath $script:ARTIFACTS_OUTPUT___CONFIGURATION___FOLDER_PATH -ChildPath @($Framework, $fileNameWithoutExtension, $InputFileName)
    }

    if ($fileExtension -iin @('.csproj', '.fsproj', '.vbproj')) {
        return Join-FolderPath -BaseFolderPath $SOURCE_CODE_FOLDER_PATH -ChildPath @($fileNameWithoutExtension, $InputFileName)
    }

    Write-Host "Unsupported test input file name `"$InputFileName`"."
    Write-Host 'Supported test input file extensions are .dll, .csproj, .fsproj, and .vbproj.'
    return $null
}

function Resolve-PackageInputFilePath {
    param(
        [Parameter(Mandatory = $true)]
        [string] $InputFileName
    )

    $fileExtension = Get-FileExtension -FileName $InputFileName
    $fileNameWithoutExtension = Get-FileNameWithoutExtension -FileName $InputFileName

    if ($fileExtension -iin @('.csproj', '.fsproj', '.vbproj')) {
        return Join-FolderPath -BaseFolderPath $SOURCE_CODE_FOLDER_PATH -ChildPath @($fileNameWithoutExtension, $InputFileName)
    }

    Write-Host "Unsupported package input file name `"$InputFileName`"."
    Write-Host 'Supported package input file extensions are .csproj, .fsproj, and .vbproj.'
    return $null
}

function Invoke-BuildPhase {
    param(
        [Parameter(Mandatory = $true)]
        [object] $BuildParameters
    )

    if ($script:PARAMETER___RUN_BUILD -ieq 'false') {
        Write-Host 'Skipping build operations because --no-build was specified.'
        return
    }

    foreach ($buildParameterItem in @(Split-ParameterList -Value $BuildParameters)) {
        $buildParameter = Split-BuildOrTestParameter -Parameter $buildParameterItem
        $buildInputFilePath = Resolve-BuildInputFilePath -InputFileName $buildParameter.InputFileName

        if (-not (Test-HasValue -Value $buildInputFilePath)) {
            exit 5
        }

        if (-not (Test-HasValue -Value $script:PARAMETER___FRAMEWORKS)) {
            Write-Host '--------------------------------------------------------------------------------'
            Write-Host "Building `"$($buildParameter.InputFileName)`" ($script:PARAMETER___VERSION | $script:PARAMETER___CONFIGURATION | project-defined frameworks)..."
            Write-Host '--------------------------------------------------------------------------------'

            $dotnetArguments = @(
                'build'
                $buildInputFilePath
                "--property:BUILD_VERSION=$script:PARAMETER___VERSION"
                '--configuration'
                $script:PARAMETER___CONFIGURATION
                "--property:ENABLE_SOURCE_LINK=$script:PARAMETER___ENABLE_SOURCE_LINK"
                '--property:GeneratePackageOnBuild=false'
            )

            Invoke-ExternalCommand -FilePath 'dotnet' -ArgumentList $dotnetArguments -FailureExitCode 6
        }
        else {
            $selectedFrameworks = Select-Frameworks -AvailableFrameworks $buildParameter.Frameworks -RequestedFrameworks $script:PARAMETER___FRAMEWORKS

            if ($null -eq $selectedFrameworks -or @($selectedFrameworks).Count -eq 0) {
                exit 4
            }

            foreach ($buildFramework in $selectedFrameworks) {
                Write-Host '--------------------------------------------------------------------------------'
                Write-Host "Building `"$($buildParameter.InputFileName)`" ($script:PARAMETER___VERSION | $script:PARAMETER___CONFIGURATION | $buildFramework)..."
                Write-Host '--------------------------------------------------------------------------------'

                $dotnetArguments = @(
                    'build'
                    $buildInputFilePath
                    "--property:BUILD_VERSION=$script:PARAMETER___VERSION"
                    '--configuration'
                    $script:PARAMETER___CONFIGURATION
                    '--framework'
                    $buildFramework
                    "--property:ENABLE_SOURCE_LINK=$script:PARAMETER___ENABLE_SOURCE_LINK"
                    '--property:GeneratePackageOnBuild=false'
                )

                Invoke-ExternalCommand -FilePath 'dotnet' -ArgumentList $dotnetArguments -FailureExitCode 6
            }
        }
    }
}

function Invoke-TestPhase {
    param(
        [AllowNull()]
        [object] $TestParameters
    )

    if ($null -eq $TestParameters -or @(Split-ParameterList -Value $TestParameters).Count -eq 0) {
        Write-Host "Skipping test operations because no test parameters defined for build unit `"$script:BUILD_UNIT`"."
        return
    }

    if ($script:PARAMETER___RUN_TEST -ieq 'false') {
        Write-Host 'Skipping test operations because --no-test was specified.'
        return
    }

    $script:ARTIFACTS_OUTPUT___CONFIGURATION___FOLDER_PATH = Join-Path -Path $ARTIFACTS_OUTPUT_FOLDER_PATH -ChildPath $script:PARAMETER___CONFIGURATION
    $script:ARTIFACTS_TEST_RESULTS___CONFIGURATION___FOLDER_PATH = Join-Path -Path $ARTIFACTS_TEST_RESULTS_FOLDER_PATH -ChildPath $script:PARAMETER___CONFIGURATION

    foreach ($testParameterItem in @(Split-ParameterList -Value $TestParameters)) {
        $testParameter = Split-BuildOrTestParameter -Parameter $testParameterItem
        $testInputFileExtension = Get-FileExtension -FileName $testParameter.InputFileName
        $testInputFileNameWithoutExtension = Get-FileNameWithoutExtension -FileName $testParameter.InputFileName

        $testFrameworks = $testParameter.Frameworks

        if (Test-HasValue -Value $script:PARAMETER___FRAMEWORKS) {
            $selectedFrameworks = Select-Frameworks -AvailableFrameworks $testParameter.Frameworks -RequestedFrameworks $script:PARAMETER___FRAMEWORKS

            if ($null -eq $selectedFrameworks -or @($selectedFrameworks).Count -eq 0) {
                exit 7
            }
        }
        else {
            $selectedFrameworks = Split-Frameworks -Frameworks $testFrameworks
        }

        foreach ($testFramework in $selectedFrameworks) {
            $testInputFilePath = Resolve-TestInputFilePath -InputFileName $testParameter.InputFileName -Framework $testFramework

            if (-not (Test-HasValue -Value $testInputFilePath)) {
                exit 8
            }

            Write-Host '--------------------------------------------------------------------------------'
            Write-Host "Testing `"$($testParameter.InputFileName)`" ($script:PARAMETER___VERSION | $script:PARAMETER___CONFIGURATION | $testFramework)..."
            Write-Host '--------------------------------------------------------------------------------'

            $nunitLogger = "nunit;LogFileName=$($testInputFileNameWithoutExtension)_$($script:PARAMETER___VERSION)_$($script:PARAMETER___CONFIGURATION)_$($testFramework)_TestResults-NUnit.xml;format=nunit3"
            $liquidLogger = "liquid.md;LogFileName=$($testInputFileNameWithoutExtension)_$($script:PARAMETER___VERSION)_$($script:PARAMETER___CONFIGURATION)_$($testFramework)_TestResults-Liquid.md"

            if ($testInputFileExtension -ieq '.dll') {
                $dotnetArguments = @(
                    'test'
                    $testInputFilePath
                    '--framework'
                    $testFramework
                    '--results-directory'
                    $script:ARTIFACTS_TEST_RESULTS___CONFIGURATION___FOLDER_PATH
                    '--logger'
                    $nunitLogger
                    '--logger'
                    $liquidLogger
                )
            }
            else {
                $dotnetArguments = @(
                    'test'
                    $testInputFilePath
                    '--configuration'
                    $script:PARAMETER___CONFIGURATION
                    '--framework'
                    $testFramework
                    '--no-build'
                    '--results-directory'
                    $script:ARTIFACTS_TEST_RESULTS___CONFIGURATION___FOLDER_PATH
                    '--logger'
                    $nunitLogger
                    '--logger'
                    $liquidLogger
                )
            }

            Invoke-ExternalCommand -FilePath 'dotnet' -ArgumentList $dotnetArguments -FailureExitCode 9
        }
    }
}

function Invoke-PackagePhase {
    param(
        [AllowNull()]
        [object] $PackageParameters,

        [AllowNull()]
        [object] $PackageNevParameters
    )

    if ($null -eq $PackageParameters -or @(Split-ParameterList -Value $PackageParameters).Count -eq 0) {
        Write-Host "Skipping package operations because no package parameters defined for build unit `"$script:BUILD_UNIT`"."
        return
    }

    if ($script:PARAMETER___RUN_PACKAGE -ieq 'false') {
        Write-Host 'Skipping package operations because --no-package was specified.'
        return
    }

    if (Test-HasValue -Value $script:PARAMETER___FRAMEWORKS) {
        Write-Host 'Skipping package operations because --frameworks was specified.'
        Write-Host 'dotnet pack --no-build requires a full project-defined framework build.'
        return
    }

    $artifactsPackageOutputConfigurationFolderPath = Join-Path -Path $ARTIFACTS_PACKAGE_OUTPUT_FOLDER_PATH -ChildPath $script:PARAMETER___CONFIGURATION

    foreach ($packageParameterItem in @(Split-ParameterList -Value $PackageParameters)) {
        $packageInputFileName = Split-PackageParameter -Parameter $packageParameterItem
        $packageInputFilePath = Resolve-PackageInputFilePath -InputFileName $packageInputFileName

        if (-not (Test-HasValue -Value $packageInputFilePath)) {
            exit 10
        }

        Write-Host '--------------------------------------------------------------------------------'
        Write-Host "Packing `"$packageInputFileName`" ($script:PARAMETER___VERSION | $script:PARAMETER___CONFIGURATION)..."
        Write-Host '--------------------------------------------------------------------------------'

        $dotnetArguments = @(
            'pack'
            $packageInputFilePath
            '--configuration'
            $script:PARAMETER___CONFIGURATION
            '--no-build'
            "--property:BUILD_VERSION=$script:PARAMETER___VERSION"
            "--property:ENABLE_SOURCE_LINK=$script:PARAMETER___ENABLE_SOURCE_LINK"
        )

        Invoke-ExternalCommand -FilePath 'dotnet' -ArgumentList $dotnetArguments -FailureExitCode 11
    }

    if ($null -eq $PackageNevParameters -or @(Split-ParameterList -Value $PackageNevParameters).Count -eq 0) {
        Write-Host "Skipping package dependency version update operations because no package dependency version update parameters defined for build unit `"$script:BUILD_UNIT`"."
        return
    }

    $nevSolutionFilePath = Join-FolderPath -BaseFolderPath $WORKSPACE_FOLDER_PATH -ChildPath @('tools', 'Explicit.NuGet.Versions', 'Explicit.NuGet.Versions.slnx')

    $dotnetArguments = @(
        'build'
        $nevSolutionFilePath
        '--configuration'
        'Release'
    )

    Invoke-ExternalCommand -FilePath 'dotnet' -ArgumentList $dotnetArguments -FailureExitCode 12 -SuppressStandardOutput

    foreach ($packageNevParameterItem in @(Split-ParameterList -Value $PackageNevParameters)) {
        $packageNevDependencyPackageIdPrefixFilter = ([string] $packageNevParameterItem).Trim().Trim('"')

        Write-Host '--------------------------------------------------------------------------------'
        Write-Host "Updating package dependency versions for `"$packageNevDependencyPackageIdPrefixFilter`" ($script:PARAMETER___VERSION | $script:PARAMETER___CONFIGURATION)..."
        Write-Host '--------------------------------------------------------------------------------'

        $packageNevCommandPath = Join-FolderPath -BaseFolderPath $ARTIFACTS_FOLDER_PATH -ChildPath @('tools', 'nev', 'nev.exe')

        $packageNevArguments = @(
            $artifactsPackageOutputConfigurationFolderPath
            $packageNevDependencyPackageIdPrefixFilter
        )

        Invoke-ExternalCommand -FilePath $packageNevCommandPath -ArgumentList $packageNevArguments -FailureExitCode 13
    }
}

function Invoke-BuildUnit {
    param(
        [Parameter(Mandatory = $true)]
        [object] $Parameters
    )

    Write-Host

    Invoke-BuildPhase -BuildParameters $Parameters.BUILD_PARAMETERS

    Write-Host

    Invoke-TestPhase -TestParameters $Parameters.TEST_PARAMETERS

    Write-Host

    Invoke-PackagePhase -PackageParameters $Parameters.PACKAGE_PARAMETERS -PackageNevParameters $Parameters.PACKAGE_NEV_PARAMETERS

    Write-Host
}



Parse-Arguments @args

Initialize-Parameters

if ($null -eq $BUILD_UNITS -or @($BUILD_UNITS).Count -eq 0) {
    exit 1
}

Write-Host

foreach ($buildUnitItem in $BUILD_UNITS) {
    $script:BUILD_UNIT = [string] $buildUnitItem

    Write-Host '================================================================================'
    Write-Host "Build Unit `"$script:BUILD_UNIT`""
    Write-Host '================================================================================'

    $buildUnitParameters = Get-BuildUnitParameters -BuildUnit $script:BUILD_UNIT
    Invoke-BuildUnit -Parameters $buildUnitParameters
}

exit 0
