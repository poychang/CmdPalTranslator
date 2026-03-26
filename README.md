# CmdPalTranslator

`CmdPalTranslator` 是一個提供給 Windows Command Palette 使用的翻譯擴充功能。它把文字翻譯流程直接帶進 Command Palette，讓你可以在同一個操作介面裡輸入文字、切換翻譯來源、複製結果，必要時再跳轉到原始翻譯網站。

目前專案內建 `Bing` 與 `Google` 兩個翻譯 provider，預設目標語言可在設定頁中調整，初始值為繁體中文（`zht`），並支援用 `text -> lang` 的方式快速指定目標語言。

## 主要功能特性

- 整合 Windows Command Palette，以 extension 形式提供翻譯命令。
- 內建 `Bing`、`Google` 兩種翻譯來源，可透過篩選器即時切換。
- 支援自動偵測來源語言。
- 支援 `text -> languageCode` 查詢語法，直接在輸入框指定目標語言。
- 提供設定頁，可自訂預設翻譯目標語言。
- 翻譯結果可直接複製到剪貼簿。
- 提供複製原文與開啟 Bing / Google 翻譯網頁等額外操作。
- 內建 Supported Languages 頁面，可查詢語言代碼並一鍵複製。
- 針對 provider 端點實作單元測試與 live 測試，驗證中英雙向翻譯流程。

## 使用簡介

1. 先將此專案建置並部署到本機，讓它能被 Windows Command Palette 載入為 extension。
2. 開啟 Windows Command Palette，搜尋 `Translator`。
3. 直接輸入要翻譯的文字，例如：

```text
hello world
```

若未指定語言，系統會自動偵測來源語言，並翻譯成你在設定頁選定的預設目標語言。

4. 若要指定目標語言，使用 `->` 語法，例如：

```text
hello world -> ja
open source software -> fr
今天天氣很好 -> en
```

5. 使用頁面上方的 filter 在 `Bing` 與 `Google` 之間切換翻譯來源。
6. 若要調整預設目標語言，進入 `Target language` 設定頁後選取語言即可儲存。
7. 選取翻譯結果後即可複製內容；更多操作中還可以複製原文或開啟對應的翻譯網站。

## 支援語言

目前專案內建以下語言代碼：

- `auto` 自動偵測
- `zhs` 簡體中文
- `zht` 繁體中文
- `en` 英文
- `ja` 日文
- `ko` 韓文
- `fr` 法文
- `de` 德文
- `es` 西班牙文
- `it` 義大利文
- `ru` 俄文
- `ar` 阿拉伯文
- `he` 希伯來文
- `pt` 葡萄牙文
- `th` 泰文

在 Command Palette 中也可以開啟 `Supported Languages` 頁面，直接瀏覽與複製語言代碼。

## 專案結構

- `CmdPalTranslator/`：Command Palette extension 主程式。
- `CmdPalTranslator/Providers/`：Bing、Google 翻譯 provider 實作。
- `CmdPalTranslator/Pages/`：翻譯頁面與支援語言頁面。
- `CmdPalTranslator/Services/`：查詢解析與 provider 管理。
- `Translator.ProviderTests/`：provider 單元測試與 live 測試。

## 開發基本起手式

### 1. 開發環境

建議準備以下環境：

- Windows 11
- Visual Studio 2022（含 .NET / Windows 開發工作負載）
- 可建置 Windows App / MSIX 專案的 SDK 與工具鏈
- 可連網環境（翻譯 provider 會呼叫線上服務）

### 2. 還原與建置

請在 Windows 終端機或 Visual Studio 環境中，於專案根目錄執行：

```bash
dotnet build CmdPalTranslator.sln
```

若只想建置主專案：

```bash
dotnet build CmdPalTranslator/CmdPalTranslator.csproj
```

### 3. 執行測試

執行 provider 測試：

```bash
dotnet test --project Translator.ProviderTests/Translator.ProviderTests.csproj
```

### 4. 建置 MSIX

移動到 `CmdPalTranslator\CmdPalTranslator` 目錄，用以下指令建立 x64 版本的 MSIX

```bash
dotnet build --configuration Release -p:GenerateAppxPackageOnBuild=true -p:Platform=x64 -p:AppxPackageDir="AppPackages\x64\"
```

完成後，可以在 `CmdPalTranslator\CmdPalTranslator\AppPackages\x64\` 目錄中找到相關 MSIX 安裝檔。

### 5. 發佈到 Marketplace

請至 [Microsoft Partner Center](https://partner.microsoft.com/dashboard/home) 發佈至 Microsoft Store，或直接提交到 WinGet。詳細流程請參考官方 [Command Palette 擴充功能發佈指南](https://learn.microsoft.com/en-us/windows/powertoys/command-palette/publish-extension)。

## 備註

- 此專案目前透過公開 Web endpoint 存取 Bing / Google 翻譯能力。
- Bing 與 Google 的語言代碼不完全相同，專案已在 `LanguageCatalog` 中做 provider 對應。
- 若未來要增加新的翻譯來源，可沿用 `ITranslatorProvider` 介面擴充。
