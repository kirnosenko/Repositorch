using System.Reflection;

[assembly: AssemblyVersion(
    ThisAssembly.Git.BaseVersion.Major + "." + 
    ThisAssembly.Git.BaseVersion.Minor + "." + 
    ThisAssembly.Git.BaseVersion.Patch)]
[assembly: AssemblyFileVersion(
    ThisAssembly.Git.BaseVersion.Major + "." +
    ThisAssembly.Git.BaseVersion.Minor + "." +
    ThisAssembly.Git.BaseVersion.Patch)]
[assembly: AssemblyInformationalVersion(
  ThisAssembly.Git.BaseVersion.Major + "." +
  ThisAssembly.Git.BaseVersion.Minor + "." +
  ThisAssembly.Git.BaseVersion.Patch + "-" +
  ThisAssembly.Git.Commit + " alpha")]
[assembly: AssemblyCompany("Semyon Kirnosenko")]
[assembly: AssemblyCopyright("Semyon Kirnosenko 2019-2021")]
[assembly: AssemblyTitle("Repositorch")]
[assembly: AssemblyDescription("Repositorch is a VCS repository analysis engine written in C#.")]
