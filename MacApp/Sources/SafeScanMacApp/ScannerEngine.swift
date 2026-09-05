import Foundation
import CryptoKit

struct ScanItem: Identifiable {
    let id = UUID()
    let path: String
    let hash: String?
    let error: String?
}

@MainActor
final class ScannerEngine: ObservableObject {
    @Published private(set) var items: [ScanItem] = []
    @Published private(set) var isScanning = false
    @Published private(set) var scannedCount = 0

    func scan(url: URL) async {
        guard !isScanning else { return }
        isScanning = true
        items = []
        scannedCount = 0
        guard let files = FileManager.default.enumerator(
            at: url,
            includingPropertiesForKeys: [.isRegularFileKey, .isReadableKey],
            options: [.skipsHiddenFiles, .skipsPackageDescendants]
        ) else {
            isScanning = false
            return
        }
        for case let file as URL in files {
            guard let values = try? file.resourceValues(forKeys: [.isRegularFileKey, .isReadableKey]),
                  values.isRegularFile == true, values.isReadable != false else { continue }
            let result = hash(file)
            items.insert(ScanItem(path: file.path, hash: result.hash, error: result.error), at: 0)
            scannedCount += 1
            await Task.yield()
        }
        isScanning = false
    }

    private func hash(_ url: URL) -> (hash: String?, error: String?) {
        do {
            let data = try Data(contentsOf: url, options: [.mappedIfSafe])
            return (SHA256.hash(data: data).map { String(format: "%02x", $0) }.joined(), nil)
        } catch {
            return (nil, error.localizedDescription)
        }
    }
}
