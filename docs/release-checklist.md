# Release checklist / 發布檢查表

## Quality / 品質

- [ ] Run `dotnet format UniversalQuickPreview.sln --verify-no-changes`.
- [ ] Build Release with warnings treated as errors.
- [ ] Run all tests on Windows 10 and Windows 11.
- [ ] Test 100%, 125%, 150%, and 200% DPI, including mixed-DPI monitors.
- [ ] Verify Space does not trigger in Explorer address, search, and rename inputs.
- [ ] Verify outside click, Space, Esc, Left, and Right behavior.
- [ ] Test malformed, locked, very large, and deleted-during-preview files.

## Packages / 套件

- [ ] Run `build/Publish.ps1 -Version <version>`.
- [ ] Install and uninstall the generated Inno Setup package as a standard user.
- [ ] Verify the portable ZIP on a clean Windows user profile.
- [ ] Confirm uninstall removes shortcuts and the startup registry value.
- [ ] Scan release packages with Microsoft Defender and VirusTotal when appropriate.

## Signing / 簽署

- [ ] Obtain an Authenticode code-signing certificate from a trusted provider.
- [ ] Store the certificate and password only as protected CI secrets.
- [ ] Sign `UniversalQuickPreview.exe` before compiling the installer.
- [ ] Sign the final installer and timestamp both signatures.
- [ ] Verify signatures with `signtool verify /pa /v`.

Unsigned development builds are expected to display Windows SmartScreen warnings. The repository intentionally contains no certificate or signing secret.

## GitHub / GitHub 發布

- [x] Add the final GitHub repository URL to package metadata.
- [ ] Record user-visible changes in release notes.
- [ ] Capture the bilingual README demo GIF using `docs/demo-script.md`.
- [ ] Tag the reviewed commit as `vX.Y.Z`.
- [ ] Confirm GitHub Actions uploaded both installer and portable artifacts.
- [ ] Create the GitHub Release and attach the verified artifacts and checksums.
