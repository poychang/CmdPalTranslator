# Privacy Policy — CmdPalTranslator

**Last Updated:** 2026-04-02

## Overview

CmdPalTranslator is a Windows Command Palette extension that provides text translation using Bing and Google translation services. This Privacy Policy describes what information the application accesses, how it is used, and your rights as a user.

---

## 1. Information We Collect

### Information You Provide

When you use CmdPalTranslator to translate text, the following data is temporarily processed:

- **Source text** — the text you type into the Command Palette for translation.
- **Language selection** — the source language (auto-detected or specified) and target language code.

This data is transmitted directly to the selected third-party translation service (Bing or Google) solely to perform the translation. **No text input or translation results are stored by this application or by the developer.**

### Locally Stored Preferences

The application stores a single preference on your device:

- **Default target language** — saved to `%LocalAppData%\CmdPalTranslator\default-target-language.txt`.

This file contains only a language code (e.g., `zht` for Traditional Chinese). It remains on your device and is never transmitted to any server.

---

## 2. Information We Do NOT Collect

The developer of CmdPalTranslator does **not** collect, store, or transmit:

- Personal identification information (name, email, account credentials)
- Device identifiers or hardware fingerprints
- Usage history, search history, or translation logs
- Crash reports or diagnostic telemetry
- Location data

---

## 3. Third-Party Services

To perform translations, CmdPalTranslator connects to the following external services on your behalf:

| Service | Endpoint | Privacy Policy |
|---------|----------|----------------|
| **Microsoft Bing Translator** | `https://www.bing.com/translator` | [Microsoft Privacy Statement](https://privacy.microsoft.com/privacystatement) |
| **Google Translate** | `https://translate.googleapis.com` | [Google Privacy Policy](https://policies.google.com/privacy) |

When you initiate a translation, your input text and selected language codes are sent directly to the chosen provider. The developer has no control over how these third parties process the data. Please review their respective privacy policies for details.

---

## 4. Internet Connectivity

CmdPalTranslator requires internet access (`internetClient` capability) exclusively to call the Bing and Google translation APIs. No other outbound network connections are made by this application.

---

## 5. Data Retention

- **Translation input** is not retained by the application. Once the translation result is returned and displayed, no copy of the input or output is kept.
- **Language preference** is stored locally until you change it or uninstall the application.

---

## 6. Children's Privacy

CmdPalTranslator does not knowingly collect any personal information from children under the age of 13. If you believe a child has provided personal data through a translation request, please contact the relevant third-party service directly (Bing or Google) as the data is processed solely by them.

---

## 7. Changes to This Policy

This Privacy Policy may be updated to reflect changes in application functionality or applicable law. Updates will be published in this document with a revised "Last Updated" date. Continued use of the application after an update constitutes acceptance of the revised policy.

---

## 8. Contact

If you have questions or concerns about this Privacy Policy, please open an issue on the project repository:

**GitHub:** [https://github.com/poychang/CmdPalTranslator](https://github.com/poychang/CmdPalTranslator)

---

## 隱私權政策（繁體中文）

**最後更新：** 2026-04-02

CmdPalTranslator 是一個 Windows Command Palette 翻譯擴充功能。本應用程式透過 Bing 與 Google 翻譯服務執行文字翻譯。

### 資料收集與使用

- 您輸入的**文字內容**與**語言設定**會直接傳送至所選的第三方翻譯服務（Bing 或 Google），僅用於執行翻譯。開發者不保留任何翻譯紀錄。
- 本應用程式僅在您的裝置上本機儲存一項設定：**預設目標語言**（`%LocalAppData%\CmdPalTranslator\default-target-language.txt`）。此設定不會傳送至任何伺服器。
- 本應用程式**不收集**任何個人識別資訊、裝置識別碼、使用記錄、診斷遙測或位置資料。

### 第三方服務

翻譯請求由 Bing（Microsoft）或 Google 處理。請參閱其各自的隱私權政策：

- [Microsoft 隱私權聲明](https://privacy.microsoft.com/privacystatement)
- [Google 隱私權政策](https://policies.google.com/privacy)

### 聯絡方式

如有任何疑問，請至 GitHub 開立 Issue：[https://github.com/poychang/CmdPalTranslator](https://github.com/poychang/CmdPalTranslator)
