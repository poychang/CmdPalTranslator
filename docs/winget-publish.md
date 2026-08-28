# WinGet 發布準備流程

本流程參考 Microsoft Learn 的 Command Palette 擴充功能 WinGet 發布文件：
https://learn.microsoft.com/zh-tw/windows/powertoys/command-palette/publish-extension-winget

## 正式發布前準備

1. 確認本機或 CI 具備必要工具：
   - .NET SDK 10.x，因為此專案目前使用 `net10.0-windows10.0.26100.0`
   - Inno Setup 6，用來產生 EXE 安裝檔
   - GitHub CLI，方便觸發 workflow 與檢查 release
   - WingetCreate，用來建立或更新 WinGet manifest
2. 確認專案 metadata：
   - 版本：`src/CmdPalTranslator/CmdPalTranslator.csproj` 的 `AppxPackageVersion`
   - WinGet PackageIdentifier 候選值：`25526PoyChang.TranslatorforCommandPalette`
   - CLSID：`60fe7a20-f163-4bd8-909c-d3d23f2df6ea`
   - 顯示名稱：`Translator for Command Palette`
   - Publisher：`Poy Chang`
3. 建置 x64 與 arm64 的 unpackaged EXE，並用 Inno Setup 產生：
   - `CmdPalTranslator-Setup-<version>-x64.exe`
   - `CmdPalTranslator-Setup-<version>-arm64.exe`
4. 將兩個 EXE 上傳到 GitHub Release，WinGet manifest 需要公開可下載的 installer URL。
5. 首次提交 WinGet 必須手動執行 `wingetcreate new`，因為它會互動式詢問 package 欄位。
6. 送出 PR 前要檢查 WinGet manifest：
   - 每個 `.locale.*.yaml` 要有 `Tags: windows-commandpalette-extension`
   - `.installer.yaml` 若未自動產生 Windows App Runtime 相依性，補上 `Microsoft.WindowsAppRuntime.1.8`
   - `.installer.yaml` 若沒有 `Scope: user`，手動補上；因為安裝檔（`setup-template.iss`）使用 `PrivilegesRequired=lowest`，本來就不需要系統管理員權限，加上此欄位可讓 Winget 正確辨識，避免安裝時被要求提升權限

## 本機手動流程

從 repo 根目錄執行：

```powershell
.\build-winget-release.ps1
```

只檢查 metadata 與輸出 WinGet checklist，不建置 installer：

```powershell
.\build-winget-release.ps1 -MetadataOnly
```

指定版本：

```powershell
.\build-winget-release.ps1 -Version 0.2.3.0
```

腳本會輸出 installer 路徑、預期 GitHub Release URL，以及首次送 WinGet 的 `wingetcreate new` 指令。

首次 GitHub Release 發布後，使用公開 asset URL 執行：

```powershell
wingetcreate new "https://github.com/poychang/CmdPalTranslator/releases/download/CmdPalTranslator-v0.2.3.0/CmdPalTranslator-Setup-0.2.3.0-x64.exe" "https://github.com/poychang/CmdPalTranslator/releases/download/CmdPalTranslator-v0.2.3.0/CmdPalTranslator-Setup-0.2.3.0-arm64.exe"
```

`wingetcreate` 建立 manifest 後，確認 tag 與 dependency，再提交到 `microsoft/winget-pkgs`。

## GitHub Actions 流程

`Build and Release Installers` workflow 會：

1. 安裝 .NET 10
2. 安裝 Inno Setup
3. 還原套件
4. 執行 Unit tests
5. 呼叫 `build-winget-release.ps1` 建置 x64/arm64 EXE installer
6. 上傳 workflow artifacts
7. 依輸入建立 GitHub Release 並附上 installer assets

手動觸發範例：

```powershell
gh workflow run build-and-release-installers.yml --ref main -f version=0.2.3.0 -f create_release=true -f "release_notes=Release notes here"
```

`Submit WinGet Manifest` workflow 會在 GitHub Release published 後執行，使用 `wingetcreate update` 更新既有 WinGet package。這個 workflow 只有在首次 WinGet PR 已合併後才適用。

建議設定：

- Repository variable `WINGET_PACKAGE_IDENTIFIER`：填入首次 WinGet PR 最終使用的 PackageIdentifier。
- Repository secret `WINGET_TOKEN`：必須使用可對 `microsoft/winget-pkgs` 建立 PR 的 GitHub PAT；內建的 `GITHUB_TOKEN` 無法跨 repository 提交更新。
