using System.Reflection;
using System.Runtime.InteropServices;
using CommandLine;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Pascal compiler")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("pascal-compiler")]
[assembly: AssemblyCopyright("Copyright ©  2017 Polikutin Evgeny (FEFU, B8303a)")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("9ca92b28-5f31-449f-b12a-f7006e61c700")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: AssemblyLicense(
  "This is free software. You may redistribute copies of it under the terms of",
  "the MIT License <http://www.opensource.org/licenses/mit-license.php>.")]
[assembly: AssemblyUsage(
  "Usage: pascal-compiler -m mode -i input -o output")]