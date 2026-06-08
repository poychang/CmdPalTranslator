# 01-upgrade-all-projects 進度明細

## 實際變更

- `src/CmdPalTranslator.Core/CmdPalTranslator.Core.csproj`
  - `TargetFramework`：`net9.0` → `net10.0`
- `src/CmdPalTranslator/CmdPalTranslator.csproj`
  - `TargetFramework`：`net9.0-windows10.0.26100.0` → `net10.0-windows10.0.26100.0`
- `src/CmdPalTranslator.Core/Providers/TranslatorHttpClient.cs`
  - `TimeSpan.FromSeconds(30)` 改為 `TimeSpan.FromSeconds(30d)`
  - `Math.Pow(2, retryAttempt)` 改為 `Math.Pow(2d, retryAttempt)`，明確使用 double overload

## 套件處理

- 依 assessment 曾嘗試將 `Shmuelie.WinRTServer` 調整為 `1.3.1`，但造成 `Shmuelie.WinRTServer.CsWinRT` 與 `Microsoft.CommandPalette.Extensions` 相關型別無法解析（CS0234/CS0246）。
- 已回復 `src/Directory.Packages.props` 中 `Shmuelie.WinRTServer` 版本為 `2.2.1`，恢復可編譯狀態。
- `Microsoft.CommandPalette.Extensions` 維持 `0.9.260303001`（在 `net10.0-windows10.0.26100.0` 下可 restore/build）。

## 驗證結果

- Build：`run_build` 成功（整個 solution 建置成功）
- Tests：`CmdPalTranslator.Tests` 共 21 筆，**21 passed / 0 failed**

## Done when 對照

- [x] 所有目標專案 TFM 已更新為 net10 對應值
- [x] 套件還原成功
- [x] 解決方案可完成編譯且無錯誤
