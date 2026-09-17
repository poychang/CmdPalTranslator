# Adding a Display Language

The application's interface strings are stored in the [`src/CmdPalTranslator/Localization`](../src/CmdPalTranslator/Localization) directory. Each JSON file represents one display language, and its filename without the extension is the language ID used by the application.

For example, Traditional Chinese uses `zh-TW` as its language ID, so its localization file is:

```text
src/CmdPalTranslator/Localization/zh-TW.json
```

## Adding a Language

1. Copy [`en-US.json`](../src/CmdPalTranslator/Localization/en-US.json) as a template.
2. Name the file using the new language ID. A BCP 47 language tag is recommended, for example:
   - Traditional Chinese: `zh-TW.json`
   - Japanese: `ja-JP.json`
   - French: `fr-FR.json`
3. Set `_meta.displayName` to the language's native name. This name appears in the application's language selector.
4. Keep every localization key from the template and translate each value.
5. Build and launch the application, select the new language under **Display language** in Settings, and verify that all interface strings appear correctly.

## Traditional Chinese Example

The basic structure of `zh-TW.json` is:

```json
{
  "_meta.displayName": "繁體中文",
  "Settings.PreferredProvider.Label": "偏好翻譯服務",
  "Settings.PreferredProvider.Description": "選擇預設的翻譯服務提供者。"
}
```

The actual file must contain every localization key from [`en-US.json`](../src/CmdPalTranslator/Localization/en-US.json), not only the keys shown in this example.

## Translation Guidelines

- Do not translate the JSON keys. Translate only their values.
- `_meta.displayName` is the required display name for the language. Keys starting with `_meta.*` are not loaded as regular interface strings.
- Preserve formatting placeholders such as `{0}` and `{1}` without changing their purpose. For example:

  ```json
  "Page.Main.Item.Details.LanguagePair.Text": "{0} 翻譯為 {1}"
  ```

- Escape line breaks, backslashes, and double quotes correctly in JSON strings.
- Keep the same key set and ordering as `en-US.json` to make future comparisons and maintenance easier.

## How Languages Are Loaded

At startup, the application automatically scans all `*.json` files in the output directory's `Localization` folder and uses each filename as its language ID. The project is already configured to copy `Localization/*.json` files to the output directory, so no additional registration in the code or project file is required.

If the selected localization file does not exist, the application falls back to the default language, `en-US`. If a localization key is missing, the interface displays the key itself to make missing translations easy to identify.

> [!NOTE]
> Files under `Localization` control the application's display language. To add a source or target language supported by the translation providers, update `src/CmdPalTranslator.Core/Models/LanguageCatalog.cs` separately. These are different features.

## Validation Checklist

- The localization file is located in `src/CmdPalTranslator/Localization`.
- The filename uses the correct language ID, such as `zh-TW.json`.
- The JSON is valid, and every value is a string.
- `_meta.displayName` is set to the language's native name.
- The localization keys match those in `en-US.json`.
- Formatting placeholders such as `{0}` and `{1}` are preserved.
- The project builds successfully.
- The new language appears in the **Display language** selector and displays correctly when selected.
