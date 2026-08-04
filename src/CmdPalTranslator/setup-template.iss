; Inno Setup template for the CmdPalTranslator WinGet installer flow.
; build-winget-release.ps1 generates platform-specific .iss files from this template.

#define AppVersion "0.0.0.0"
#define AppId "{{00000000-0000-0000-0000-000000000000}}"
#define AppName "Translator for Command Palette"
#define ExtensionName "CmdPalTranslator"
#define PublisherName "Poy Chang"
#define PublisherUrl "https://github.com/poychang/CmdPalTranslator"
#define SourceDir "bin\Release\win-x64\publish"
#define OutputDir "bin\Release\installer"
#define Platform "x64"
#define ArchitecturesAllowed "x64compatible"
#define ArchitecturesInstallIn64BitMode "x64compatible"
#define Clsid "{{60fe7a20-f163-4bd8-909c-d3d23f2df6ea}}"

[Setup]
AppId={#AppId}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#PublisherName}
AppPublisherURL={#PublisherUrl}
AppSupportURL={#PublisherUrl}/issues
AppUpdatesURL={#PublisherUrl}/releases
PrivilegesRequired=lowest
DefaultDirName={autopf}\{#ExtensionName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir={#OutputDir}
OutputBaseFilename={#ExtensionName}-Setup-{#AppVersion}-{#Platform}
Compression=lzma
SolidCompression=yes
MinVersion=10.0.19041
ArchitecturesAllowed={#ArchitecturesAllowed}
ArchitecturesInstallIn64BitMode={#ArchitecturesInstallIn64BitMode}
UninstallDisplayIcon={app}\{#ExtensionName}.exe
VersionInfoVersion={#AppVersion}
VersionInfoCompany={#PublisherName}
VersionInfoDescription={#AppName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#ExtensionName}.exe"

[Registry]
Root: HKCU; Subkey: "SOFTWARE\Classes\CLSID\{#Clsid}"; ValueType: string; ValueName: ""; ValueData: "{#ExtensionName}"; Flags: uninsdeletekey
Root: HKCU; Subkey: "SOFTWARE\Classes\CLSID\{#Clsid}\LocalServer32"; ValueType: string; ValueName: ""; ValueData: """{app}\{#ExtensionName}.exe"" -RegisterProcessAsComServer"; Flags: uninsdeletekey
