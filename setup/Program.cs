using System;
using System.IO;
using System.Linq;
using WixSharp;

namespace Setup
{
    internal class Program
    {
        static void Main()
        {
            try
            {
                // Attempt to locate WiX binaries in the NuGet cache
                var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var wixBinPath = Path.Combine(userProfile, @".nuget\packages\wixsharp.bin\1.26.0\tools\bin");
                
                // Fallback for different package structures or if tools/bin is different
                if (!Directory.Exists(wixBinPath))
                {
                     // Try another common location if the above fails
                     wixBinPath = Path.Combine(userProfile, @".nuget\packages\wixsharp.bin\1.26.0\lib\bin");
                }

                if (Directory.Exists(wixBinPath))
                {
                    Compiler.WixLocation = wixBinPath;
                    Console.WriteLine($"Found WiX binaries at: {wixBinPath}");
                }
                else
                {
                    Console.WriteLine("!!! WARNING: Could not locate WiX binaries in NuGet cache. Build might fail.");
                    Console.WriteLine($"Checked: {wixBinPath}");
                }

                var projectRoot = FindProjectRoot();
                // Point to the publish directory of sfixer
                var sourceDir = Path.Combine(projectRoot, @"sfixer\bin\Release\net10.0-windows\win-x64\publish");

                Console.WriteLine($"Building MSI for artifacts in: {sourceDir}");

                if (!Directory.Exists(sourceDir))
                {
                    Console.WriteLine("!!! ERROR: Publish directory not found.");
                    Console.WriteLine("Ensure you have run: dotnet publish -c Release -r win-x64");
                    return;
                }

                var project = new Project("sFixer",
                    new Dir(@"%ProgramFiles%\sFixer",
                        new Files(Path.Combine(sourceDir, "*.*"))
                    )
                );

                project.GUID = new Guid("6FE30B47-2577-43AD-9095-1861BA25889B");
                project.Version = new Version("1.0.0.0");
                project.OutFileName = "sFixerSetup";
                
                // Ensure we are generating x64 MSI since we are targeting win-x64
                project.Platform = Platform.x64;

                // Add Desktop Shortcut
                // We use ResolveWildCards to find the exe within the harvested files
                project.ResolveWildCards().FindFile(f => f.Name.EndsWith("sfixer.exe"))
                    .First()
                    .Shortcuts = new[] {
                        new FileShortcut("sFixer", "%Desktop%")
                    };

                // Build the MSI
                project.BuildMsi();
            }
            catch (Exception ex)
            {
                Console.WriteLine("CRITICAL ERROR: " + ex.ToString());
                throw;
            }
        }

        static string FindProjectRoot()
        {
            var dir = Directory.GetCurrentDirectory();
            // Walk up to find the .slnx or root marker
            while (dir != null)
            {
                if (System.IO.File.Exists(Path.Combine(dir, "sfixer.slnx")))
                    return dir;
                dir = Path.GetDirectoryName(dir);
            }
            return Directory.GetCurrentDirectory(); 
        }
    }
}