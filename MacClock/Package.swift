// swift-tools-version: 5.9
import PackageDescription

let package = Package(
    name: "OffFactoryPrecisionClock",
    platforms: [.macOS(.v13)],
    products: [.executable(name: "OffFactoryPrecisionClock", targets: ["OffFactoryPrecisionClock"])],
    targets: [.executableTarget(name: "OffFactoryPrecisionClock")]
)
