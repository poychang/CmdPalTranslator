# 新增介面語系

本應用程式的介面文字存放於 [`src/CmdPalTranslator/Localization`](../src/CmdPalTranslator/Localization) 資料夾。每個 JSON 檔案代表一個介面語系，檔名（不含副檔名）就是應用程式使用的語系 ID。

例如，繁體中文使用 `zh-TW` 作為語系 ID，因此語系檔案為：

```text
src/CmdPalTranslator/Localization/zh-TW.json
```

## 新增語系

1. 複製 [`en-US.json`](../src/CmdPalTranslator/Localization/en-US.json) 作為範本。
2. 使用新語系的 ID 命名檔案，建議採用 BCP 47 語言標籤，例如：
   - 繁體中文：`zh-TW.json`
   - 日文：`ja-JP.json`
   - 法文：`fr-FR.json`
3. 將 `_meta.displayName` 設為該語言的原生名稱。這個名稱會顯示在應用程式的語言選單中。
4. 保留範本中的所有翻譯鍵，並翻譯每個鍵的值。
5. 建置並啟動應用程式，從設定中的「顯示語言」選取新增的語系，確認所有介面文字皆能正常顯示。

## 繁體中文範例

`zh-TW.json` 的基本結構如下：

```json
{
  "_meta.displayName": "繁體中文",
  "Settings.PreferredProvider.Label": "偏好翻譯服務",
  "Settings.PreferredProvider.Description": "選擇預設的翻譯服務提供者。"
}
```

實際檔案必須包含 [`en-US.json`](../src/CmdPalTranslator/Localization/en-US.json) 中的所有翻譯鍵，而不只是上述範例中的鍵。

## 翻譯注意事項

- 不要翻譯 JSON 的鍵，只翻譯值。
- `_meta.displayName` 是必要的語系顯示名稱；載入介面文字時不會將 `_meta.*` 視為一般翻譯鍵。
- 必須保留 `{0}`、`{1}` 等格式化預留位置，且不可改變其用途。例如：

  ```json
  "Page.Main.Item.Details.LanguagePair.Text": "{0} 翻譯為 {1}"
  ```

- JSON 字串中的換行、反斜線及雙引號必須正確逸出。
- 建議讓新語系檔與 `en-US.json` 使用相同的鍵集合及排列順序，以利日後比對與維護。

## 語系如何被載入

應用程式啟動時會自動掃描輸出目錄下 `Localization` 資料夾中的所有 `*.json` 檔案，並以檔名作為語系 ID。專案檔已設定將 `Localization/*.json` 複製到輸出目錄，因此新增語系檔後，不需要另外在程式碼或專案檔中註冊。

若選取的語系檔不存在，應用程式會回退至預設語系 `en-US`。若翻譯鍵不存在，介面上會直接顯示該鍵名，方便辨識遺漏的翻譯。

> [!NOTE]
> `Localization` 語系檔控制的是應用程式介面語言。若要新增翻譯服務支援的來源或目標語言，則需要另外調整 `src/CmdPalTranslator.Core/Models/LanguageCatalog.cs`；兩者是不同的功能。

## 驗證清單

- 語系檔位於 `src/CmdPalTranslator/Localization`。
- 檔名使用正確的語系 ID，例如 `zh-TW.json`。
- JSON 格式有效，且所有值皆為字串。
- `_meta.displayName` 已設定為該語言的原生名稱。
- 翻譯鍵與 `en-US.json` 一致。
- `{0}`、`{1}` 等格式化預留位置皆已保留。
- 專案能成功建置。
- 新語系會出現在「顯示語言」選單中，且選取後介面顯示正確。
