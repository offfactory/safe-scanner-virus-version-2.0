import SwiftUI

@main
struct PrecisionClockApp: App {
    var body: some Scene {
        WindowGroup("Precision Timekeeping") { ClockView() }
    }
}

struct ClockView: View {
    private let zones = ["UTC", "America/New_York", "Europe/London", "Africa/Kampala", "Asia/Tokyo", "Australia/Sydney"]
    @State private var now = Date()
    private let timer = Timer.publish(every: 1, on: .main, in: .common).autoconnect()

    var body: some View {
        VStack(alignment: .leading, spacing: 18) {
            Text("Precision Timekeeping").font(.largeTitle.bold())
            Text("OffFactory World Clock").foregroundStyle(.secondary)
            ForEach(zones, id: \.self) { zone in
                HStack {
                    Text(zone.replacingOccurrences(of: "_", with: " ")).frame(width: 190, alignment: .leading)
                    Text(now.formatted(.dateTime.timeZone(TimeZone(identifier: zone) ?? .current).year().month().day().hour().minute().second()))
                        .font(.system(.body, design: .monospaced))
                    Spacer()
                }
                .padding(.vertical, 6)
                .overlay(alignment: .bottom) { Divider() }
            }
        }
        .padding(28)
        .frame(minWidth: 560, minHeight: 390)
        .onReceive(timer) { now = $0 }
    }
}
