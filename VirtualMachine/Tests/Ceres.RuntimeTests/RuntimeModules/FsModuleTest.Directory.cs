using System.Collections.Generic;
using System.IO;
using Ceres.Runtime.Modules;
using EVIL.CommonTypes.TypeSystem;
using NUnit.Framework;
using Shouldly;

namespace Ceres.RuntimeTests.RuntimeModules
{
    public partial class FsModuleTest : ModuleTest<FsModule>
    {
        [Test]
        public void DirectoryExists()
        {
            var info = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), "evil_test_temp_dir")
            );

            var result = EvilTestResult(
                $"fn test() -> fs.dir.exists('{info.FullName.Replace('\\', '/')}');"
            );

            Directory.Delete(info.FullName);
            result.ShouldBe(true);
        }

        [Test]
        public void DirectoryCreate()
        {
            var path = Path.Combine(
                Path.GetTempPath(),
                "evil_test_temp_dir"
            ).Replace('\\', '/');

            var result = EvilTestResult(
                $"fn test() -> fs.dir.create('{path}');"
            );

            result.Type.ShouldBe(DynamicValueType.Table);
            var tbl = result.Table!;

            Directory.Exists(tbl["path"].String!).ShouldBe(true);
            Directory.Delete(path);
        }

        [Test]
        public void DirectoryDelete()
        {
            var path = Path.Combine(
                Path.GetTempPath(),
                "evil_test_temp_dir"
            ).Replace('\\', '/');

            Directory.CreateDirectory(path);

            var result = EvilTestResult(
                $"fn test() -> fs.dir.delete('{path}');"
            );

            result.ShouldBe(true);
            Directory.Exists(path).ShouldBe(false);
        }

        [Test]
        public void DirectoryGetFiles()
        {
            var dirPath = Path.Combine(
                Path.GetTempPath(),
                "evil_test_temp_dir"
            ).Replace('\\', '/');

            try
            {
                Directory.Delete(dirPath, true);
            }
            catch
            {
                /* Ignore */
            }

            Directory.CreateDirectory(dirPath);

            var fileNames = new List<string>();
            for (var i = 0; i < 10; i++)
            {
                var name = Path.GetRandomFileName();
                var filePath = Path.Combine(dirPath, name).Replace('\\', '/');

                fileNames.Add(filePath);
                File.Create(filePath).Dispose();
            }

            var value = EvilTestResult(
                $"fn test() -> fs.dir.get_files('{dirPath}');"
            );

            value.Type.ShouldBe(DynamicValueType.Array);
            var array = value.Array!;
            array.Length.ShouldBe(10);

            for (var i = 0; i < array.Length; i++)
            {
                array[i].Type.ShouldBe(DynamicValueType.String);
                fileNames.ShouldContain(array[i].String);
            }

            Directory.Delete(dirPath, true);
        }

        [Test]
        public void DirectoryGetDirectories()
        {
            var dirPath = Path.Combine(
                Path.GetTempPath(),
                "evil_test_temp_dir"
            ).Replace('\\', '/');

            try
            {
                Directory.Delete(dirPath, true);
            }
            catch
            {
                /* Ignore */
            }

            Directory.CreateDirectory(dirPath);

            var dirNames = new List<string>();
            for (var i = 0; i < 10; i++)
            {
                var name = Path.GetRandomFileName();
                var subDirPath = Path.Combine(dirPath, name).Replace('\\', '/');

                dirNames.Add(subDirPath);
                Directory.CreateDirectory(subDirPath);
            }

            var value = EvilTestResult(
                $"fn test() -> fs.dir.get_dirs('{dirPath}');"
            );

            value.Type.ShouldBe(DynamicValueType.Array);
            var array = value.Array!;
            array.Length.ShouldBe(10);

            for (var i = 0; i < array.Length; i++)
            {
                array[i].Type.ShouldBe(DynamicValueType.String);
                dirNames.ShouldContain(array[i].String);
            }

            Directory.Delete(dirPath, true);
        }

        [Test]
        public void DirectoryCopy()
        {
            var dirPath = Path.Combine(
                Path.GetTempPath(),
                "evil_test_temp_dir"
            ).Replace('\\', '/');

            var dirPath2 = Path.Combine(
                Path.GetTempPath(),
                "evil_test_temp_dir2"
            ).Replace('\\', '/');

            try
            {
                Directory.Delete(dirPath, true);
            }
            catch
            {
                /* Ignore */
            }
            
            try
            {
                Directory.Delete(dirPath2, true);
            }
            catch
            {
                /* Ignore */
            }

            Directory.CreateDirectory(dirPath);

            var fileNames = new List<string>();
            for (var i = 0; i < 10; i++)
            {
                var name = Path.GetRandomFileName();
                var filePath = Path.Combine(dirPath, name).Replace('\\', '/');

                fileNames.Add(filePath);
                File.Create(filePath).Dispose();
            }

            var value = EvilTestResult(
                $"fn test() -> fs.dir.copy(" +
                $"  '{dirPath}'," +
                $"  '{dirPath2}'" +
                $");"
            );

            value.Type.ShouldBe(DynamicValueType.Boolean);
            value.ShouldBe(true);

            var containsAllFiles = true;
            for (var i = 0; i < fileNames.Count; i++)
            {
                containsAllFiles &= File.Exists(
                    Path.Combine(dirPath2, fileNames[i]
                    )
                );
            }

            containsAllFiles.ShouldBe(true);

            Directory.Delete(dirPath, true);
            Directory.Delete(dirPath2, true);
        }
        
        [Test]
        public void DirectoryMove()
        {
            var dirPath = Path.Combine(
                Path.GetTempPath(),
                "evil_test_temp_dir"
            ).Replace('\\', '/');

            var dirPath2 = Path.Combine(
                Path.GetTempPath(),
                "evil_test_temp_dir2"
            ).Replace('\\', '/');

            try
            {
                Directory.Delete(dirPath, true);
            }
            catch
            {
                /* Ignore */
            }

            try
            {
                Directory.Delete(dirPath2, true);
            }
            catch
            {
                /* Ignore */
            }

            Directory.CreateDirectory(dirPath);

            var fileNames = new List<string>();
            for (var i = 0; i < 10; i++)
            {
                var name = Path.GetRandomFileName();
                var filePath = Path.Combine(dirPath, name).Replace('\\', '/');

                fileNames.Add(filePath);
                File.Create(filePath).Dispose();
            }

            var value = EvilTestResult(
                $"fn test() -> fs.dir.move(" +
                $"  '{dirPath}'," +
                $"  '{dirPath2}'" +
                $");"
            );

            value.Type.ShouldBe(DynamicValueType.Boolean);
            value.ShouldBe(true);

            Directory.Exists(dirPath).ShouldBe(false);

            var containsAllFiles = true;
            for (var i = 0; i < fileNames.Count; i++)
            {
                containsAllFiles &= File.Exists(
                    Path.Combine(dirPath2, Path.GetFileName(fileNames[i])!)
                );
            }

            containsAllFiles.ShouldBe(true);
            Directory.Delete(dirPath2, true);
        }
    }
}