using System.IO;

namespace Ceres.Runtime.Modules
{
    public partial class FsModule
    {
        private static void CopyDirectoryRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            foreach (var fileInfo in source.GetFiles())
            {
                fileInfo.CopyTo(
                    Path.Combine(target.FullName, fileInfo.Name), 
                    true
                );
            }

            foreach (var sourceSubDir in source.GetDirectories())
            {
                var nextTargetSubDir = target.CreateSubdirectory(sourceSubDir.Name);
                CopyDirectoryRecursively(sourceSubDir, nextTargetSubDir);
            }
        }
    }
}