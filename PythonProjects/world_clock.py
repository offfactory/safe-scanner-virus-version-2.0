import tkinter as tk
from datetime import datetime
from zoneinfo import ZoneInfo

ZONES = ("UTC", "America/New_York", "Europe/London", "Africa/Kampala", "Asia/Tokyo", "Australia/Sydney")
app = tk.Tk()
app.title("OffFactory World Clock")
labels = {zone: tk.Label(app, font=("Segoe UI", 15), padx=20, pady=8) for zone in ZONES}
for label in labels.values():
    label.pack(anchor="w")


def tick():
    for zone, label in labels.items():
        label.configure(text=f"{zone}: {datetime.now(ZoneInfo(zone)).strftime('%Y-%m-%d %H:%M:%S')}")
    app.after(1000, tick)


tick()
app.mainloop()
