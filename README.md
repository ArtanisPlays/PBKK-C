# PBKK C — 2026

Assignment folders for PBKK C:

1. `2026-09-10-pertemuan-2-dotnet-console` — C#/.NET console Hello World and student CRUD, plus a Windows Forms student manager based on the linked PBKK C documentation. Blogger draft: `Penjelasan-Blogger.html`.
2. `2026-09-17-pertemuan-3-scientific-calculator` — Windows Forms scientific calculator. Blogger draft: `Penjelasan-Blogger.html`.
3. [`pertemuan-4-wpf-student-registration`](pertemuan-4-wpf-student-registration/README.md) — WPF student registration app using **.NET Framework 4.8**. Open `PBKKC.Pertemuan4.sln` in Visual Studio. Indonesian explanation: `Penjelasan-Blogger.html`.

The class report showed Pertemuan 2 on September 10 and Pertemuan 3 on September 17. Their original lecturer brief was not present in the dashboard, so those drafts identify reconstructed requirements. Pertemuan 4 follows the requested WPF student registration assignment; its date was not provided.

The previous project files were checked: Pertemuan 2 targets `net10.0` (console) and `net10.0-windows` (Windows Forms); Pertemuan 3 targets `net10.0-windows`. They use modern **.NET 10**, rather than .NET Framework. Pertemuan 4 targets `net48`, which is **.NET Framework 4.8**. The earlier projects retain their original targets.

Pertemuan 4 was built in Release mode with a temporary .NET SDK, using NuGet reference assemblies for .NET Framework 4.8. See its README for verified behavior and setup instructions. Compilation of the earlier assignments has not been verified.
