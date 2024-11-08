#Requires -Version 7.0


param(
    [string] $NuGetConfigFilePath = (Join-Path -Path (Split-Path -Parent $PSScriptRoot) -ChildPath 'nuget.config'),

    [string] $PackageSourceKeyPrefix = 'Local Package Artifacts'
)

$ErrorActionPreference = 'Stop'

function Test-HasValue {
    param(
        [AllowNull()]
        [string] $Value
    )

    return -not [string]::IsNullOrWhiteSpace($Value)
}

function Test-PackageSourceKeyMatchesPrefix {
    param(
        [Parameter(Mandatory = $true)]
        [string] $PackageSourceKey,

        [Parameter(Mandatory = $true)]
        [string] $PackageSourceKeyPrefix
    )

    return $PackageSourceKey -eq $PackageSourceKeyPrefix -or
        $PackageSourceKey.StartsWith("$PackageSourceKeyPrefix (", [System.StringComparison]::Ordinal)
}

function Test-PackageSourceIsDisabled {
    param(
        [Parameter(Mandatory = $true)]
        [xml] $NuGetConfig,

        [Parameter(Mandatory = $true)]
        [string] $PackageSourceKey
    )

    $disabledPackageSources = @($NuGetConfig.configuration.disabledPackageSources.add)

    foreach ($disabledPackageSource in $disabledPackageSources) {
        $disabledPackageSourceKey = [string] $disabledPackageSource.key
        $disabledPackageSourceValue = [string] $disabledPackageSource.value

        if ($disabledPackageSourceKey -eq $PackageSourceKey -and $disabledPackageSourceValue -ieq 'true') {
            return $true
        }
    }

    return $false
}

function Resolve-PackageSourceFolderPath {
    param(
        [Parameter(Mandatory = $true)]
        [string] $NuGetConfigFolderPath,

        [Parameter(Mandatory = $true)]
        [string] $PackageSourceValue
    )

    $packageSourceUri = $null
    $isAbsoluteUri = [System.Uri]::TryCreate(
        $PackageSourceValue,
        [System.UriKind]::Absolute,
        [ref] $packageSourceUri
    )

    if ($isAbsoluteUri -and $packageSourceUri.Scheme -ne 'file') {
        return $null
    }

    if ($isAbsoluteUri -and $packageSourceUri.Scheme -eq 'file') {
        return $packageSourceUri.LocalPath
    }

    if ([System.IO.Path]::IsPathRooted($PackageSourceValue)) {
        return [System.IO.Path]::GetFullPath($PackageSourceValue)
    }

    return [System.IO.Path]::GetFullPath(
        [System.IO.Path]::Combine($NuGetConfigFolderPath, $PackageSourceValue)
    )
}



if (-not (Test-Path -LiteralPath $NuGetConfigFilePath -PathType Leaf)) {
    throw "Missing '$NuGetConfigFilePath'."
}

if (-not (Test-HasValue -Value $PackageSourceKeyPrefix)) {
    throw 'Package source key prefix must not be empty.'
}

[xml] $nuGetConfig = Get-Content -LiteralPath $NuGetConfigFilePath
$nuGetConfigFolderPath = Split-Path -Parent $NuGetConfigFilePath

$packageSources = @($nuGetConfig.configuration.packageSources.add |
    Where-Object {
        $packageSourceKey = [string] $_.key
        Test-PackageSourceKeyMatchesPrefix `
            -PackageSourceKey $packageSourceKey `
            -PackageSourceKeyPrefix $PackageSourceKeyPrefix
    })

if ($packageSources.Count -eq 0) {
    throw "Missing NuGet package sources with key prefix '$PackageSourceKeyPrefix' in '$NuGetConfigFilePath'."
}

foreach ($packageSource in $packageSources) {
    $packageSourceKey = [string] $packageSource.key
    $packageSourceValue = [string] $packageSource.value

    if (Test-PackageSourceIsDisabled -NuGetConfig $nuGetConfig -PackageSourceKey $packageSourceKey) {
        Write-Host "Skipping disabled NuGet package source '$packageSourceKey'."
        continue
    }

    if (-not (Test-HasValue -Value $packageSourceValue)) {
        throw "NuGet package source '$packageSourceKey' has an empty value in '$NuGetConfigFilePath'."
    }

    $packageSourceFolderPath = Resolve-PackageSourceFolderPath `
        -NuGetConfigFolderPath $nuGetConfigFolderPath `
        -PackageSourceValue $packageSourceValue

    if ($null -eq $packageSourceFolderPath) {
        Write-Host "Skipping non-local NuGet package source '$packageSourceKey': $packageSourceValue"
        continue
    }

    if (Test-Path -LiteralPath $packageSourceFolderPath -PathType Leaf) {
        throw "NuGet package source '$packageSourceKey' resolves to an existing file: $packageSourceFolderPath"
    }

    if (-not (Test-Path -LiteralPath $packageSourceFolderPath -PathType Container)) {
        New-Item -ItemType Directory -Path $packageSourceFolderPath -Force | Out-Null
    }

    Write-Host "Prepared NuGet package source '$packageSourceKey': $packageSourceFolderPath"
}
