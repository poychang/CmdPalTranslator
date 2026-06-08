# 01-upgrade-all-projects: 升級所有專案到 net10 並修正相容性

在單一原子化升級作業中，更新 `CmdPalTranslator.Core` 與 `CmdPalTranslator` 的 Target Framework 到 net10 家族，並檢查 `CmdPalTranslator.Tests` 對應目標是否維持一致。此任務同時涵蓋相依套件調整，針對 assessment 指出的 2 個不相容 NuGet 套件完成替代或版本修正，確保還原與編譯路徑在 .NET 10 下可用。

同一任務中也會處理 assessment 指出的 source-incompatible API 變更（TimeSpan.FromSeconds 相關）與行為變更風險點（Uri/HttpContent），以「Fix Inline / Resolve Inline」方式一次完成，不建立延後子任務。

## Scope Inventory

- **Projects affected**:
  - `src/CmdPalTranslator.Core/CmdPalTranslator.Core.csproj`
  - `src/CmdPalTranslator/CmdPalTranslator.csproj`
  - `src/CmdPalTranslator.Tests/CmdPalTranslator.Tests.csproj`（驗證一致性，不預期修改）
- **Distinct concerns**:
  - TargetFramework 升級（Core 與 App）
  - 不相容套件處理（`Shmuelie.WinRTServer` 降版；`Microsoft.CommandPalette.Extensions` 驗證相容）
  - API 相容修正（TimeSpan.FromSeconds 呼叫點）
- **Assessment signals**:
  - `CmdPalTranslator.Core`: 25 issues（1 mandatory + 24 potential），TFM net9.0 → net10.0
  - `CmdPalTranslator`: 5 issues（3 mandatory + 2 potential），TFM net9.0-windows10.0.26100.0 → net10.0-windows
  - `CmdPalTranslator.Tests`: 0 issues，已為 net10.0
- **Package research**:
  - `Shmuelie.WinRTServer` 在 net10.0-windows 建議版本為 `1.3.1`
  - `Microsoft.CommandPalette.Extensions` 在 net10.0-windows10.0.26100.0 可用版本為 `0.9.260303001`（現有版本可保留）
- **Stub discovery**:
  - 已掃描 `src/**/*.cs` 的 `// STUB:`，未發現既有 stub。

## Execution Notes

- 保持專案為單一目標（不引入 multi-targeting）。
- `CmdPalTranslator` 採用 `net10.0-windows10.0.26100.0` 以維持現有 Windows SDK 版本一致性。
- 套件維持 per-project/CPM 現況，不新增 CPM 結構變更。

**Done when**: 所有目標專案的 TFM 已更新為 net10 對應值、套件還原成功、解決方案可完成編譯且無錯誤。
