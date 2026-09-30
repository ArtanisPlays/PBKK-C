using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using PBKKC.Pertemuan4.Models;

namespace PBKKC.Pertemuan4.Services
{
    public static class StudentRegistration
    {
        public static readonly ReadOnlyCollection<string> StudyPrograms =
            Array.AsReadOnly(new[] { "Informatika", "Sistem Informasi", "Teknik Komputer", "Teknologi Informasi" });

        public static bool TryCreate(StudentDraft draft, IEnumerable<Student> students,
            Student currentStudent, out Student student, out string error)
        {
            student = null;
            error = null;
            string nim = (draft.Nim ?? "").Trim();
            string name = (draft.Name ?? "").Trim();
            string email = (draft.Email ?? "").Trim();
            string phone = (draft.Phone ?? "").Trim();
            string address = (draft.Address ?? "").Trim();

            if (!Regex.IsMatch(nim, @"^[0-9]{1,20}$"))
                error = "NIM wajib diisi dengan angka (maksimal 20 digit).";
            else if (currentStudent != null && nim != currentStudent.Nim)
                error = "NIM tidak dapat diubah saat mengedit mahasiswa.";
            else if (students.Any(item => !ReferenceEquals(item, currentStudent) && item.Nim == nim))
                error = "NIM sudah terdaftar. Gunakan NIM yang berbeda.";
            else if (name.Length == 0 || name.Length > 100)
                error = "Nama lengkap wajib diisi (maksimal 100 karakter).";
            else if (!StudyPrograms.Contains(draft.StudyProgram))
                error = "Pilih program studi mahasiswa.";
            else if (draft.Gender != "Laki-laki" && draft.Gender != "Perempuan")
                error = "Pilih jenis kelamin mahasiswa.";
            else if (!draft.DateOfBirth.HasValue || draft.DateOfBirth.Value.Date > DateTime.Today)
                error = "Tanggal lahir wajib diisi dan tidak boleh melewati hari ini.";
            else if (!IsValidEmail(email))
                error = "Masukkan email yang valid, misalnya nama@example.com.";
            else if (phone.Length > 0 && !Regex.IsMatch(Regex.Replace(phone, @"[\s-]", ""), @"^\+?[0-9]{6,15}$"))
                error = "Nomor telepon harus berisi 6–15 digit; boleh diawali + serta memakai spasi atau tanda hubung.";
            else if (address.Length > 500)
                error = "Alamat maksimal 500 karakter.";

            if (error != null)
                return false;

            student = new Student
            {
                Nim = nim,
                Name = name,
                StudyProgram = draft.StudyProgram,
                Gender = draft.Gender,
                DateOfBirth = draft.DateOfBirth.Value.Date,
                Email = email,
                Phone = phone,
                Address = address
            };
            return true;
        }

        private static bool IsValidEmail(string email)
        {
            if (email.Length == 0 || email.Length > 254 || email.Any(char.IsWhiteSpace))
                return false;
            try
            {
                var address = new MailAddress(email);
                return address.Address == email && address.Host.IndexOf('.') > 0;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
