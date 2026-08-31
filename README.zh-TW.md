# Universal Quick Preview

**按下 Space，快速預覽檔案。**

這是一套適用於 Windows、風格類似 macOS Quick Look 的快速檔案預覽工具。在檔案總管選取檔案後按下 <kbd>Space</kbd>，即可在不開啟其他應用程式的情況下查看檔案資訊。

[English README](README.md)

> 目前狀態：第一個可運作的垂直切片。現階段會顯示所選檔案的名稱、完整路徑、大小與修改時間；檔案內容預覽 Provider 將留待後續里程碑實作。

## 現有功能

- 適用於 Windows 10／11 的 WPF 系統匣背景應用程式
- 只在檔案總管作用中時偵測 <kbd>Space</kbd>，並排除文字輸入控制項
- 透過 Windows Shell COM API 取得檔案總管選取項目
- 在檔案總管所在螢幕顯示不主動搶焦點的輕量預覽視窗
- 按 <kbd>Space</kbd> 或 <kbd>Esc</kbd> 關閉
- 繁體中文與英文介面，可由系統匣即時切換
- 語言偏好只儲存在本機；沒有帳號、雲端服務、AI 或遙測

## 建置與執行

需求：Windows 10／11，以及 .NET 8 SDK 或更新版本。

```powershell
dotnet build UniversalQuickPreview.sln
dotnet test UniversalQuickPreview.sln
dotnet run --project src/UniversalQuickPreview.App
```

啟動後，應用程式會留在通知區域。開啟檔案總管、選取檔案，再按下 <kbd>Space</kbd>。系統匣選單可用來啟用／停用快速預覽、切換語言、查看關於資訊或結束程式。

## 解決方案結構

- `UniversalQuickPreview.Core`：與平台無關的模型、介面、檔案中繼資料與格式化
- `UniversalQuickPreview.Windows`：鍵盤 Hook、Explorer Shell 整合與螢幕定位
- `UniversalQuickPreview.App`：WPF 預覽視窗、系統匣主程式與本地化
- `UniversalQuickPreview.Core.Tests`：平台無關行為的聚焦測試

Explorer 整合方案與替代方案比較請見 [docs/explorer-integration.md](docs/explorer-integration.md)。

## 本次範圍

這個版本只驗證第一條端到端路徑：檔案總管選取 → Space → WPF 檔案資訊預覽。文字、圖片、Markdown、CSV、ZIP、PDF、左右導覽、開機啟動、安裝程式與第三方 Provider 尚未實作。

## 授權

MIT
