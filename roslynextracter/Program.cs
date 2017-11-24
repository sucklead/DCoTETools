using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using SevenZip;
using System.Diagnostics;
using Mischel.CabDotNet;

namespace roslynextracter
{
    class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            SevenZipBase.SetLibraryPath(@"F:\workspaces\vs\DCoTETools\roslynextracter\bin\Debug\7z.dll");

            Directory.CreateDirectory("roslyntemp");

            using (MemoryStream ms = new MemoryStream())
            {
                using (SevenZipExtractor extractor = new SevenZipExtractor("RoslynSetup.exe"))
                {
                    extractor.ExtractFile("roslyn.msi", ms);
                    File.WriteAllBytes("roslyn.msi", ms.ToArray());
                }
            }

            string roslyntemp = Path.Combine(Directory.GetCurrentDirectory(), "roslyntemp");

            string parameters = string.Empty;
            parameters = string.Format(@"/a {0} /qb TARGETDIR=""{1}"" REINSTALLMODE=amus", "roslyn.msi", roslyntemp);
            Process process = Process.Start("msiexec", parameters);
            process.WaitForExit();

            File.Delete("roslyn.msi");

            File.Copy(Path.Combine(roslyntemp, @"Reference Assemblies\Microsoft\Roslyn\v1.0\Roslyn.Compilers.CSharp.dll"), "Roslyn.Compilers.CSharp.dll");
            File.Copy(Path.Combine(roslyntemp, @"Reference Assemblies\Microsoft\Roslyn\v1.0\Roslyn.Compilers.dll"), "Roslyn.Compilers.dll");

            Directory.Delete("roslyntemp", true);

        }

    }
}
