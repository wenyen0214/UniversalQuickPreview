# Explorer integration decision / 檔案總管整合決策

## Decision / 決策

The application uses three independent Windows integration points:

1. A `WH_KEYBOARD_LL` low-level keyboard hook detects Space, Alt+Space, Ctrl+Space, Escape, Left, and Right.
2. `Shell.Application.Windows()` finds the foreground Explorer window and reads `Document.SelectedItems()`.
3. A `WH_MOUSE_LL` hook watches only while the preview is visible and closes it when a mouse button is pressed outside the visible preview border.

應用程式採用三個彼此獨立的 Windows 整合點：

1. 以 `WH_KEYBOARD_LL` 低階鍵盤 Hook 偵測 Space、Alt+Space、Ctrl+Space、Escape、Left 與 Right。
2. 以 `Shell.Application.Windows()` 尋找前景檔案總管視窗，再由 `Document.SelectedItems()` 讀取選取項目。
3. 只有在預覽視窗可見時才啟用 `WH_MOUSE_LL` Hook；滑鼠按鍵若落在可見預覽邊框外，就關閉預覽，但不攔截原本的點擊。

The keyboard path checks the foreground window class and focused UI Automation control before raising an app event. Shell COM and preview loading work is then queued outside the hook callback. Each new selection cancels the previous preview load. This keeps the hooks short and prevents Explorer COM or file I/O latency from delaying input.

鍵盤路徑會先檢查前景視窗類別與目前具有焦點的 UI Automation 控制項，再引發應用程式事件。Shell COM 查詢與預覽載入會排在 Hook callback 之外執行；切換檔案時也會取消上一個載入工作，避免 COM 或檔案 I/O 延遲阻塞輸入。

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

The application accepts regular filesystem files only. Left and Right navigate through filesystem items returned by the current folder's Shell collection; this is best-effort folder order and may differ from a custom Explorer sort or grouping. Desktop selection, virtual Shell items, multiple-file preview, exact `IFolderView2` view-order navigation, and elevated Explorer windows remain outside the current scope.

目前只接受一般檔案系統中的檔案。左右鍵會依目前資料夾 Shell 集合傳回的檔案順序導覽；若檔案總管使用自訂排序或群組，順序可能不同。桌面選取、虛擬 Shell 項目、多檔預覽、精確的 `IFolderView2` 檢視順序，以及高權限檔案總管視窗仍不在目前範圍內。
