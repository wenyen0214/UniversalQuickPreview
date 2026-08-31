# Universal Quick Preview

**Press Space. Preview anything.**

A fast, lightweight Quick Look-style file previewer for Windows. Select a file in File Explorer and press <kbd>Space</kbd> to inspect it without opening another application.

[繁體中文說明](README.zh-TW.md)

> Current status: the first functional vertical slice. It displays the selected file's name, full path, size, and modified time. Content preview providers are intentionally deferred to the next milestones.

## Current features

- Background WPF tray application for Windows 10 and Windows 11
- Explorer-only <kbd>Space</kbd> detection with focused text inputs excluded
- Selection lookup through the Windows Shell COM API
- Lightweight, non-activating preview window on Explorer's monitor
- <kbd>Space</kbd> or <kbd>Esc</kbd> to close
- English and Traditional Chinese UI, switchable from the tray menu
- Local-only preferences; no account, cloud service, AI, or telemetry

## Build and run

Requirements: Windows 10/11 and the .NET 8 SDK or newer.

```powershell
dotnet build UniversalQuickPreview.sln
dotnet test UniversalQuickPreview.sln
dotnet run --project src/UniversalQuickPreview.App
```

After launch, the app remains in the notification area. Open File Explorer, select a file, and press <kbd>Space</kbd>. Use the tray menu to enable or disable previewing, change language, view About, or exit.

## Solution structure

- `UniversalQuickPreview.Core`: platform-neutral models, contracts, metadata, and formatting
- `UniversalQuickPreview.Windows`: keyboard hook, Explorer Shell integration, and monitor placement
- `UniversalQuickPreview.App`: WPF preview window, tray host, and localization
- `UniversalQuickPreview.Core.Tests`: focused tests for platform-neutral behavior

The Explorer integration decision and considered alternatives are documented in [docs/explorer-integration.md](docs/explorer-integration.md).

## Scope

This commit proves the first end-to-end interaction only: Explorer selection → Space → WPF metadata preview. Text, image, Markdown, CSV, ZIP, PDF, navigation, startup registration, installer, and third-party providers are not implemented yet.

## License

MIT
