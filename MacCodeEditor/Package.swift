// swift-tools-version: 5.9
import PackageDescription

let package = Package(
    name: "OffFactoryPythonCodeEditor",
    platforms: [.macOS(.v13)],
    products: [.executable(name: "OffFactoryPythonCodeEditor", targets: ["OffFactoryPythonCodeEditor"])],
    targets: [.executableTarget(name: "OffFactoryPythonCodeEditor")]
)
