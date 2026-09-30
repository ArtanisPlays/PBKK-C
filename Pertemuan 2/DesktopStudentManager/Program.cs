namespace DesktopStudentManager;
internal static class Program
{
    [STAThread]
    private static void Main() { ApplicationConfiguration.Initialize(); Application.Run(new StudentForm()); }
}
internal sealed class Student
{
    public string Nim { get; set; } = "";
    public string Name { get; set; } = "";
    public string Program { get; set; } = "";
    public double Gpa { get; set; }
}
internal sealed class StudentForm : Form
{
    private readonly BindingSource source = new();
    private readonly List<Student> students = new();
    private readonly DataGridView grid = new();
    private readonly TextBox nim = new(), nameBox = new(), program = new(), gpa = new(), search = new();
    private readonly Label status = new();
    public StudentForm()
    {
        Text = "Sistem Data Mahasiswa — PBKK C"; StartPosition = FormStartPosition.CenterScreen; Size = new Size(1000, 680); MinimumSize = new Size(820, 560); Font = new Font("Segoe UI", 10); BackColor = Color.FromArgb(247, 248, 252);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 1, RowCount = 4 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        var heading = new Label { Text = "Sistem Data Mahasiswa", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.FromArgb(35, 70, 110), TextAlign = ContentAlignment.MiddleLeft };
        var form = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 2, Padding = new Padding(0, 8, 0, 8) };
        for (int i = 0; i < 5; i++) form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        form.Controls.Add(Field("NIM", nim), 0, 0); form.Controls.Add(Field("Nama", nameBox), 1, 0); form.Controls.Add(Field("Program Studi", program), 2, 0); form.Controls.Add(Field("IPK (0–4)", gpa), 3, 0);
        var add = ActionButton("Tambah", Color.FromArgb(45, 130, 95)); add.Click += (_, _) => AddStudent(); form.Controls.Add(add, 4, 0);
        var searchLabel = new Label { Text = "Cari:", AutoSize = true, Anchor = AnchorStyles.Left }; form.Controls.Add(searchLabel, 0, 1);
        search.PlaceholderText = "NIM, nama, atau program studi"; search.Dock = DockStyle.Fill; search.TextChanged += (_, _) => RefreshRows(); form.Controls.Add(search, 1, 1); form.SetColumnSpan(search, 2);
        var edit = ActionButton("Simpan Edit", Color.FromArgb(70, 105, 170)); edit.Click += (_, _) => EditStudent(); form.Controls.Add(edit, 3, 1);
        var delete = ActionButton("Hapus", Color.FromArgb(180, 75, 75)); delete.Click += (_, _) => DeleteStudent(); form.Controls.Add(delete, 4, 1);
        grid.Dock = DockStyle.Fill; grid.ReadOnly = true; grid.AllowUserToAddRows = false; grid.AutoGenerateColumns = true; grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; grid.MultiSelect = false; grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; grid.SelectionChanged += (_, _) => LoadSelection();
        source.DataSource = students; grid.DataSource = source;
        status.Dock = DockStyle.Fill; status.TextAlign = ContentAlignment.MiddleLeft;
        layout.Controls.Add(heading, 0, 0); layout.Controls.Add(form, 0, 1); layout.Controls.Add(grid, 0, 2); layout.Controls.Add(status, 0, 3); Controls.Add(layout); RefreshRows();
    }
    private static Control Field(string label, TextBox box) { var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4) }; var l = new Label { Text = label, Dock = DockStyle.Top, Height = 24 }; box.Dock = DockStyle.Top; panel.Controls.Add(box); panel.Controls.Add(l); return panel; }
    private static Button ActionButton(string text, Color color) => new() { Text = text, Dock = DockStyle.Fill, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Margin = new Padding(4) };
    private void AddStudent()
    {
        string id = nim.Text.Trim(); if (id.Length == 0 || nameBox.Text.Trim().Length == 0 || program.Text.Trim().Length == 0) { MessageBox.Show("Semua kolom wajib diisi."); return; }
        if (students.Any(s => s.Nim.Equals(id, StringComparison.OrdinalIgnoreCase))) { MessageBox.Show("NIM sudah terdaftar."); return; }
        if (!TryGpa(out double value)) return;
        students.Add(new Student { Nim = id, Name = nameBox.Text.Trim(), Program = program.Text.Trim(), Gpa = value }); ClearFields(); RefreshRows();
    }
    private void EditStudent()
    {
        if (grid.CurrentRow?.DataBoundItem is not Student s) { MessageBox.Show("Pilih data yang akan diedit."); return; }
        if (nameBox.Text.Trim().Length == 0 || program.Text.Trim().Length == 0) { MessageBox.Show("Nama dan program studi wajib diisi."); return; }
        if (!TryGpa(out double value)) return;
        s.Name = nameBox.Text.Trim(); s.Program = program.Text.Trim(); s.Gpa = value; RefreshRows();
    }
    private void DeleteStudent()
    {
        if (grid.CurrentRow?.DataBoundItem is not Student s) { MessageBox.Show("Pilih data yang akan dihapus."); return; }
        if (MessageBox.Show($"Hapus data {s.Name} ({s.Nim})?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) { students.Remove(s); ClearFields(); RefreshRows(); }
    }
    private bool TryGpa(out double value)
    {
        string text = gpa.Text.Trim().Replace(',', '.');
        if (double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value) && double.IsFinite(value) && value is >= 0 and <= 4) return true;
        MessageBox.Show("IPK harus berupa angka dari 0 sampai 4."); return false;
    }
    private void LoadSelection() { if (grid.CurrentRow?.DataBoundItem is Student s) { nim.Text = s.Nim; nameBox.Text = s.Name; program.Text = s.Program; gpa.Text = s.Gpa.ToString("0.00"); nim.ReadOnly = true; } }
    private void ClearFields() { nim.ReadOnly = false; nim.Clear(); nameBox.Clear(); program.Clear(); gpa.Clear(); }
    private void RefreshRows()
    {
        string q = search.Text.Trim(); var view = students.Where(s => s.Nim.Contains(q, StringComparison.OrdinalIgnoreCase) || s.Name.Contains(q, StringComparison.OrdinalIgnoreCase) || s.Program.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        source.DataSource = view; status.Text = $"Jumlah mahasiswa: {students.Count}    |    Rata-rata IPK: {(students.Count == 0 ? 0 : students.Average(s => s.Gpa)):0.00}    |    Data tersimpan sementara selama aplikasi berjalan.";
    }
}
