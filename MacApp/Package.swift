// swift-tools-version: 5.9
import PackageDescription

let package = Package(
    name: "SafeScanMacApp",
    platforms: [.macOS(.v13)],
    products: [
        .executable(name: "SafeScanMacApp", targets: ["SafeScanMacApp"])
    ],
    targets: [
        .executableTarget(name: "SafeScanMacApp")
    ]
)
