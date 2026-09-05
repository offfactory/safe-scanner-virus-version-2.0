using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace SafeScan.Themes
{
    public class ThemeManager
    {
        private readonly Dictionary<string, ThemeDefinition> _themes = new Dictionary<string, ThemeDefinition>
        {
            ["SafeScan Dark"] = new ThemeDefinition("SafeScan Dark", Color.FromArgb(12, 22, 38), Color.FromArgb(22, 32, 48), Color.FromArgb(22, 132, 255), Color.FromArgb(16, 24, 35), Color.FromArgb(30, 45, 60), Color.FromArgb(255, 255, 255)),
            ["SafeScan Light"] = new ThemeDefinition("SafeScan Light", Color.FromArgb(240, 244, 250), Color.FromArgb(219, 227, 237), Color.FromArgb(37, 99, 235), Color.FromArgb(255, 255, 255), Color.FromArgb(244, 247, 252), Color.FromArgb(20, 20, 20)),
            ["Midnight Blue"] = new ThemeDefinition("Midnight Blue", Color.FromArgb(7, 12, 28), Color.FromArgb(18, 28, 45), Color.FromArgb(47, 128, 237), Color.FromArgb(15, 23, 42), Color.FromArgb(20, 35, 60), Color.FromArgb(210, 227, 255)),
            ["Cyber Green"] = new ThemeDefinition("Cyber Green", Color.FromArgb(4, 28, 25), Color.FromArgb(8, 48, 38), Color.FromArgb(33, 209, 102), Color.FromArgb(9, 33, 30), Color.FromArgb(13, 52, 46), Color.FromArgb(200, 255, 230)),
            ["Purple Neon"] = new ThemeDefinition("Purple Neon", Color.FromArgb(20, 10, 36), Color.FromArgb(41, 20, 62), Color.FromArgb(168, 85, 247), Color.FromArgb(31, 15, 49), Color.FromArgb(55, 23, 78), Color.FromArgb(245, 222, 255)),
            ["Red Alert"] = new ThemeDefinition("Red Alert", Color.FromArgb(28, 6, 6), Color.FromArgb(60, 16, 16), Color.FromArgb(220, 38, 38), Color.FromArgb(20, 8, 8), Color.FromArgb(80,20,20), Color.FromArgb(255, 215, 215))
        };

        public ThemeDefinition GetTheme(string name)
        {
            return _themes.TryGetValue(name, out var theme) ? theme : _themes["SafeScan Dark"];
        }

        public void ApplyTheme(Form form, string name)
        {
            var theme = GetTheme(name);
            form.BackColor = theme.Background;
            form.ForeColor = theme.Text;

            foreach (var control in GetAllControls(form))
            {
                if (control is Label label)
                {
                    label.ForeColor = theme.Text;
                }
                else if (control is Button button)
                {
                    button.BackColor = theme.Button;
                    button.ForeColor = Color.White;
                }
                else if (control is CheckBox checkBox)
                {
                    checkBox.ForeColor = theme.Text;
                }
                else if (control is ComboBox comboBox)
                {
                    comboBox.BackColor = theme.Card;
                    comboBox.ForeColor = theme.Text;
                }
                else if (control is ListView listView)
                {
                    listView.BackColor = theme.Card;
                    listView.ForeColor = theme.Text;
                }
                else if (control is DataGridView gridView)
                {
                    gridView.BackgroundColor = theme.Card;
                    gridView.GridColor = theme.Sidebar;
                    gridView.ColumnHeadersDefaultCellStyle.BackColor = theme.Button;
                    gridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                    gridView.ForeColor = theme.Text;
                }
                else if (control is TextBox textBox)
                {
                    textBox.BackColor = theme.Card;
                    textBox.ForeColor = theme.Text;
                }
                else if (control is ProgressBar progressBar)
                {
                    progressBar.BackColor = theme.Sidebar;
                    progressBar.ForeColor = theme.Button;
                }
                else if (control is TabPage tabPage)
                {
                    tabPage.BackColor = theme.Background;
                    tabPage.ForeColor = theme.Text;
                }
            }
        }

        private IEnumerable<Control> GetAllControls(Control container)
        {
            foreach (Control child in container.Controls)
            {
                yield return child;
                foreach (var nested in GetAllControls(child))
                {
                    yield return nested;
                }
            }
        }
    }

    public class ThemeDefinition
    {
        public ThemeDefinition(string name, Color background, Color sidebar, Color button, Color card, Color table, Color text)
        {
            Name = name;
            Background = background;
            Sidebar = sidebar;
            Button = button;
            Card = card;
            Table = table;
            Text = text;
        }

        public string Name { get; }
        public Color Background { get; }
        public Color Sidebar { get; }
        public Color Button { get; }
        public Color Card { get; }
        public Color Table { get; }
        public Color Text { get; }
    }
}
