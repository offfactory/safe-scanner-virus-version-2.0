import SwiftUI

@main
struct PythonCodeEditorApp: App {
    var body: some Scene {
        WindowGroup("XCode Python Decode Editor") { EditorView() }
    }
}

struct EditorView: View {
    @State private var source = "# Open a Python file to begin\nprint(\"Hello from OffFactory\")\n"
    @State private var fileURL: URL?
    @State private var output = "Run output will appear here."
    @State private var showingImporter = false

    var body: some View {
        VStack(spacing: 0) {
            HStack {
                Text("XCode Python Decode Editor").font(.headline)
                Spacer()
                Button("Open", systemImage: "folder") { showingImporter = true }
                Button("Save", systemImage: "square.and.arrow.down") { save() }
                Button("Run Python", systemImage: "play.fill") { run() }
                    .buttonStyle(.borderedProminent)
            }.padding()
            TextEditor(text: $source)
                .font(.system(.body, design: .monospaced))
                .padding(8)
            Divider()
            ScrollView { Text(output).frame(maxWidth: .infinity, alignment: .leading).padding() }
                .frame(minHeight: 130, maxHeight: 180)
                .background(Color.black.opacity(0.12))
        }
        .frame(minWidth: 760, minHeight: 560)
        .fileImporter(isPresented: $showingImporter, allowedContentTypes: [.sourceCode, .plainText]) { result in
            guard case .success(let url) = result else { return }
            fileURL = url
            source = (try? String(contentsOf: url, encoding: .utf8)) ?? ""
        }
    }

    private func save() {
        guard let fileURL else { return }
        do { try source.write(to: fileURL, atomically: true, encoding: .utf8); output = "Saved \(fileURL.lastPathComponent)." }
        catch { output = "Save failed: \(error.localizedDescription)" }
    }

    private func run() {
        save()
        guard let fileURL, fileURL.pathExtension == "py" else {
            output = "Save a .py file before running it."
            return
        }
        let process = Process()
        let pipe = Pipe()
        process.executableURL = URL(fileURLWithPath: "/usr/bin/python3")
        process.arguments = [fileURL.path]
        process.standardOutput = pipe
        process.standardError = pipe
        do {
            try process.run()
            process.waitUntilExit()
            output = String(data: pipe.fileHandleForReading.readDataToEndOfFile(), encoding: .utf8) ?? "No output."
        } catch { output = "Run failed: \(error.localizedDescription)" }
    }
}
