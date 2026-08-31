# Universal Quick Preview

![Universal Quick Preview 圖示](src/UniversalQuickPreview.App/Assets/AppIcon.png)

**按下 Space，立即預覽。**

適用於 Windows 10／11、風格類似 macOS Quick Look 的本機快速預覽工具。在檔案總管選取檔案後按下 <kbd>Space</kbd>，即可在不開啟其他應用程式的情況下查看內容。

[English README](README.md)

## 功能

- 預覽文字、記錄檔、程式碼、JSON、Markdown、CSV 與常見圖片
- 大型文字保護：最多讀取前 256K 個字元
- 圖片降採樣，兼顧回應速度與記憶體使用量
- 使用 <kbd>←</kbd>／<kbd>→</kbd> 導覽，切換時取消過期載入工作
- 再按快速鍵、<kbd>Esc</kbd>、關閉按鈕或點擊可見預覽外部即可關閉
- 在網址列、搜尋框及重新命名欄位中不會攔截快速鍵
- 可設定 Space、Alt+Space 或 Ctrl+Space
- 跟隨系統／淺色／深色主題，以及精簡／中等／大型視窗
- 繁體中文與英文介面，可在執行期間切換
- 單一執行個體、選用開機啟動與可輪替本機日誌
- 不需要帳號、雲端、執行期 AI 或遙測，也不會修改預覽檔案

## 已支援的 Preview Provider

| Provider | 格式 |
| --- | --- |
| 文字 | TXT、LOG、INI、CFG、CONF、C/C++、C#、Python、JS/TS、HTML、CSS、XML、YAML、JSON、Markdown、CSV/TSV、SQL、PowerShell、Shell Script |
| 圖片 | JPG/JPEG、PNG、WebP、BMP、GIF（第一格） |
| Fallback | 其他一般檔案的中繼資料 |

Provider 合約與 WPF 顯示層分離，因此未來可加入 PDF、壓縮檔、Office 或第三方外掛，而不必把解析邏輯耦合到預覽視窗。

## 建置與測試

需求：Windows 10／11，以及 .NET 8 SDK 或更新版本。

```powershell
dotnet restore UniversalQuickPreview.sln
dotnet build UniversalQuickPreview.sln --configuration Release
dotnet test UniversalQuickPreview.sln --configuration Release
dotnet run --project src/UniversalQuickPreview.App
```

## 發布套件

建立不需另外安裝 .NET 的可攜式 ZIP：

```powershell
./build/Publish.ps1 -Version 0.1.0 -SkipInstaller
```

安裝 [Inno Setup 7](https://jrsoftware.org/isdl.php) 並移除 `-SkipInstaller`，即可同時產生每位使用者安裝程式。GitHub Actions 會在每次 Push、Pull Request、Tag 與手動執行時完成 Windows 建置、測試，並上傳安裝版與可攜版。

在設定可信任的程式碼簽署憑證之前，不會假裝自動簽署。簽署策略與驗證流程請見[發布檢查表](docs/release-checklist.md)。

## 專案結構

- `UniversalQuickPreview.Core`：模型、合約、Provider Registry、導覽與檔案中繼資料
- `UniversalQuickPreview.Windows`：Explorer Shell 整合、鍵盤／滑鼠 Hook、開機啟動與螢幕定位
- `UniversalQuickPreview.Previewers`：獨立文字、圖片及 Fallback Provider
- `UniversalQuickPreview.App`：WPF UI、系統匣、設定、主題、日誌與本地化
- `tests`：Core 與 Previewer 聚焦測試

另請參考 [Explorer 整合決策](docs/explorer-integration.md)、[Demo 錄製腳本](docs/demo-script.md)及[發布檢查表](docs/release-checklist.md)。

## 授權

MIT
