using System.Globalization;

namespace PBKKC.Pertemuan3;

internal sealed class CalculatorEngine
{
    public double Evaluate(string expression, bool degrees, double answer)
    {
        string normalized = expression.Replace("×", "*").Replace("÷", "/").Replace("−", "-")
            .Replace("π", "pi").Replace("√", "sqrt");
        var parser = new Parser(normalized, degrees, answer);
        double result = parser.Expression();
        if (!double.IsFinite(result)) throw new ArithmeticException("Hasil tidak terdefinisi.");
        return result;
    }

    private sealed class Parser(string text, bool degrees, double answer)
    {
        private int pos;
        private void Spaces() { while (pos < text.Length && char.IsWhiteSpace(text[pos])) pos++; }
        private bool Match(char c) { Spaces(); if (pos < text.Length && text[pos] == c) { pos++; return true; } return false; }
        private bool End { get { Spaces(); return pos >= text.Length; } }

        public double Expression()
        {
            double x = Term();
            while (true) { if (Match('+')) x += Term(); else if (Match('-')) x -= Term(); else break; }
            if (!End) throw new FormatException($"Karakter tidak dikenal pada posisi {pos + 1}.");
            return x;
        }
        private double Term()
        {
            double x = Power();
            while (true)
            {
                if (Match('*')) x *= Power();
                else if (Match('/')) { double d = Power(); if (Math.Abs(d) < 1e-15) throw new DivideByZeroException(); x /= d; }
                else break;
            }
            return x;
        }
        private double Power() { double x = Unary(); return Match('^') ? Math.Pow(x, Power()) : x; }
        private double Unary() { if (Match('+')) return Unary(); if (Match('-')) return -Unary(); return Postfix(); }
        private double Postfix() { double x = Primary(); while (Match('!')) x = Factorial(x); return x; }

        private double Primary()
        {
            Spaces();
            if (Match('(')) { double x = Additive(); if (!Match(')')) throw new FormatException("Kurung tutup tidak ditemukan."); return x; }
            Spaces();
            if (pos < text.Length && (char.IsDigit(text[pos]) || text[pos] == '.')) return Number();
            if (pos < text.Length && char.IsLetter(text[pos]))
            {
                string name = Name();
                if (name == "pi") return Math.PI;
                if (name == "e") return Math.E;
                if (name == "ans") return answer;
                if (!Match('(')) throw new FormatException($"Fungsi {name} harus diikuti tanda kurung.");
                double a = Additive(); if (!Match(')')) throw new FormatException("Kurung tutup tidak ditemukan.");
                return Function(name, a);
            }
            throw new FormatException("Ekspresi tidak lengkap atau token tidak valid.");
        }
        private double Additive()
        {
            double x = Term();
            while (true) { if (Match('+')) x += Term(); else if (Match('-')) x -= Term(); else break; }
            return x;
        }
        private double Number()
        {
            int start = pos; bool dot = false;
            while (pos < text.Length && (char.IsDigit(text[pos]) || (!dot && text[pos] == '.'))) { if (text[pos] == '.') dot = true; pos++; }
            if (!double.TryParse(text[start..pos], NumberStyles.Float, CultureInfo.InvariantCulture, out double value)) throw new FormatException("Angka tidak valid.");
            return value;
        }
        private string Name() { int start = pos; while (pos < text.Length && char.IsLetter(text[pos])) pos++; return text[start..pos].ToLowerInvariant(); }
        private double Factorial(double x)
        {
            if (x < 0 || x > 170 || Math.Abs(x - Math.Round(x)) > 1e-10) throw new ArithmeticException("n! hanya menerima bilangan bulat 0 sampai 170.");
            double y = 1; for (int i = 2; i <= (int)x; i++) y *= i; return y;
        }
        private double Function(string name, double x)
        {
            double rad = degrees ? x * Math.PI / 180.0 : x;
            return name switch
            {
                "sqrt" when x >= 0 => Math.Sqrt(x), "sqrt" => throw new ArithmeticException("Akar kuadrat bilangan negatif tidak real."),
                "cbrt" => Math.Cbrt(x), "ln" when x > 0 => Math.Log(x), "ln" => throw new ArithmeticException("ln hanya menerima nilai positif."),
                "log" when x > 0 => Math.Log10(x), "log" => throw new ArithmeticException("log hanya menerima nilai positif."),
                "sin" => Math.Sin(rad), "cos" => Math.Cos(rad), "tan" => Math.Tan(rad),
                "asin" when x is >= -1 and <= 1 => FromRad(Math.Asin(x)), "asin" => throw new ArithmeticException("asin menerima nilai -1 sampai 1."),
                "acos" when x is >= -1 and <= 1 => FromRad(Math.Acos(x)), "acos" => throw new ArithmeticException("acos menerima nilai -1 sampai 1."),
                "atan" => FromRad(Math.Atan(x)), "abs" => Math.Abs(x), "exp" => Math.Exp(x),
                "inv" when x != 0 => 1 / x, "inv" => throw new DivideByZeroException(),
                "sq" => x * x, "cube" => x * x * x, "ten" => Math.Pow(10, x),
                _ => throw new FormatException($"Fungsi '{name}' tidak didukung.")
            };
        }
        private double FromRad(double x) => degrees ? x * 180.0 / Math.PI : x;
    }
}
