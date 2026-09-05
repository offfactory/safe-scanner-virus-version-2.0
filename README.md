# SafeScan Defender

SafeScan Defender is a Windows antivirus scanner built with .NET 8 and WinForms.

SafeScan Defender 2.0 is a Windows security scanner with an offline-first modular engine.

## Features
- Local SHA-256 hash scanning and offline signature matching
- Conservative heuristic detection with explanations
- Desktop, Documents, Downloads, fixed-drive, and custom scanning
- Dashboard with quick scan, full scan, custom scan, and Roblox Security Check
- Quarantine metadata, restore support, and local scan history
- Six built-in themes and persistent settings
- No telemetry or cloud scanning required for core functionality

## Installation
1. Download the latest ZIP from the [releases page](https://github.com/behade/behade.github.io/releases).
2. Extract it to a folder on Windows.
3. Run `SafeScan.exe`.

To build from source:
```powershell
dotnet build Desktop.sln -c Release
```

## Usage
- Use **Dashboard** for quick actions and protection status.
- Use **Scan** to watch local scan progress.
- Use **Files** to review results and locations.
- Use **Settings** to configure themes, offline mode, and scan behavior.

## Links
- GitHub: https://github.com/behade/behade.github.io
- Website: https://behade.github.io
