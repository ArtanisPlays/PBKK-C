using System.Globalization;

namespace PBKKC.Pertemuan2;

internal sealed class Student
{
    public string Nim { get; }
    public string Name { get; set; }
    public string Program { get; set; }
    public double Gpa { get; set; }

    public Student(string nim, string name, string program, double gpa)
    {
        Nim = nim;
        Name = name;
        Program = program;
        Gpa = gpa;
    }
}

internal static class Program
{
    private static readonly List<Student> Students = new();

    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("Hello, World! — PBKK C / Pertemuan 2");
        while (true)
        {
            Console.WriteLine("\n=== SISTEM DATA MAHASISWA ===");
            Console.WriteLine("1. Tambah mahasiswa\n2. Tampilkan semua\n3. Cari mahasiswa\n4. Edit mahasiswa\n5. Hapus mahasiswa\n0. Keluar");
            Console.Write("Pilih menu: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": AddStudent(); break;
                case "2": ListStudents(Students); break;
                case "3": SearchStudent(); break;
                case "4": EditStudent(); break;
                case "5": DeleteStudent(); break;
                case "0": return;
                default: Console.WriteLine("Pilihan tidak dikenal."); break;
            }
        }
    }

    private static void AddStudent()
    {
        string nim = ReadRequired("NIM: ");
        if (Students.Any(s => s.Nim.Equals(nim, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("NIM sudah terdaftar.");
            return;
        }
        string name = ReadRequired("Nama: ");
        string program = ReadRequired("Program studi: ");
        double gpa = ReadGpa("IPK (0.00-4.00): ");
        Students.Add(new Student(nim, name, program, gpa));
        Console.WriteLine("Data mahasiswa berhasil ditambahkan.");
    }

    private static void SearchStudent()
    {
        string query = ReadRequired("Cari berdasarkan NIM, nama, atau program studi: ");
        ListStudents(Students.Where(s => s.Nim.Contains(query, StringComparison.OrdinalIgnoreCase)
            || s.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
            || s.Program.Contains(query, StringComparison.OrdinalIgnoreCase)));
    }

    private static void EditStudent()
    {
        Student? student = FindByNim(ReadRequired("NIM yang akan diedit: "));
        if (student is null) { Console.WriteLine("Mahasiswa tidak ditemukan."); return; }
        student.Name = ReadOptional("Nama", student.Name);
        student.Program = ReadOptional("Program studi", student.Program);
        student.Gpa = ReadOptionalGpa(student.Gpa);
        Console.WriteLine("Data berhasil diperbarui.");
    }

    private static void DeleteStudent()
    {
        Student? student = FindByNim(ReadRequired("NIM yang akan dihapus: "));
        if (student is null) { Console.WriteLine("Mahasiswa tidak ditemukan."); return; }
        Console.Write($"Hapus data {student.Name} ({student.Nim})? [y/N]: ");
        if (Console.ReadLine()?.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) == true)
        {
            Students.Remove(student);
            Console.WriteLine("Data berhasil dihapus.");
        }
        else Console.WriteLine("Penghapusan dibatalkan.");
    }

    private static Student? FindByNim(string nim) => Students.FirstOrDefault(s => s.Nim.Equals(nim, StringComparison.OrdinalIgnoreCase));

    private static void ListStudents(IEnumerable<Student> students)
    {
        Student[] rows = students.ToArray();
        if (rows.Length == 0) { Console.WriteLine("Belum ada data yang cocok."); return; }
        Console.WriteLine("\n{0,-16} {1,-28} {2,-30} {3,5}", "NIM", "NAMA", "PROGRAM STUDI", "IPK");
        Console.WriteLine(new string('-', 84));
        foreach (Student s in rows)
            Console.WriteLine("{0,-16} {1,-28} {2,-30} {3,5:F2}", s.Nim, TrimTo(s.Name, 28), TrimTo(s.Program, 30), s.Gpa);
    }

    private static string ReadRequired(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string value = Console.ReadLine()?.Trim() ?? "";
            if (value.Length > 0) return value;
            Console.WriteLine("Nilai wajib diisi.");
        }
    }

    private static double ReadGpa(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = (Console.ReadLine() ?? "").Trim().Replace(',', '.');
            if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double gpa) && double.IsFinite(gpa) && gpa is >= 0 and <= 4)
                return gpa;
            Console.WriteLine("Masukkan angka IPK yang valid antara 0 dan 4.");
        }
    }

    private static string ReadOptional(string label, string current)
    {
        Console.Write($"{label} [{current}] (Enter untuk mempertahankan): ");
        string value = Console.ReadLine()?.Trim() ?? "";
        return value.Length == 0 ? current : value;
    }

    private static double ReadOptionalGpa(double current)
    {
        Console.Write($"IPK [{current:F2}] (Enter untuk mempertahankan): ");
        string input = (Console.ReadLine() ?? "").Trim();
        if (input.Length == 0) return current;
        input = input.Replace(',', '.');
        if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double gpa) && double.IsFinite(gpa) && gpa is >= 0 and <= 4)
            return gpa;
        Console.WriteLine("IPK tidak valid; nilai sebelumnya dipertahankan.");
        return current;
    }

    private static string TrimTo(string value, int max) => value.Length <= max ? value : value[..(max - 1)] + "…";
}



