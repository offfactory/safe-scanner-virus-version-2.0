import SwiftUI

struct ContentView: View {
    @StateObject private var scanner = ScannerEngine()
    @State private var selectedFolder: URL?
    @State private var showingPicker = false

    var body: some View {
        NavigationSplitView {
            VStack(alignment: .leading, spacing: 18) {
                Label("SafeScan Defender", systemImage: "shield.lefthalf.filled")
                    .font(.title2.bold())
                Text("Local-first macOS scanner")
                    .foregroundStyle(.secondary)
                Divider()
                Button("Choose folder", systemImage: "folder") { showingPicker = true }
                Button("Start local scan", systemImage: "magnifyingglass") {
                    guard let selectedFolder else { return }
                    Task { await scanner.scan(url: selectedFolder) }
                }
                .buttonStyle(.borderedProminent)
                .disabled(selectedFolder == nil || scanner.isScanning)
                Spacer()
                Label("No files are uploaded", systemImage: "lock.shield")
                    .font(.caption)
                    .foregroundStyle(.secondary)
            }
            .padding()
            .frame(minWidth: 230)
        } detail: {
            VStack(alignment: .leading, spacing: 16) {
                HStack {
                    VStack(alignment: .leading) {
                        Text("Scan results").font(.largeTitle.bold())
                        Text(selectedFolder?.path ?? "Choose a folder to begin")
                            .font(.caption).foregroundStyle(.secondary)
                    }
                    Spacer()
                    if scanner.isScanning {
                        ProgressView("Scanning \(scanner.scannedCount) files…")
                    } else {
                        Label("\(scanner.scannedCount) files", systemImage: "checkmark.circle")
                            .foregroundStyle(.green)
                    }
                }
                Divider()
                if scanner.items.isEmpty {
                    ContentUnavailableView("No results yet", systemImage: "doc.text.magnifyingglass",
                                           description: Text("SafeScan calculates local SHA-256 hashes and keeps results on this Mac."))
                } else {
                    List(scanner.items) { item in
                        VStack(alignment: .leading) {
                            Text(item.path).font(.callout).lineLimit(1)
                            Text(item.error ?? item.hash ?? "Unavailable")
                                .font(.caption.monospaced()).foregroundStyle(.secondary).lineLimit(1)
                        }
                    }
                }
            }
            .padding()
        }
        .fileImporter(isPresented: $showingPicker, allowedContentTypes: [.folder]) { result in
            if case .success(let url) = result { selectedFolder = url }
        }
    }
}
