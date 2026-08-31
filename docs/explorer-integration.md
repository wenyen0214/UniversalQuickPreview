# Explorer integration decision / 檔案總管整合決策

## Decision / 決策

The first vertical slice uses two independent Windows integration points:

1. A `WH_KEYBOARD_LL` low-level keyboard hook detects an unmodified Space key.
2. `Shell.Application.Windows()` finds the foreground Explorer window and reads `Document.SelectedItems()`.

第一個垂直切片採用兩個彼此獨立的 Windows 整合點：

1. 以 `WH_KEYBOARD_LL` 低階鍵盤 Hook 偵測未搭配修飾鍵的 Space。
2. 以 `Shell.Application.Windows()` 尋找前景檔案總管視窗，再由 `Document.SelectedItems()` 讀取選取項目。

The keyboard path checks the foreground window class and focused UI Automation control before raising an app event. Shell COM work is then queued outside the hook callback. This keeps the hook short and prevents Explorer COM latency from delaying keyboard input.

鍵盤路徑會先檢查前景視窗類別與目前具有焦點的 UI Automation 控制項，再引發應用程式事件。Shell COM 查詢會排在 Hook callback 之外執行，避免 COM 延遲阻塞鍵盤輸入。

## Alternatives considered / 評估過的替代方案

### `RegisterHotKey`

This is the preferred API for combinations such as Ctrl+Space or Alt+Space, but it cannot reliably register a bare Space key as a system-wide hotkey. The shortcut abstraction intentionally leaves room for this implementation when configurable modified shortcuts are added.

這是 Ctrl+Space 或 Alt+Space 等組合鍵的首選 API，但無法可靠地把單獨 Space 註冊為系統全域快速鍵。現有快速鍵抽象仍保留未來加入可設定組合鍵時改用此方案的空間。

### Native Shell interfaces

Interfaces such as `IShellWindows`, `IWebBrowserApp`, and `IFolderView2` can provide stricter typing and deeper view-order information. They require substantially more COM interop definitions. The Shell Automation API offers the same selected-path capability with much less first-milestone code. A native `IFolderView2` implementation remains a candidate for exact navigation order in Milestone 3.

`IShellWindows`、`IWebBrowserApp` 與 `IFolderView2` 等原生介面能提供更嚴格的型別與更完整的檢視排序資訊，但需要大量 COM interop 定義。Shell Automation API 能以更少的第一階段程式碼提供相同的選取路徑能力。里程碑 3 若要精確跟隨檔案總管排序，可再考慮 `IFolderView2`。

### UI Automation selection lookup

UI Automation is retained only for identifying focused edit controls. Using it to scrape file-list names and reconstruct paths is more fragile across Explorer versions, localized shells, virtual folders, and Windows 11 UI changes.

目前只使用 UI Automation 判斷文字輸入焦點。若以它擷取檔案清單名稱並重建路徑，會更容易受檔案總管版本、系統語言、虛擬資料夾與 Windows 11 UI 變更影響，因此不作為主要選取查詢方案。

## Known boundary / 已知邊界

This milestone accepts regular filesystem files only. Desktop selection, virtual Shell items, multiple-file preview, Explorer view-order navigation, and elevated Explorer windows are outside the current slice.

本里程碑只接受一般檔案系統中的檔案。桌面選取、虛擬 Shell 項目、多檔預覽、依檔案總管排序導覽，以及高權限檔案總管視窗都不在目前範圍內。
