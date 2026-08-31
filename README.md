# Universal Quick Preview

![Universal Quick Preview icon](src/UniversalQuickPreview.App/Assets/AppIcon.png)

**Press Space. Preview instantly.**

A fast, local-only Quick Look-style file previewer for Windows 10 and Windows 11. Select a file in File Explorer and press <kbd>Space</kbd> to inspect it without opening another application.

[繁體中文說明](README.zh-TW.md)

## Features

- Preview text, logs, source code, JSON, Markdown, CSV, and common image formats
- Large-text protection: reads at most the first 256K characters
- Image downsampling for responsive, memory-conscious display
- <kbd>Left</kbd> / <kbd>Right</kbd> navigation with cancellation of stale loads
- Close with the configured shortcut, <kbd>Esc</kbd>, the close button, or any click outside the visible preview
- Explorer address, search, and rename inputs are excluded from shortcut handling
- Configurable Space, Alt+Space, or Ctrl+Space shortcut
- System, light, and dark themes; compact, medium, and large windows
- English and Traditional Chinese UI with runtime switching
- Single-instance behavior, optional startup registration, and rotating local logs
- No account, cloud dependency, AI runtime, telemetry, or file modification

## Supported preview providers

| Provider | Formats |
| --- | --- |
| Text | TXT, LOG, INI, CFG, CONF, C/C++, C#, Python, JS/TS, HTML, CSS, XML, YAML, JSON, Markdown, CSV/TSV, SQL, PowerShell, shell scripts |
| Image | JPG/JPEG, PNG, WebP, BMP, GIF (first frame) |
| Fallback | Metadata for every other regular file |

The provider contract is independent from WPF rendering so future PDF, archive, Office, and plugin providers can be added without coupling them to the preview window.

## Build and test

Requirements: Windows 10/11 and the .NET 8 SDK or newer.

```powershell
dotnet restore UniversalQuickPreview.sln
dotnet build UniversalQuickPreview.sln --configuration Release
dotnet test UniversalQuickPreview.sln --configuration Release
dotnet run --project src/UniversalQuickPreview.App
```

## Packages

Create a self-contained portable ZIP:

```powershell
./build/Publish.ps1 -Version 0.1.0 -SkipInstaller
```

Install [Inno Setup 7](https://jrsoftware.org/isdl.php) and omit `-SkipInstaller` to also create the per-user installer. GitHub Actions runs the full build and test suite on Windows and uploads both packages for every push, pull request, tag, and manual run.

Authenticode signing is intentionally not automated until a trusted certificate is configured. See [the release checklist](docs/release-checklist.md) for the signing strategy and verification steps.

## Project structure

- `UniversalQuickPreview.Core`: models, contracts, provider registry, navigation, and metadata
- `UniversalQuickPreview.Windows`: Explorer Shell integration, keyboard/mouse hooks, startup, and monitor placement
- `UniversalQuickPreview.Previewers`: independent text, image, and fallback providers
- `UniversalQuickPreview.App`: WPF UI, tray host, settings, themes, logging, and localization
- `tests`: focused Core and Previewer test suites

See [Explorer integration decisions](docs/explorer-integration.md), the [demo capture script](docs/demo-script.md), and the [release checklist](docs/release-checklist.md).

## License

MIT
