# WindowTopmostToggler (WPF, no WinForms)

A minimal WPF app that lists all visible top-level windows and lets you toggle the **Always on Top** state for each window. Uses pure Win32 P/Invoke; **no WinForms** references.

## Build & Run (CLI only)

1. Install [.NET 8 SDK for Windows].
2. Extract this zip.
3. Open a Developer Command Prompt (or PowerShell) in the extracted folder.
4. Build:
   ```
   dotnet build -c Release
   ```
5. Run:
   ```
   dotnet run --project WindowTopmostToggler -c Release
   ```

## UI change: clearly visible toggle
- The **Toggle** column uses a bold, colored button with clear labels:
  - **Topmost ✓** (green) when active
  - **Set Topmost** (neutral) when inactive
- A separate **State** badge shows **TOPMOST** vs **Normal** for extra clarity.

## Notes

- The app calls `SetWindowPos(HWND_TOPMOST / HWND_NOTOPMOST)` to toggle topmost without moving or resizing the target window.
- Topmost status is detected by checking `GWL_EXSTYLE` for `WS_EX_TOPMOST`.
- Only windows that are **visible** and have a non-empty title are shown.
- You can click **Refresh** any time to re-enumerate windows.

## Structure

```
WindowTopmostToggler.sln
WindowTopmostToggler/
  App.xaml
  App.xaml.cs
  MainWindow.xaml
  MainWindow.xaml.cs
  Models/WindowEntry.cs
  Services/Win32.cs
  ViewModels/MainViewModel.cs
  WindowTopmostToggler.csproj
```