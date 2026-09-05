import ast
import operator
import tkinter as tk
from tkinter import ttk

OPS = {ast.Add: operator.add, ast.Sub: operator.sub, ast.Mult: operator.mul, ast.Div: operator.truediv, ast.Pow: operator.pow}


def calculate(expression):
    def evaluate(node):
        if isinstance(node, ast.Expression):
            return evaluate(node.body)
        if isinstance(node, ast.Constant) and isinstance(node.value, (int, float)):
            return node.value
        if isinstance(node, ast.BinOp) and type(node.op) in OPS:
            return OPS[type(node.op)](evaluate(node.left), evaluate(node.right))
        if isinstance(node, ast.UnaryOp) and isinstance(node.op, (ast.UAdd, ast.USub)):
            return evaluate(node.operand) * (-1 if isinstance(node.op, ast.USub) else 1)
        raise ValueError("Only numbers and + - * / ** are supported")
    return evaluate(ast.parse(expression, mode="eval"))


app = tk.Tk()
app.title("OffFactory Calculator")
entry = ttk.Entry(app, width=32)
entry.pack(padx=16, pady=16)
result = ttk.Label(app, text="Enter a calculation")
result.pack(pady=(0, 12))
ttk.Button(app, text="Calculate", command=lambda: result.configure(text=str(calculate(entry.get())))).pack(pady=(0, 16))
app.mainloop()
