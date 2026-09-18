# CmdPalTranslator 網站

這是 `Translator for Command Palette` 的產品介紹網站，使用原生 HTML、CSS 與少量 JavaScript，沒有前端框架、套件管理或建置流程。網站以英文為預設語系，使用者可在頁首切換繁體中文（台灣）；產品名稱與實際介面文字保留英文。

## 本機預覽

請從儲存庫根目錄啟動 HTTP server，避免直接以 `file://` 開啟造成相對路徑行為與部署環境不同：

```powershell
py -m http.server 8080 --directory website
```

接著開啟 <http://localhost:8080>。也可以使用任何可提供靜態檔案的 HTTP server。

## 維護內容

- 產品功能、操作語法與需求應以根目錄 README、程式碼及隱私權文件為依據。
- 實際產品畫面放在 `assets/screenshots/`；若新增操作示意，必須在畫面與文案中明確標示為示意，不可冒充實際截圖。
- 網站沒有正式網址 metadata。部署到自訂網域或 GitHub Pages 後，才補上 `canonical` 與正式 `og:url`。
- 連結與素材使用相對路徑，確保網站部署在 GitHub Pages 的專案子路徑時仍可載入。
- 語系切換使用瀏覽器的 `localStorage` 保存偏好；停用 JavaScript 時仍會顯示完整英文內容。

## GitHub Pages 部署

1. 在儲存庫 **Settings → Pages** 將 **Source** 設為 **GitHub Actions**。
2. 開啟 **Actions**，選取 **Deploy website to GitHub Pages**，按 **Run workflow** 手動執行。
3. workflow 只會上傳 `website/`，不會將整個儲存庫部署為網站。
4. 部署完成後，從 workflow summary 的 Pages URL 開啟網站。正式網址確認後，再更新 metadata。

目前 workflow 僅接受 `workflow_dispatch`，不會因 push 自動部署。日後若要在預設分支變更時自動部署，可在 `.github/workflows/deploy-website.yml` 的 `on` 區塊加入：

```yaml
  push:
    branches: [main]
```

正式部署前也請確認 repository 的 Pages 與 Actions 權限政策允許 workflow 使用 `github-pages` environment。