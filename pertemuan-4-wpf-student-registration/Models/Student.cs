using System;

namespace PBKKC.Pertemuan4.Models
{
    public sealed class Student
    {
        public string Nim { get; set; }
        public string Name { get; set; }
        public string StudyProgram { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }

    // Form input allows an empty date before validation creates a Student.
    public sealed class StudentDraft
    {
        public string Nim { get; set; }
        public string Name { get; set; }
        public string StudyProgram { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }
}
