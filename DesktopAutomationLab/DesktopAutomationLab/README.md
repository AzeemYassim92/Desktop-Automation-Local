# DesktopAutomationLab

A small .NET 8 WPF desktop application intended for safe automation learning:

- global hotkey registration
- cursor capture
- region definition
- live screen color sampling
- region screenshot capture to PNG
- drag-select region overlay with auto-save
- sample history export to file
- JSON settings persistence
- structured logs

## Open and run

1. Open `DesktopAutomationLab.sln` in **Visual Studio 2022**.
2. Make sure the **Desktop development with .NET** workload is installed.
3. Restore packages if prompted.
4. Build and run.

## Notes

- Target framework: `net8.0-windows`
- UI framework: WPF
- This project is intended to be built on Windows.
- I could not run a local `dotnet build` in this environment because the .NET SDK is not installed here.
- Settings loading now safely falls back to defaults if JSON is invalid or unreadable.
- Settings values are normalized on load (minimum sample interval and valid region size).

## Foundation checklist before feature work

Use this quick pass before adding overlays, timers, or window targeting:

1. Confirm startup works (window appears, no first-run exceptions).
2. Register and trigger hotkey (see running state and log line update).
3. Capture cursor and define a region start/end.
4. Start sampling and verify color tile + recent samples update.
5. Save settings, restart app, then load settings and verify values persist.
6. Corrupt `appsettings.json` manually once; relaunch and verify app still starts with defaults.

## Project layout

- `MainWindow.xaml` - shell UI
- `ViewModels/MainViewModel.cs` - app state and commands
- `Services/HotkeyService.cs` - global hotkey registration
- `Services/ColorSamplerService.cs` - screen color sampling
- `Services/SettingsService.cs` - JSON save/load
- `Models/*` - settings and sample models
- `Interop/NativeMethods.cs` - minimal Win32 interop

## Next ideas

- live overlay window for region bounds
- named profiles
- process/window picker
- input test sandbox targeting a user-owned test window
- export logs / sample history
