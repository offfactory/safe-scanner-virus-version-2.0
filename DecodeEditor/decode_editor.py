"""Small local Decode Editor for files you own or are allowed to inspect."""

from __future__ import annotations

import base64
import binascii
import tkinter as tk
from pathlib import Path
from tkinter import filedialog, messagebox, ttk


class DecodeEditor(tk.Tk):
    def __init__(self) -> None:
        super().__init__()
        self.title("OffFactory Decode Editor")
        self.geometry("900x600")
        self.source: Path | None = None
        self.decoded: bytes | None = None
        self.mode = tk.StringVar(value="text")

        toolbar = ttk.Frame(self, padding=12)
        toolbar.pack(fill="x")
        ttk.Button(toolbar, text="Upload file", command=self.open_file).pack(side="left")
        ttk.Combobox(
            toolbar, textvariable=self.mode, values=("text", "base64", "hex"),
            state="readonly", width=12
        ).pack(side="left", padx=8)
        ttk.Button(toolbar, text="Decode", command=self.decode).pack(side="left")
        self.save_button = ttk.Button(toolbar, text="Save decoded file", command=self.save_file)
        self.save_button.pack(side="left", padx=8)
        self.save_button.state(["disabled"])

        self.editor = tk.Text(self, wrap="none", undo=True)
        self.editor.pack(fill="both", expand=True, padx=12, pady=(0, 12))
        self.status = ttk.Label(self, text="Choose a file to begin.")
        self.status.pack(fill="x", padx=12, pady=(0, 12))

    def open_file(self) -> None:
        selected = filedialog.askopenfilename()
        if not selected:
            return
        self.source = Path(selected)
        self.editor.delete("1.0", "end")
        self.editor.insert("1.0", self.source.read_bytes().decode("utf-8", errors="replace"))
        self.status.configure(text=f"Loaded {self.source.name}")

    def decode(self) -> None:
        if self.source is None:
            messagebox.showinfo("Decode Editor", "Upload a file first.")
            return
        raw = self.source.read_bytes()
        try:
            if self.mode.get() == "base64":
                decoded = base64.b64decode(raw, validate=True)
            elif self.mode.get() == "hex":
                decoded = binascii.unhexlify(b"".join(raw.split()))
            else:
                decoded = raw
        except (binascii.Error, ValueError) as error:
            messagebox.showerror("Decode failed", str(error))
            return
        self.decoded = decoded
        self.editor.delete("1.0", "end")
        self.editor.insert("1.0", decoded.decode("utf-8", errors="replace"))
        self.save_button.state(["!disabled"])
        self.status.configure(text=f"Decoded {len(decoded):,} bytes locally")

    def save_file(self) -> None:
        if self.decoded is None:
            return
        destination = filedialog.asksaveasfilename(
            initialfile=f"decoded-{self.source.name if self.source else 'file'}"
        )
        if destination:
            Path(destination).write_text(self.editor.get("1.0", "end-1c"), encoding="utf-8")
            self.status.configure(text=f"Saved {Path(destination).name}")


if __name__ == "__main__":
    DecodeEditor().mainloop()
