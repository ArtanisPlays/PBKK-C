using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using PBKKC.Pertemuan4.Models;
using PBKKC.Pertemuan4.Services;

namespace PBKKC.Pertemuan4
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<Student> _students = new ObservableCollection<Student>();
        private Student _currentStudent;
        private bool _suppressSelectionChanged;
        private bool _invalidDateText;

        public ICollectionView StudentsView { get; private set; }
        public ReadOnlyCollection<string> StudyPrograms { get { return StudentRegistration.StudyPrograms; } }

        public MainWindow()
        {
            InitializeComponent();
            StudentsView = CollectionViewSource.GetDefaultView(_students);
            StudentsView.Filter = MatchesSearch;
            DataContext = this;
            BirthDatePicker.DisplayDateEnd = DateTime.Today;
            UpdateSummary();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Commit text typed into the DatePicker before checking its selected date.
            SaveButton.Focus();
            if (_invalidDateText || !string.IsNullOrWhiteSpace(BirthDatePicker.Text) && !BirthDatePicker.SelectedDate.HasValue)
            {
                ShowValidation("Tanggal lahir tidak valid. Pilih tanggal melalui kalender.");
                return;
            }

            var draft = new StudentDraft
            {
                Nim = NimBox.Text,
                Name = NameBox.Text,
                StudyProgram = StudyProgramBox.SelectedItem as string,
                Gender = MaleButton.IsChecked == true ? "Laki-laki" : FemaleButton.IsChecked == true ? "Perempuan" : null,
                DateOfBirth = BirthDatePicker.SelectedDate,
                Email = EmailBox.Text,
                Phone = PhoneBox.Text,
                Address = AddressBox.Text
            };

            Student student;
            string error;
            if (!StudentRegistration.TryCreate(draft, _students, _currentStudent, out student, out error))
            {
                ShowValidation(error);
                return;
            }

            bool editing = _currentStudent != null;
            _suppressSelectionChanged = true;
            try
            {
                if (editing)
                    _students[_students.IndexOf(_currentStudent)] = student;
                else
                    _students.Add(student);
                StudentsView.Refresh();
            }
            finally
            {
                _suppressSelectionChanged = false;
            }
            ResetForm();
            UpdateSummary();
            StatusText.Text = editing ? "Data " + student.Name + " berhasil diperbarui." : student.Name + " berhasil didaftarkan.";
        }

        private void StudentsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressSelectionChanged)
                return;
            var student = StudentsGrid.SelectedItem as Student;
            if (student == null)
            {
                if (_currentStudent != null)
                    ResetForm();
                return;
            }

            _currentStudent = student;
            NimBox.Text = student.Nim;
            NimBox.IsReadOnly = true;
            NimHint.Text = "NIM tetap saat data mahasiswa diedit.";
            NameBox.Text = student.Name;
            StudyProgramBox.SelectedItem = student.StudyProgram;
            MaleButton.IsChecked = student.Gender == "Laki-laki";
            FemaleButton.IsChecked = student.Gender == "Perempuan";
            BirthDatePicker.SelectedDate = student.DateOfBirth;
            _invalidDateText = false;
            EmailBox.Text = student.Email;
            PhoneBox.Text = student.Phone;
            AddressBox.Text = student.Address;
            FormHeading.Text = "Edit mahasiswa";
            SaveButton.Content = "Simpan perubahan";
            DeleteButton.IsEnabled = true;
            ValidationBanner.Visibility = Visibility.Collapsed;
            StatusText.Text = "Mengedit mahasiswa dengan NIM " + student.Nim + ".";
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStudent == null)
                return;
            var student = _currentStudent;
            var result = MessageBox.Show(this,
                "Hapus data " + student.Name + " (" + student.Nim + ")?",
                "Konfirmasi hapus", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (result != MessageBoxResult.Yes)
                return;

            ResetForm();
            _students.Remove(student);
            UpdateSummary();
            StatusText.Text = "Data " + student.Name + " berhasil dihapus.";
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
            StatusText.Text = "Form dikosongkan. Siap menerima pendaftaran baru.";
            NimBox.Focus();
        }

        private void ResetForm()
        {
            _suppressSelectionChanged = true;
            try
            {
                StudentsGrid.UnselectAll();
                _currentStudent = null;
                NimBox.IsReadOnly = false;
                NimBox.Clear();
                NimHint.Text = "Gunakan NIM unik, maksimal 20 digit.";
                NameBox.Clear();
                StudyProgramBox.SelectedIndex = -1;
                MaleButton.IsChecked = false;
                FemaleButton.IsChecked = false;
                BirthDatePicker.SelectedDate = null;
                BirthDatePicker.Text = "";
                _invalidDateText = false;
                EmailBox.Clear();
                PhoneBox.Clear();
                AddressBox.Clear();
                FormHeading.Text = "Pendaftaran baru";
                SaveButton.Content = "Daftarkan mahasiswa";
                DeleteButton.IsEnabled = false;
                ValidationBanner.Visibility = Visibility.Collapsed;
            }
            finally
            {
                _suppressSelectionChanged = false;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (StudentsView == null)
                return;
            StudentsView.Refresh();
            UpdateSummary();
        }

        private bool MatchesSearch(object item)
        {
            var student = item as Student;
            if (student == null)
                return false;
            string query = SearchBox.Text.Trim();
            return student.Nim.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0
                || student.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0
                || student.StudyProgram.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void UpdateSummary()
        {
            int visible = StudentsView.Cast<Student>().Count();
            TotalCountText.Text = _students.Count.ToString();
            VisibleCountText.Text = "Menampilkan " + visible + " dari " + _students.Count + " mahasiswa";
            EmptyState.Visibility = visible == 0 ? Visibility.Visible : Visibility.Collapsed;
            EmptyHeading.Text = _students.Count == 0 ? "Belum ada mahasiswa" : "Tidak ada hasil pencarian";
            EmptyDescription.Text = _students.Count == 0
                ? "Isi formulir untuk memulai pendaftaran."
                : "Coba kata kunci lain atau kosongkan kolom pencarian.";
        }

        private void ShowValidation(string message)
        {
            ValidationText.Text = message;
            ValidationBanner.Visibility = Visibility.Visible;
            ValidationBanner.BringIntoView();
            StatusText.Text = "Pendaftaran belum disimpan. Periksa isian formulir.";
        }

        private void BirthDatePicker_DateValidationError(object sender, DatePickerDateValidationErrorEventArgs e)
        {
            e.ThrowException = false;
            _invalidDateText = true;
            // Do not silently keep a previous valid date after invalid text is entered.
            BirthDatePicker.SelectedDate = null;
            ShowValidation("Tanggal lahir tidak valid. Pilih tanggal melalui kalender.");
        }

        private void BirthDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BirthDatePicker.SelectedDate.HasValue)
                _invalidDateText = false;
        }
    }
}
