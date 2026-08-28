<#
.SYNOPSIS
Building WinGet release assets for CmdPalTranslator.

.DESCRIPTION
This script follows the PowerToys Command Palette WinGet publishing flow:
1. Reads project and package metadata.
2. Checks local prerequisites for .NET, Inno Setup, GitHub CLI, and WingetCreate.
3. Publishes unpackaged self-contained x64 and arm64 builds.
4. Builds Inno Setup EXE installers.
5. Prints the GitHub Release asset URLs and WinGet submission checklist.

The first WinGet submission is intentionally not automated because wingetcreate new
is interactive and the generated manifest still needs review.

.EXAMPLE
.\build-winget-release.ps1

.EXAMPLE
.\build-winget-release.ps1 -Version 0.2.3.0 -Platforms x64

.EXAMPLE
.\build-winget-release.ps1 -SkipBuild -SkipInstaller

.EXAMPLE
.\build-winget-release.ps1 -MetadataOnly
#>

#requires -Version 5.1

[CmdletBinding()]
param(
    [string]$ProjectPath = 'src\CmdPalTranslator\CmdPalTranslator.csproj',

    [string]$Configuration = 'Release',

    [ValidateSet('x64', 'arm64')]
    [string[]]$Platforms = @('x64', 'arm64'),

    [string]$Version,

    [string]$GitHubUser = 'poychang',

    [string]$GitHubRepo = 'CmdPalTranslator',

    [string]$ReleaseTag,

    [switch]$SkipBuild,

    [switch]$SkipInstaller,

    [switch]$SkipPrerequisiteCheck,

    [switch]$NoClean,

    [switch]$MetadataOnly
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

function Get-ManifestIdentity {
    param(
        [Parameter(Mandatory)]
        [xml]$ManifestXml
    )

    $namespaceManager = New-Object System.Xml.XmlNamespaceManager($ManifestXml.NameTable)
    $namespaceManager.AddNamespace('appx', $ManifestXml.DocumentElement.NamespaceURI)
    $identity = $ManifestXml.SelectSingleNode('/appx:Package/appx:Identity', $namespaceManager)

    if ($null -eq $identity) {
        throw 'Identity was not found in Package.appxmanifest.'
    }

    return $identity
}

function Get-ManifestPropertyText {
    param(
        [Parameter(Mandatory)]
        [xml]$ManifestXml,

        [Parameter(Mandatory)]
        [string]$PropertyName
    )

    $node = $ManifestXml.SelectSingleNode("//*[local-name()='Properties']/*[local-name()='$PropertyName']")

    if ($null -eq $node) {
        return $null
    }

    return $node.InnerText.Trim()
}

function Get-CommandPaletteClsid {
    param(
        [Parameter(Mandatory)]
        [xml]$ManifestXml,

        [Parameter(Mandatory)]
        [string]$ProjectDirectory
    )

    $createInstance = $ManifestXml.SelectSingleNode("//*[local-name()='CreateInstance']")

    if ($null -ne $createInstance) {
        $classId = $createInstance.GetAttribute('ClassId')

        if (-not [string]::IsNullOrWhiteSpace($classId)) {
            return $classId.Trim()
        }
    }

    $sourceFiles = Get-ChildItem -LiteralPath $ProjectDirectory -Filter '*.cs' -File

    foreach ($sourceFile in $sourceFiles) {
        $text = [System.IO.File]::ReadAllText($sourceFile.FullName)
        $match = [regex]::Match($text, '\[Guid\("(?<guid>[0-9a-fA-F-]{36})"\)\]')

        if ($match.Success) {
            return $match.Groups['guid'].Value
        }
    }

    throw 'Command Palette CLSID was not found in Package.appxmanifest or project source files.'
}

function Get-WindowsAppRuntimeDependency {
    param(
        [Parameter(Mandatory)]
        [string]$SourceRoot
    )

    $packagesPropsPath = Join-Path $SourceRoot 'Directory.Packages.props'

    if (-not (Test-Path -LiteralPath $packagesPropsPath)) {
        return $null
    }

    $packagesXml = Read-XmlDocument -Path $packagesPropsPath
    $packageNode = $packagesXml.SelectSingleNode("//PackageVersion[@Include='Microsoft.WindowsAppSDK']")

    if ($null -eq $packageNode) {
        return $null
    }

    $sdkVersion = $packageNode.GetAttribute('Version')
    $match = [regex]::Match($sdkVersion, '^(?<major>\d+)\.(?<minor>\d+)\.')

    if (-not $match.Success) {
        return $null
    }

    return [PSCustomObject]@{
        SdkVersion = $sdkVersion
        PackageIdentifier = 'Microsoft.WindowsAppRuntime.{0}.{1}' -f $match.Groups['major'].Value, $match.Groups['minor'].Value
    }
}

function Resolve-InnoSetup {
    $command = Get-Command iscc.exe -ErrorAction SilentlyContinue

    if ($null -ne $command) {
        return $command.Source
    }

    $candidates = @()

    if (-not [string]::IsNullOrWhiteSpace(${env:ProgramFiles(x86)})) {
        $candidates += (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\iscc.exe')
    }

    if (-not [string]::IsNullOrWhiteSpace($env:ProgramFiles)) {
        $candidates += (Join-Path $env:ProgramFiles 'Inno Setup 6\iscc.exe')
    }

    if (-not [string]::IsNullOrWhiteSpace($env:LOCALAPPDATA)) {
        $candidates += (Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\iscc.exe')
    }

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) {
            return $candidate
        }
    }

    return $null
}

function Assert-ChildPath {
    param(
        [Parameter(Mandatory)]
        [string]$BasePath,

        [Parameter(Mandatory)]
        [string]$TargetPath
    )

    $baseFullPath = [System.IO.Path]::GetFullPath($BasePath).TrimEnd('\', '/')
    $targetFullPath = [System.IO.Path]::GetFullPath($TargetPath).TrimEnd('\', '/')
    $comparison = [StringComparison]::OrdinalIgnoreCase

    if (-not $targetFullPath.StartsWith($baseFullPath + [System.IO.Path]::DirectorySeparatorChar, $comparison) -and
        -not $targetFullPath.Equals($baseFullPath, $comparison)) {
        throw "Refusing to operate outside '$baseFullPath': $targetFullPath"
    }
}

function Invoke-NativeCommand {
    param(
        [Parameter(Mandatory)]
        [string]$FilePath,

        [Parameter(Mandatory)]
        [string[]]$Arguments,

        [Parameter(Mandatory)]
        [string]$FailureMessage
    )

    & $FilePath @Arguments

    if ($LASTEXITCODE -ne 0) {
        throw "$FailureMessage Exit code: $LASTEXITCODE"
    }
}

function Get-PlatformSettings {
    param(
        [Parameter(Mandatory)]
        [string]$Platform
    )

    switch ($Platform.ToLowerInvariant()) {
        'x64' {
            return [PSCustomObject]@{
                Token = 'x64'
                RuntimeIdentifier = 'win-x64'
                MsBuildPlatform = 'x64'
                InnoArchitecturesAllowed = 'x64compatible'
                InnoArchitecturesInstallIn64BitMode = 'x64compatible'
            }
        }
        'arm64' {
            return [PSCustomObject]@{
                Token = 'arm64'
                RuntimeIdentifier = 'win-arm64'
                MsBuildPlatform = 'ARM64'
                InnoArchitecturesAllowed = 'arm64'
                InnoArchitecturesInstallIn64BitMode = 'arm64'
            }
        }
        default {
            throw "Unsupported platform '$Platform'."
        }
    }
}

function ConvertTo-InnoString {
    param(
        [AllowNull()]
        [string]$Value
    )

    if ($null -eq $Value) {
        return ''
    }

    return $Value.Replace('"', '""')
}

function Set-InnoDefine {
    param(
        [Parameter(Mandatory)]
        [string]$Text,

        [Parameter(Mandatory)]
        [string]$Name,

        [AllowNull()]
        [string]$Value
    )

    $pattern = '^(?<prefix>#define\s+' + [regex]::Escape($Name) + '\s+").*(?<suffix>"\s*)$'
    $regex = New-Object System.Text.RegularExpressions.Regex($pattern, [System.Text.RegularExpressions.RegexOptions]::Multiline)
    $matches = @($regex.Matches($Text))

    if ($matches.Count -ne 1) {
        throw "Expected exactly one Inno define named '$Name', but found $($matches.Count)."
    }

    $escapedValue = ConvertTo-InnoString -Value $Value
    return $regex.Replace(
        $Text,
        [System.Text.RegularExpressions.MatchEvaluator]{
            param($match)
            return "$($match.Groups['prefix'].Value)$escapedValue$($match.Groups['suffix'].Value)"
        },
        1
    )
}

function Write-Utf8NoBomFile {
    param(
        [Parameter(Mandatory)]
        [string]$Path,

        [Parameter(Mandatory)]
        [string]$Text
    )

    $encoding = New-Object System.Text.UTF8Encoding($false)
    $writer = New-Object System.IO.StreamWriter($Path, $false, $encoding)

    try {
        $writer.Write($Text)
    } finally {
        $writer.Dispose()
    }
}

function New-PlatformSetupScript {
    param(
        [Parameter(Mandatory)]
        [string]$TemplatePath,

        [Parameter(Mandatory)]
        [string]$OutputPath,

        [Parameter(Mandatory)]
        [hashtable]$Defines
    )

    $text = [System.IO.File]::ReadAllText($TemplatePath)

    foreach ($defineName in $Defines.Keys) {
        $text = Set-InnoDefine -Text $text -Name $defineName -Value $Defines[$defineName]
    }

    Write-Utf8NoBomFile -Path $OutputPath -Text $text
}

function Get-ExecutableName {
    param(
        [Parameter(Mandatory)]
        [xml]$ProjectXml,

        [Parameter(Mandatory)]
        [string]$ProjectPath
    )

    $assemblyName = Get-ProjectProperty -ProjectXml $ProjectXml -Name 'AssemblyName'

    if ([string]::IsNullOrWhiteSpace($assemblyName)) {
        return [System.IO.Path]::GetFileNameWithoutExtension($ProjectPath)
    }

    return $assemblyName
}

function Test-Prerequisites {
    param(
        [AllowNull()]
        [string]$InnoSetupPath
    )

    Write-Host 'Prerequisite check:'

    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($null -eq $dotnet) {
        throw 'dotnet was not found. Install the .NET SDK required by this project.'
    }

    $dotnetVersion = (& dotnet --version)
    Write-Host "  dotnet: $dotnetVersion"

    if ([string]::IsNullOrWhiteSpace($InnoSetupPath)) {
        throw 'Inno Setup 6 was not found. Install it from https://jrsoftware.org/isdl.php or with Chocolatey in CI.'
    }

    Write-Host "  Inno Setup: $InnoSetupPath"

    $wingetCreate = Get-Command wingetcreate -ErrorAction SilentlyContinue
    if ($null -eq $wingetCreate) {
        Write-Warning 'wingetcreate was not found. Install it with: winget install Microsoft.WingetCreate'
    } else {
        Write-Host "  wingetcreate: $($wingetCreate.Source)"
    }

    $gh = Get-Command gh -ErrorAction SilentlyContinue
    if ($null -eq $gh) {
        Write-Warning 'GitHub CLI was not found. Install it if you want to trigger workflows or inspect releases locally.'
    } else {
        Write-Host "  GitHub CLI: $($gh.Source)"
    }
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
$sourceRoot = Split-Path -Parent $projectDirectory
$manifestPath = Join-Path $projectDirectory 'Package.appxmanifest'
$setupTemplatePath = Join-Path $projectDirectory 'setup-template.iss'
$generatedSetupDirectory = Join-Path $projectDirectory 'obj\winget'
$installerOutputDirectory = Join-Path $projectDirectory "bin\$Configuration\installer"
$projectXml = Read-XmlDocument -Path $projectFullPath
$manifestXml = Read-XmlDocument -Path $manifestPath
$identity = Get-ManifestIdentity -ManifestXml $manifestXml
$extensionName = Get-ExecutableName -ProjectXml $projectXml -ProjectPath $projectFullPath
$displayName = Get-ManifestPropertyText -ManifestXml $manifestXml -PropertyName 'DisplayName'
$publisherDisplayName = Get-ManifestPropertyText -ManifestXml $manifestXml -PropertyName 'PublisherDisplayName'
$appxPackageIdentityName = Get-ProjectProperty -ProjectXml $projectXml -Name 'AppxPackageIdentityName'
$currentVersion = Get-ProjectProperty -ProjectXml $projectXml -Name 'AppxPackageVersion'
$targetFramework = Get-ProjectProperty -ProjectXml $projectXml -Name 'TargetFramework'
$classId = Get-CommandPaletteClsid -ManifestXml $manifestXml -ProjectDirectory $projectDirectory
$windowsAppRuntimeDependency = Get-WindowsAppRuntimeDependency -SourceRoot $sourceRoot
$innoSetupPath = Resolve-InnoSetup

if ([string]::IsNullOrWhiteSpace($displayName)) {
    $displayName = $extensionName
}

if ([string]::IsNullOrWhiteSpace($publisherDisplayName)) {
    $publisherDisplayName = 'Poy Chang'
}

if ([string]::IsNullOrWhiteSpace($appxPackageIdentityName)) {
    $appxPackageIdentityName = $identity.GetAttribute('Name')
}

if ([string]::IsNullOrWhiteSpace($currentVersion)) {
    $currentVersion = $identity.GetAttribute('Version')
}

if ([string]::IsNullOrWhiteSpace($Version)) {
    $Version = $currentVersion
}

if ($Version -notmatch '^\d+\.\d+\.\d+\.\d+$') {
    throw "Version '$Version' must use four numeric parts, for example 0.2.3.0."
}

if ([string]::IsNullOrWhiteSpace($ReleaseTag)) {
    $ReleaseTag = "$extensionName-v$Version"
}

if (-not (Test-Path -LiteralPath $setupTemplatePath)) {
    throw "Inno Setup template was not found: $setupTemplatePath"
}

if (-not $SkipPrerequisiteCheck -and -not $MetadataOnly) {
    Test-Prerequisites -InnoSetupPath $innoSetupPath
}

Write-Host ''
Write-Host 'Project metadata:'
Write-Host "  Project: $projectFullPath"
Write-Host "  Extension executable: $extensionName.exe"
Write-Host "  Display name: $displayName"
Write-Host "  Publisher: $publisherDisplayName"
Write-Host "  Target framework: $targetFramework"
Write-Host "  Version: $Version"
Write-Host "  Command Palette CLSID: $classId"
Write-Host "  WinGet package identifier candidate: $appxPackageIdentityName"

if ($null -ne $windowsAppRuntimeDependency) {
    Write-Host "  Windows App Runtime dependency candidate: $($windowsAppRuntimeDependency.PackageIdentifier)"
}

if (-not $MetadataOnly -and -not (Test-Path -LiteralPath $generatedSetupDirectory)) {
    New-Item -Path $generatedSetupDirectory -ItemType Directory | Out-Null
}

if (-not $MetadataOnly -and -not (Test-Path -LiteralPath $installerOutputDirectory)) {
    New-Item -Path $installerOutputDirectory -ItemType Directory | Out-Null
}

$installerFiles = New-Object System.Collections.Generic.List[object]

if ($MetadataOnly) {
    Write-Host ''
    Write-Host 'MetadataOnly was specified. Skipping publish and installer creation.'
} elseif (-not $SkipBuild) {
    Write-Host ''
    Write-Host 'Restoring NuGet packages...'
    Invoke-NativeCommand -FilePath 'dotnet' -Arguments @('restore', $projectFullPath) -FailureMessage 'dotnet restore failed.'
}

if (-not $MetadataOnly) {
foreach ($platform in $Platforms) {
    $settings = Get-PlatformSettings -Platform $platform
    $publishDirectory = Join-Path $projectDirectory ("bin\$Configuration\$($settings.RuntimeIdentifier)\publish")
    $setupScriptPath = Join-Path $generatedSetupDirectory ("setup-$($settings.Token).iss")

    if (-not $NoClean -and -not $SkipBuild -and (Test-Path -LiteralPath $publishDirectory)) {
        Assert-ChildPath -BasePath (Join-Path $projectDirectory 'bin') -TargetPath $publishDirectory
        Remove-Item -LiteralPath $publishDirectory -Recurse -Force
    }

    if (-not $SkipBuild) {
        Write-Host ''
        Write-Host "Publishing $($settings.Token) unpackaged EXE..."
        Invoke-NativeCommand -FilePath 'dotnet' -Arguments @(
            'publish',
            $projectFullPath,
            '--configuration',
            $Configuration,
            '--runtime',
            $settings.RuntimeIdentifier,
            '--self-contained',
            'true',
            '--output',
            $publishDirectory,
            "-p:Platform=$($settings.MsBuildPlatform)",
            '-p:WindowsPackageType=None',
            '-p:EnableMsixTooling=false',
            '-p:PublishProfile=',
            '-p:PublishSingleFile=false'
        ) -FailureMessage "dotnet publish failed for $($settings.Token)."
    } else {
        Write-Host ''
        Write-Host "Skipping $($settings.Token) publish."
    }

    $publishedExe = Join-Path $publishDirectory "$extensionName.exe"
    if (-not (Test-Path -LiteralPath $publishedExe)) {
        throw "Published executable was not found: $publishedExe"
    }

    $defines = @{
        AppVersion = $Version
        AppId = '{{d5a9f05f-a6d4-4da0-85be-7ff53bb67d08}}'
        AppName = $displayName
        ExtensionName = $extensionName
        PublisherName = $publisherDisplayName
        PublisherUrl = "https://github.com/$GitHubUser/$GitHubRepo"
        SourceDir = $publishDirectory
        OutputDir = $installerOutputDirectory
        Platform = $settings.Token
        ArchitecturesAllowed = $settings.InnoArchitecturesAllowed
        ArchitecturesInstallIn64BitMode = $settings.InnoArchitecturesInstallIn64BitMode
        Clsid = "{{$classId}}"
    }

    New-PlatformSetupScript -TemplatePath $setupTemplatePath -OutputPath $setupScriptPath -Defines $defines

    if (-not $SkipInstaller) {
        if ([string]::IsNullOrWhiteSpace($innoSetupPath)) {
            throw 'Inno Setup is required to create installers. Re-run with -SkipInstaller to only validate publish outputs.'
        }

        Write-Host "Building $($settings.Token) installer..."
        Invoke-NativeCommand -FilePath $innoSetupPath -Arguments @($setupScriptPath) -FailureMessage "Inno Setup failed for $($settings.Token)."
    } else {
        Write-Host "Skipping $($settings.Token) installer build."
    }

    $installerPath = Join-Path $installerOutputDirectory ("$extensionName-Setup-$Version-$($settings.Token).exe")
    if (-not $SkipInstaller -and -not (Test-Path -LiteralPath $installerPath)) {
        throw "Installer was not found: $installerPath"
    }

    if (Test-Path -LiteralPath $installerPath) {
        $installer = Get-Item -LiteralPath $installerPath
        $installerFiles.Add([PSCustomObject]@{
            Platform = $settings.Token
            Path = $installer.FullName
            FileName = $installer.Name
            SizeMB = [math]::Round($installer.Length / 1MB, 2)
        }) | Out-Null
    }
}
}

Write-Host ''
Write-Host 'Release outputs:'
if ($installerFiles.Count -eq 0) {
    Write-Host '  No installer EXE files were created in this run.'
} else {
    foreach ($installer in $installerFiles) {
        Write-Host "  $($installer.Platform): $($installer.Path) ($($installer.SizeMB) MB)"
    }
}

Write-Host ''
Write-Host 'GitHub Release asset URLs expected by WinGet:'
foreach ($platform in @('x64', 'arm64')) {
    $fileName = "$extensionName-Setup-$Version-$platform.exe"
    $url = "https://github.com/$GitHubUser/$GitHubRepo/releases/download/$ReleaseTag/$fileName"
    Write-Host "  ${platform}: $url"
}

Write-Host ''
Write-Host 'Manual first WinGet submission after the GitHub Release is published:'
Write-Host "  wingetcreate new `"https://github.com/$GitHubUser/$GitHubRepo/releases/download/$ReleaseTag/$extensionName-Setup-$Version-x64.exe`" `"https://github.com/$GitHubUser/$GitHubRepo/releases/download/$ReleaseTag/$extensionName-Setup-$Version-arm64.exe`""
Write-Host ''
Write-Host 'Required manifest review before submitting the WinGet PR:'
Write-Host '  1. Add Tags: windows-commandpalette-extension to each .locale.*.yaml manifest.'
if ($null -ne $windowsAppRuntimeDependency) {
    Write-Host "  2. Add installer dependency: $($windowsAppRuntimeDependency.PackageIdentifier) to the .installer.yaml manifest if the generated manifest does not include it."
} else {
    Write-Host '  2. If this package uses Windows App SDK, add the matching Microsoft.WindowsAppRuntime.#.# dependency to the .installer.yaml manifest.'
}
Write-Host "  3. Confirm the WinGet PackageIdentifier. Current candidate: $appxPackageIdentityName"
Write-Host '  4. Add Scope: user to the .installer.yaml manifest if it is missing (installer uses PrivilegesRequired=lowest, so it does not need admin rights).'
