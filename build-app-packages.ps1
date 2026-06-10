
<#
.SYNOPSIS
Builds CmdPalTranslator MSIX packages and an MSIX Bundle.

.DESCRIPTION
This script automates the package build flow for release:
1. Reads the package version from the project file and Package.appxmanifest.
2. Increments the last version part by default, then updates AppxPackageVersion and Identity Version.
3. Runs dotnet build for each selected platform to create MSIX packages.
4. Collects the generated MSIX files and creates bundle_mapping.txt.
5. Uses makeappx.exe to create a Microsoft Store-ready .msixbundle.

Before running this script, make sure the .NET SDK and Windows SDK are installed and makeappx.exe can be found.
By default, the bundle and bundle_mapping.txt are written to src\CmdPalTranslator\AppPackages.

.EXAMPLE
.\build-app-packages.ps1
Builds x64 and ARM64 packages with the default settings and automatically increments the version.

.EXAMPLE
.\build-app-packages.ps1 -Platforms x64 -PackageVersion 0.1.1.4
Builds only the x64 package and uses the specified package version.

.EXAMPLE
.\build-app-packages.ps1 -SkipVersionBump -SkipBuild
Skips the version update and build steps, then creates the bundle from existing MSIX packages.
#>

#requires -Version 5.1

[CmdletBinding()]
param(
    [string]$ProjectPath = 'src\CmdPalTranslator\CmdPalTranslator.csproj',

    [string]$Configuration = 'Release',

    [ValidateSet('x64', 'ARM64')]
    [string[]]$Platforms = @('x64', 'ARM64'),

    [string]$ExtensionName,

    [string]$PackageVersion,

    [string]$BundleDirectory,

    [switch]$SkipVersionBump,

    [switch]$SkipBuild,

    [switch]$SkipBundle
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function ConvertTo-FullPath {
    param(
        [Parameter(Mandatory)]
        [string]$Path,

        [Parameter(Mandatory)]
        [string]$BasePath
    )

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $BasePath $Path))
}

function Get-ProjectProperty {
    param(
        [Parameter(Mandatory)]
        [xml]$ProjectXml,

        [Parameter(Mandatory)]
        [string]$Name
    )

    foreach ($propertyGroup in $ProjectXml.Project.PropertyGroup) {
        $node = $propertyGroup.SelectSingleNode($Name)

        if ($null -ne $node -and -not [string]::IsNullOrWhiteSpace($node.InnerText)) {
            return $node.InnerText.Trim()
        }
    }

    return $null
}

function ConvertTo-PackageVersionParts {
    param(
        [Parameter(Mandatory)]
        [string]$Version
    )

    if ($Version -notmatch '^\d+\.\d+\.\d+\.\d+$') {
        throw "Package version '$Version' must use four numeric parts, for example 0.1.1.0."
    }

    $parts = @($Version.Split('.') | ForEach-Object { [int]$_ })

    foreach ($part in $parts) {
        if ($part -lt 0 -or $part -gt 65535) {
            throw "Package version '$Version' contains '$part', but each part must be between 0 and 65535."
        }
    }

    return $parts
}

function Get-IncrementedPackageVersion {
    param(
        [Parameter(Mandatory)]
        [string]$Version
    )

    $parts = ConvertTo-PackageVersionParts -Version $Version

    if ($parts[3] -ge 65535) {
        throw "Package version '$Version' cannot be incremented because the last part is already 65535."
    }

    $parts[3]++
    return $parts -join '.'
}

function Read-XmlDocument {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    $document = New-Object System.Xml.XmlDocument
    $document.PreserveWhitespace = $true
    $document.Load($Path)
    return $document
}

function Read-TextFile {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $encoding = New-Object System.Text.UTF8Encoding($false)
    $offset = 0

    if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
        $encoding = New-Object System.Text.UTF8Encoding($true)
        $offset = 3
    } elseif ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFF -and $bytes[1] -eq 0xFE) {
        $encoding = New-Object System.Text.UnicodeEncoding($false, $true)
        $offset = 2
    } elseif ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFE -and $bytes[1] -eq 0xFF) {
        $encoding = New-Object System.Text.UnicodeEncoding($true, $true)
        $offset = 2
    }

    $text = if ($bytes.Length -gt $offset) {
        $encoding.GetString($bytes, $offset, $bytes.Length - $offset)
    } else {
        ''
    }

    return [PSCustomObject]@{
        Path = $Path
        Text = $text
        Encoding = $encoding
    }
}

function Write-TextFile {
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$File,

        [Parameter(Mandatory)]
        [string]$Text
    )

    $writer = New-Object System.IO.StreamWriter($File.Path, $false, $File.Encoding)

    try {
        $writer.Write($Text)
    } finally {
        $writer.Dispose()
    }
}

function Get-UpdatedProjectPackageVersionText {
    param(
        [Parameter(Mandatory)]
        [string]$Text,

        [Parameter(Mandatory)]
        [string]$Version
    )

    $regex = New-Object System.Text.RegularExpressions.Regex(
        '^(?<prefix>\s*<AppxPackageVersion>)(?<version>[^<]+)(?<suffix></AppxPackageVersion>\s*)$',
        [System.Text.RegularExpressions.RegexOptions]::Multiline
    )
    $matches = @($regex.Matches($Text))

    if ($matches.Count -ne 1) {
        throw "Expected exactly one AppxPackageVersion line, but found $($matches.Count)."
    }

    return $regex.Replace(
        $Text,
        [System.Text.RegularExpressions.MatchEvaluator]{
            param($match)
            return "$($match.Groups['prefix'].Value)$Version$($match.Groups['suffix'].Value)"
        },
        1
    )
}

function Get-ManifestIdentity {
    param(
        [Parameter(Mandatory)]
        [System.Xml.XmlDocument]$ManifestXml
    )

    $namespaceManager = New-Object System.Xml.XmlNamespaceManager($ManifestXml.NameTable)
    $namespaceManager.AddNamespace('appx', $ManifestXml.DocumentElement.NamespaceURI)
    $identity = $ManifestXml.SelectSingleNode('/appx:Package/appx:Identity', $namespaceManager)

    if ($null -eq $identity) {
        throw 'Identity was not found in Package.appxmanifest.'
    }

    return $identity
}

function Get-ManifestPackageVersion {
    param(
        [Parameter(Mandatory)]
        [string]$ManifestPath
    )

    $manifestXml = Read-XmlDocument -Path $ManifestPath
    $identity = Get-ManifestIdentity -ManifestXml $manifestXml
    return $identity.GetAttribute('Version')
}

function Get-UpdatedManifestPackageVersionText {
    param(
        [Parameter(Mandatory)]
        [string]$Text,

        [Parameter(Mandatory)]
        [string]$Version
    )

    $regex = New-Object System.Text.RegularExpressions.Regex(
        '(?<prefix><Identity\b(?:(?!</?Identity\b).)*?\bVersion=")(?<version>[^"]+)(?<suffix>"(?:(?!</?Identity\b).)*?/>)',
        [System.Text.RegularExpressions.RegexOptions]::Singleline
    )
    $matches = @($regex.Matches($Text))

    if ($matches.Count -ne 1) {
        throw "Expected exactly one Identity Version attribute, but found $($matches.Count)."
    }

    return $regex.Replace(
        $Text,
        [System.Text.RegularExpressions.MatchEvaluator]{
            param($match)
            return "$($match.Groups['prefix'].Value)$Version$($match.Groups['suffix'].Value)"
        },
        1
    )
}

function Update-PackageVersionFiles {
    param(
        [Parameter(Mandatory)]
        [string]$ProjectPath,

        [Parameter(Mandatory)]
        [string]$ManifestPath,

        [Parameter(Mandatory)]
        [string]$Version
    )

    $projectFile = Read-TextFile -Path $ProjectPath
    $manifestFile = Read-TextFile -Path $ManifestPath

    $updatedProjectText = Get-UpdatedProjectPackageVersionText -Text $projectFile.Text -Version $Version
    $updatedManifestText = Get-UpdatedManifestPackageVersionText -Text $manifestFile.Text -Version $Version

    Write-TextFile -File $projectFile -Text $updatedProjectText
    Write-TextFile -File $manifestFile -Text $updatedManifestText
}

function ConvertTo-PackagePlatform {
    param(
        [Parameter(Mandatory)]
        [string]$Platform
    )

    if ($Platform -ieq 'arm64') {
        return 'ARM64'
    }

    if ($Platform -ieq 'x64') {
        return 'x64'
    }

    throw "Unsupported platform '$Platform'."
}

function Get-RelativePath {
    param(
        [Parameter(Mandatory)]
        [string]$BasePath,

        [Parameter(Mandatory)]
        [string]$TargetPath
    )

    $baseFullPath = [System.IO.Path]::GetFullPath($BasePath)
    $targetFullPath = [System.IO.Path]::GetFullPath($TargetPath)
    $directorySeparator = [System.IO.Path]::DirectorySeparatorChar
    $alternateSeparator = [System.IO.Path]::AltDirectorySeparatorChar

    if (-not $baseFullPath.EndsWith($directorySeparator) -and -not $baseFullPath.EndsWith($alternateSeparator)) {
        $baseFullPath += $directorySeparator
    }

    $baseUri = [Uri]$baseFullPath
    $targetUri = [Uri]$targetFullPath

    if ($baseUri.Scheme -ne $targetUri.Scheme) {
        return $targetFullPath
    }

    $relativeUri = $baseUri.MakeRelativeUri($targetUri)
    return [Uri]::UnescapeDataString($relativeUri.ToString()).Replace('/', $directorySeparator)
}

function Resolve-MakeAppx {
    $command = Get-Command makeappx.exe -ErrorAction SilentlyContinue

    if ($null -ne $command) {
        return $command.Source
    }

    $programFilesX86 = ${env:ProgramFiles(x86)}

    if ([string]::IsNullOrWhiteSpace($programFilesX86)) {
        $programFilesX86 = $env:ProgramFiles
    }

    if ([string]::IsNullOrWhiteSpace($programFilesX86)) {
        throw 'Unable to locate Program Files path for Windows SDK lookup.'
    }

    $windowsKitBin = Join-Path $programFilesX86 'Windows Kits\10\bin'

    if (Test-Path -LiteralPath $windowsKitBin) {
        $candidate = Get-ChildItem -LiteralPath $windowsKitBin -Directory |
            Sort-Object Name -Descending |
            ForEach-Object {
                Join-Path $_.FullName 'x64\makeappx.exe'
                Join-Path $_.FullName 'x86\makeappx.exe'
            } |
            Where-Object { Test-Path -LiteralPath $_ } |
            Select-Object -First 1

        if ($null -ne $candidate) {
            return $candidate
        }
    }

    throw 'makeappx.exe was not found. Install the Windows SDK or add makeappx.exe to PATH.'
}

function Find-MsixPackage {
    param(
        [Parameter(Mandatory)]
        [string]$SearchRoot,

        [Parameter(Mandatory)]
        [string]$ExpectedName,

        [Parameter(Mandatory)]
        [string]$PlatformToken
    )

    if (-not (Test-Path -LiteralPath $SearchRoot)) {
        throw "MSIX output directory was not found: $SearchRoot"
    }

    $packages = @(Get-ChildItem -LiteralPath $SearchRoot -Recurse -Filter '*.msix' |
        Where-Object { $_.Name -eq $ExpectedName } |
        Sort-Object LastWriteTimeUtc -Descending)

    if ($packages.Count -eq 0) {
        $packages = @(Get-ChildItem -LiteralPath $SearchRoot -Recurse -Filter '*.msix' |
            Where-Object { $_.Name -like "*_$PlatformToken.msix" } |
            Sort-Object LastWriteTimeUtc -Descending)
    }

    if ($packages.Count -eq 0) {
        throw "No MSIX package was found under '$SearchRoot'. Expected '$ExpectedName'."
    }

    if ($packages.Count -gt 1) {
        Write-Warning "Multiple MSIX packages matched '$ExpectedName'. Using newest: $($packages[0].FullName)"
    }

    return $packages[0]
}

$scriptRoot = if ([string]::IsNullOrWhiteSpace($PSScriptRoot)) {
    (Get-Location).Path
} else {
    $PSScriptRoot
}

$projectFullPath = ConvertTo-FullPath -Path $ProjectPath -BasePath $scriptRoot

if (-not (Test-Path -LiteralPath $projectFullPath)) {
    throw "Project file was not found: $projectFullPath"
}

$projectDirectory = Split-Path -Parent $projectFullPath
$manifestPath = Join-Path $projectDirectory 'Package.appxmanifest'

$projectXml = Read-XmlDocument -Path $projectFullPath

if ([string]::IsNullOrWhiteSpace($ExtensionName)) {
    $ExtensionName = Get-ProjectProperty -ProjectXml $projectXml -Name 'AssemblyName'

    if ([string]::IsNullOrWhiteSpace($ExtensionName)) {
        $ExtensionName = [System.IO.Path]::GetFileNameWithoutExtension($projectFullPath)
    }
}

$currentPackageVersion = Get-ProjectProperty -ProjectXml $projectXml -Name 'AppxPackageVersion'

if ([string]::IsNullOrWhiteSpace($currentPackageVersion)) {
    if (Test-Path -LiteralPath $manifestPath) {
        $currentPackageVersion = Get-ManifestPackageVersion -ManifestPath $manifestPath
    }
}

if ([string]::IsNullOrWhiteSpace($currentPackageVersion)) {
    throw 'Package version was not found. Set AppxPackageVersion in the project file or pass -PackageVersion.'
}

if ([string]::IsNullOrWhiteSpace($PackageVersion)) {
    if ($SkipVersionBump) {
        $PackageVersion = $currentPackageVersion
    } else {
        $PackageVersion = Get-IncrementedPackageVersion -Version $currentPackageVersion
    }
} else {
    ConvertTo-PackageVersionParts -Version $PackageVersion | Out-Null
}

if (-not $SkipVersionBump) {
    if (-not (Test-Path -LiteralPath $manifestPath)) {
        throw "Package manifest was not found: $manifestPath"
    }

    Update-PackageVersionFiles -ProjectPath $projectFullPath -ManifestPath $manifestPath -Version $PackageVersion
}

if ([string]::IsNullOrWhiteSpace($BundleDirectory)) {
    $BundleDirectory = Join-Path $projectDirectory 'AppPackages'
}

$bundleDirectoryFullPath = ConvertTo-FullPath -Path $BundleDirectory -BasePath $scriptRoot

if (-not (Test-Path -LiteralPath $bundleDirectoryFullPath)) {
    New-Item -Path $bundleDirectoryFullPath -ItemType Directory | Out-Null
}

$platformsToBuild = @($Platforms | ForEach-Object { ConvertTo-PackagePlatform -Platform $_ } | Select-Object -Unique)
$packageFiles = @()

Write-Host "Project: $projectFullPath"
Write-Host "Configuration: $Configuration"
Write-Host "Extension: $ExtensionName"
Write-Host "Version: $currentPackageVersion -> $PackageVersion"
Write-Host "Platforms: $($platformsToBuild -join ', ')"

foreach ($platform in $platformsToBuild) {
    $platformToken = $platform.ToLowerInvariant()
    $appxPackageDir = "AppPackages\$platform\"
    
    if (-not $SkipBuild) {
        Write-Host ''
        Write-Host "Building $platform MSIX..." -BackgroundColor DarkGreen
        Write-Host "`e[0m" -NoNewline
        & dotnet build $projectFullPath `
            --configuration $Configuration `
            "-p:GenerateAppxPackageOnBuild=true" `
            "-p:Platform=$platform" `
            "-p:AppxPackageDir=$appxPackageDir"

        if ($LASTEXITCODE -ne 0) {
            throw "dotnet build failed for $platform."
        }
    } else {
        Write-Host "Skipping $platform build."
    }

    $platformOutputDirectory = Join-Path $projectDirectory "AppPackages\$platform"
    $expectedMsixName = '{0}_{1}_{2}.msix' -f $ExtensionName, $PackageVersion, $platformToken
    $msixPackage = Find-MsixPackage -SearchRoot $platformOutputDirectory -ExpectedName $expectedMsixName -PlatformToken $platformToken

    $packageFiles += [PSCustomObject]@{
        Platform = $platform
        Token = $platformToken
        File = $msixPackage
    }
}

Write-Host ''
Write-Host 'MSIX packages:' -BackgroundColor DarkGreen
Write-Host "`e[0m" -NoNewline

foreach ($package in $packageFiles) {
    Write-Host "  $($package.File.FullName)"
}

if ($SkipBundle) {
    Write-Host ''
    Write-Host 'Skipping MSIX bundle creation.'
}

$makeAppxPath = Resolve-MakeAppx
$mappingPath = Join-Path $bundleDirectoryFullPath 'bundle_mapping.txt'
$bundleName = '{0}_{1}_Bundle.msixbundle' -f $ExtensionName, $PackageVersion
$bundlePath = Join-Path $bundleDirectoryFullPath $bundleName
$mappingLines = New-Object System.Collections.Generic.List[string]

[void]$mappingLines.Add('[Files]')

foreach ($package in $packageFiles) {
    $sourcePath = Get-RelativePath -BasePath $bundleDirectoryFullPath -TargetPath $package.File.FullName
    $destinationName = '{0}_{1}_{2}.msix' -f $ExtensionName, $PackageVersion, $package.Token
    [void]$mappingLines.Add(('"{0}" "{1}"' -f $sourcePath, $destinationName))
}

Set-Content -LiteralPath $mappingPath -Value $mappingLines -Encoding ASCII

Write-Host ''
Write-Host "Bundle mapping: $mappingPath" -BackgroundColor DarkGreen
Write-Host "`e[0m" -NoNewline
Write-Host "Creating MSIX bundle..."

Push-Location $bundleDirectoryFullPath

try {
    & $makeAppxPath bundle /v /o /f (Split-Path -Leaf $mappingPath) /p (Split-Path -Leaf $bundlePath)

    if ($LASTEXITCODE -ne 0) {
        throw 'makeappx bundle failed.'
    }
} finally {
    Pop-Location
}

Write-Host ''
Write-Host "Package outputs: $bundleDirectoryFullPath" -BackgroundColor DarkGreen
Write-Host "`e[0m" -NoNewline

foreach ($package in $packageFiles) {
    Write-Host "MSIX ($($package.Platform)): $($package.File.FullName)"
}
Write-Host "MSIX bundle: $bundlePath"

