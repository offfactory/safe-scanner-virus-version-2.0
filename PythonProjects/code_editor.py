"""Local Python code editor. Run only files you trust."""
from pathlib import Path
import subprocess
import sys
import tkinter as tk
from tkinter import filedialog, messagebox, ttk


class CodeEditor(tk.Tk):
    def __init__(self):
        super().__init__()
        self.title("OffFactory Python Code Editor")
        self.geometry("1000x700")
        self.path = None
        bar = ttk.Frame(self, padding=8)
        bar.pack(fill="x")
        for label, command in (("Open", self.open_file), ("Save", self.save_file), ("Run Python", self.run_file)):
            ttk.Button(bar, text=label, command=command).pack(side="left", padx=3)
        self.editor = tk.Text(self, wrap="none", undo=True, font=("Consolas", 11))
        self.editor.pack(fill="both", expand=True, padx=8)
        self.output = tk.Text(self, height=8, state="disabled", background="#101018", foreground="#dcd6f7")
        self.output.pack(fill="x", padx=8, pady=8)

    def open_file(self):
        selected = filedialog.askopenfilename(filetypes=[("Code files", "*.py *.js *.cs *.swift *.txt"), ("All files", "*.*")])
        if selected:
            self.path = Path(selected)
            self.editor.delete("1.0", "end")
            self.editor.insert("1.0", self.path.read_text(encoding="utf-8", errors="replace"))

    def save_file(self):
        if self.path is None:
            selected = filedialog.asksaveasfilename(defaultextension=".py")
            if not selected:
                return
            self.path = Path(selected)
        self.path.write_text(self.editor.get("1.0", "end-1c"), encoding="utf-8")

    def run_file(self):
        if self.path is None or self.path.suffix.lower() != ".py":
            messagebox.showinfo("Python Code Editor", "Save or open a .py file first.")
            return
        self.save_file()
        result = subprocess.run([sys.executable, str(self.path)], capture_output=True, text=True, timeout=30)
        self.output.configure(state="normal")
        self.output.delete("1.0", "end")
        self.output.insert("1.0", (result.stdout + result.stderr).strip() or "Program finished with no output.")
        self.output.configure(state="disabled")


if __name__ == "__main__":
    CodeEditor().mainloop()
