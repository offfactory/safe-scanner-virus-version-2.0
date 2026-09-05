# SafeScan Defender macOS scanner

This is the first macOS-compatible SafeScan package. It is a local command-line scanner that recursively calculates SHA-256 hashes and safely skips inaccessible files.

## Run

```bash
dotnet SafeScanMac.dll "$HOME/Downloads"
```

The package does not upload file contents. It is not a signed `.app` bundle yet; the full cross-platform GUI is planned separately.
