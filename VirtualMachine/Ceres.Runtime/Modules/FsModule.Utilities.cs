using System;
using System.IO;
using System.Linq;

namespace Ceres.Runtime.Modules
{
    public partial class FsModule
    {
        private static void CopyDirectoryRecursively(DirectoryInfo source, DirectoryInfo target, bool overwriteExisting)
        {
            Directory.CreateDirectory(target.FullName);

            foreach (var fileInfo in source.GetFiles())
            {
                fileInfo.CopyTo(
                    Path.Combine(target.FullName, fileInfo.Name), 
                    overwriteExisting
                );
            }

            foreach (var sourceSubDir in source.GetDirectories())
            {
                var nextTargetSubDir = target.CreateSubdirectory(sourceSubDir.Name);
                CopyDirectoryRecursively(sourceSubDir, nextTargetSubDir, overwriteExisting);
            }
        }

        private static (FileAccess Access, FileMode FileMode) AccessAndModeFromString(string modeString)
        {
            var (access, fileMode) = ((FileAccess)0, (FileMode)0);
            var hasPlus = modeString.Contains('+');
            modeString = modeString.Replace("+", "");
            
            (access, fileMode) = modeString switch
            {
                "r" when hasPlus => (FileAccess.ReadWrite, FileMode.Open),
                "r" => (FileAccess.Read, FileMode.Open),
                
                "w" when hasPlus => (FileAccess.ReadWrite, FileMode.Create),
                "w" => (FileAccess.Write, FileMode.Create),
                
                "a" when hasPlus => (FileAccess.ReadWrite, FileMode.OpenOrCreate),
                "a" => (FileAccess.Write, FileMode.Append),
                
                "t" => (FileAccess.Write, FileMode.Truncate),
                "rw" => (FileAccess.ReadWrite, FileMode.OpenOrCreate),
                "wr" => (FileAccess.ReadWrite, FileMode.OpenOrCreate),

                _ => throw new ArgumentException($"Invalid file mode '{modeString}'.")
            };

            return (access, fileMode);
        }
    }
}