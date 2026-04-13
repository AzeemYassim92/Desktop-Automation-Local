# DesktopAutomationLab

A small .NET 8 WPF desktop application intended for safe automation learning:

- global hotkey registration
- cursor capture
- region definition
- live screen color sampling
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
