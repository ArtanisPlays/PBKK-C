using System.Globalization;

namespace PBKKC.Pertemuan3;

internal sealed class CalculatorForm : Form
{
    private readonly CalculatorEngine engine = new();
    private readonly TextBox expression = new();
    private readonly Label result = new();
    private readonly Label answerLabel = new();
    private readonly Label modeLabel = new();
    private double lastAnswer, memory;
    private bool degrees = true;
    private bool justCalculated;

    public CalculatorForm()
    {
        Text = "Scientific Calculator — PBKK C";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(650, 700);
        Size = new Size(760, 850);
        BackColor = Color.FromArgb(247, 248, 252);
        Font = new Font("Segoe UI", 10);
        BuildUi();
        KeyPreview = true;
        KeyDown += HandleKey;
    }

    private void BuildUi()
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 62, BackColor = Color.FromArgb(170, 150, 218) };
        var title = new Label { Text = "Scientific Calculator", ForeColor = Color.White, Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true, Location = new Point(18, 15) };
        var mode = Button("DEG", Color.FromArgb(168, 216, 234)); mode.Dock = DockStyle.Right; mode.Width = 80; mode.Click += (_, _) => { degrees = !degrees; mode.Text = degrees ? "DEG" : "RAD"; modeLabel.Text = mode.Text; };
        header.Controls.Add(title); header.Controls.Add(mode);
        Controls.Add(header);

        var display = new Panel { Dock = DockStyle.Top, Height = 148, Padding = new Padding(16), BackColor = Color.White };
        modeLabel.Text = "DEG"; modeLabel.ForeColor = Color.FromArgb(120, 90, 180); modeLabel.Dock = DockStyle.Top; modeLabel.Height = 24;
        expression.ReadOnly = true; expression.BorderStyle = BorderStyle.None; expression.Font = new Font("Segoe UI", 17); expression.TextAlign = HorizontalAlignment.Right; expression.Dock = DockStyle.Top; expression.Height = 42;
        result.Text = "0"; result.Font = new Font("Segoe UI", 26, FontStyle.Bold); result.TextAlign = ContentAlignment.MiddleRight; result.Dock = DockStyle.Fill;
        answerLabel.Text = "Ans = 0"; answerLabel.ForeColor = Color.Gray; answerLabel.Dock = DockStyle.Bottom; answerLabel.Height = 24; answerLabel.TextAlign = ContentAlignment.MiddleRight;
        display.Controls.Add(result); display.Controls.Add(expression); display.Controls.Add(modeLabel); display.Controls.Add(answerLabel); Controls.Add(display);

        string[][] rows =
        {
            new[] {"2nd","π","e","x²","x³","xʸ"},
            new[] {"sin","cos","tan","√x","³√x","n!"},
            new[] {"asin","acos","atan","ln","log","1/x"},
            new[] {"MC","MR","M+","M-","MS","Ans"},
            new[] {"(",")","%","÷","⌫","AC"},
            new[] {"7","8","9","×","exp","10ˣ"},
            new[] {"4","5","6","−","abs","RND"},
            new[] {"1","2","3","+","±","."},
            new[] {"0","00","!","=","",""}
        };
        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8), ColumnCount = 6, RowCount = rows.Length };
        for (int c = 0; c < 6; c++) grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 6));
        for (int r = 0; r < rows.Length; r++) grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows.Length));
        foreach (string[] row in rows)
            foreach (string text in row)
            {
                var b = Button(text, IsDigit(text) ? Color.White : Color.FromArgb(232, 229, 245));
                b.Click += (_, _) => Press(text);
                grid.Controls.Add(b);
            }
        // Replace the final row with a wide equals button and a clear visual emphasis.
        var equals = grid.GetControlFromPosition(3, 8) as Button;
        if (equals is not null) { grid.Controls.Remove(equals); equals.Dispose(); }
        var wideEquals = Button("=", Color.FromArgb(170, 150, 218)); wideEquals.ForeColor = Color.White; wideEquals.Font = new Font("Segoe UI", 15, FontStyle.Bold); wideEquals.Click += (_, _) => Calculate();
        grid.Controls.Add(wideEquals, 3, 8); grid.SetColumnSpan(wideEquals, 3);
        Controls.Add(grid); grid.BringToFront();
    }

    private static bool IsDigit(string text) => text.Length > 0 && text.All(char.IsDigit) || text == ".";
    private static Button Button(string text, Color color) => new() { Text = text, Dock = DockStyle.Fill, Margin = new Padding(4), BackColor = color, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10), Cursor = Cursors.Hand };

    private void Press(string key)
    {
        if (key == "=") { Calculate(); return; }
        if (key == "AC") { expression.Clear(); result.Text = "0"; answerLabel.Text = "Ans = " + Format(lastAnswer); justCalculated = false; return; }
        if (key == "⌫") { if (expression.TextLength > 0) expression.Text = expression.Text[..^1]; justCalculated = false; return; }
        if (key == "DEG") { degrees = !degrees; modeLabel.Text = degrees ? "DEG" : "RAD"; return; }
        if (key == "MC") { memory = 0; return; }
        if (key == "MR") { Append(Format(memory)); return; }
        if (key is "M+" or "M-" or "MS") { double value = CurrentValue(); if (key == "M+") memory += value; else if (key == "M-") memory -= value; else memory = value; return; }
        if (key == "RND") { expression.Text = Format(Math.Round(CurrentValue())); justCalculated = false; return; }
        if (key == "±") { if (expression.TextLength > 0) expression.Text = "-(" + expression.Text + ")"; justCalculated = false; return; }
        if (key == "%") { if (expression.TextLength > 0) expression.Text = "(" + expression.Text + ")/100"; justCalculated = false; return; }
        if (key == "2nd") return;
        string token = key switch
        {
            "π" => "pi", "x²" => "sq(", "x³" => "cube(", "xʸ" => "^", "√x" => "sqrt(", "³√x" => "cbrt(",
            "1/x" => "inv(", "10ˣ" => "ten(", "!" or "n!" => "!", "exp" or "sin" or "cos" or "tan" or "asin" or "acos" or "atan" or "ln" or "log" or "abs" => key + "(",
            "×" => "*", "÷" => "/", "−" => "-", _ => key
        };
        Append(token);
    }

    private void Append(string value)
    {
        if (justCalculated && value.Length > 0 && (char.IsDigit(value[0]) || value is "pi" or "e")) expression.Clear();
        justCalculated = false; expression.AppendText(value);
    }

    private void Calculate()
    {
        if (string.IsNullOrWhiteSpace(expression.Text)) return;
        try { lastAnswer = engine.Evaluate(expression.Text, degrees, lastAnswer); result.Text = Format(lastAnswer); answerLabel.Text = "Ans = " + Format(lastAnswer); }
        catch (Exception ex) { result.Text = ex.Message; }
        justCalculated = true;
    }
    private double CurrentValue() { try { return expression.TextLength == 0 ? lastAnswer : engine.Evaluate(expression.Text, degrees, lastAnswer); } catch { return 0; } }
    private static string Format(double value) => (Math.Abs(value) < 1e-12 ? 0 : value).ToString("G12", CultureInfo.InvariantCulture);
    private void HandleKey(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode is >= Keys.D0 and <= Keys.D9) { Press(((int)e.KeyCode - (int)Keys.D0).ToString()); e.SuppressKeyPress = true; }
        else if (e.KeyCode is >= Keys.NumPad0 and <= Keys.NumPad9) { Press(((int)e.KeyCode - (int)Keys.NumPad0).ToString()); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.Enter) { Calculate(); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.Back) { Press("⌫"); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.Escape) { Press("AC"); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.Decimal) { Press("."); e.SuppressKeyPress = true; }
    }
}

