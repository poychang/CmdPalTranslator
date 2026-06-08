# .NET Version Upgrade Plan

## Overview

**Target**: 將 CmdPalTranslator 解決方案中的所有專案升級並統一到 net10.0（含 Windows TFM 對應）。
**Scope**: 3 個專案、約 2k LOC，小型且低風險的 modern-to-modern 升級。

### Selected Strategy
**All-At-Once** — All projects upgraded simultaneously in a single operation.  
**Rationale**: 3 個專案皆為 SDK-style、目前為 net9/net10，依賴關係清楚（Core 為下游共用），適合一次性升級。

## Tasks

### 01-upgrade-all-projects: 升級所有專案到 net10 並修正相容性

在單一原子化升級作業中，更新 `CmdPalTranslator.Core` 與 `CmdPalTranslator` 的 Target Framework 到 net10 家族，並檢查 `CmdPalTranslator.Tests` 對應目標是否維持一致。此任務同時涵蓋相依套件調整，針對 assessment 指出的 2 個不相容 NuGet 套件完成替代或版本修正，確保還原與編譯路徑在 .NET 10 下可用。

同一任務中也會處理 assessment 指出的 source-incompatible API 變更（TimeSpan.FromSeconds 相關）與行為變更風險點（Uri/HttpContent），以「Fix Inline / Resolve Inline」方式一次完成，不建立延後子任務。

**Done when**: 所有目標專案的 TFM 已更新為 net10 對應值、套件還原成功、解決方案可完成編譯且無錯誤。

---

### 02-final-solution-validation: 完整驗證與收尾

針對整個 solution 執行最終驗證，包含完整 build、測試執行與警告清理，確認升級後功能面與品質閘門通過。此任務也會記錄延後事項（若有），包含 CPM 採用建議保留為 post-migration cleanup，而非此次升級內強制導入。

**Done when**: 全解決方案 build 成功、測試全數通過、修改過的專案無新增或遺留警告，且升級結果可進入提交階段。
