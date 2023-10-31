using System.IO;
using EVIL.CommonTypes.TypeSystem;
using NUnit.Framework;
using Shouldly;

namespace Ceres.RuntimeTests.RuntimeModules
{
    public partial class FsModuleTest
    {
        [Test]
        public void FileExists()
        {
            var tmpPath = Path.Combine(
                Path.GetTempPath(),
                Path.GetRandomFileName()
            ).Replace('\\', '/');
            
            File.Create(tmpPath).Dispose();

            var result = EvilTestResult(
                $"fn test() -> fs.file.exists('{tmpPath}');"
            );
            
            File.Delete(tmpPath);
            
            result.Type.ShouldBe(DynamicValueType.Boolean);
            result.ShouldBe(true);
        }

        [Test]
        public void FileDelete()
        {
            var tmpPath = Path.Combine(
                Path.GetTempPath(),
                Path.GetRandomFileName()
            ).Replace('\\', '/');
            
            File.Create(tmpPath).Dispose();

            var result = EvilTestResult(
                $"fn test() -> fs.file.delete('{tmpPath}');"
            );
            
            result.Type.ShouldBe(DynamicValueType.Boolean);
            result.ShouldBe(true);
            File.Exists(tmpPath).ShouldBe(false);
        }
        
        [Test]
        public void FileCopy()
        {
            var tmpPath = Path.Combine(
                Path.GetTempPath(),
                Path.GetRandomFileName()
            ).Replace('\\', '/');

            var tmpPath2 = tmpPath + "2";
            
            File.Create(tmpPath).Dispose();

            var result = EvilTestResult(
                $"fn test() -> fs.file.copy(" +
                $"  '{tmpPath}'," +
                $"  '{tmpPath2}'" +
                $");"
            );
            
            result.Type.ShouldBe(DynamicValueType.Boolean);
            result.ShouldBe(true);
            
            File.Exists(tmpPath).ShouldBe(true);
            File.Exists(tmpPath2).ShouldBe(true);

            File.Delete(tmpPath);
            File.Delete(tmpPath2);
        }
        
        [Test]
        public void FileMove()
        {
            var tmpPath = Path.Combine(
                Path.GetTempPath(),
                Path.GetRandomFileName()
            ).Replace('\\', '/');

            var tmpPath2 = tmpPath + "2";
            
            File.Create(tmpPath).Dispose();

            var result = EvilTestResult(
                $"fn test() -> fs.file.move(" +
                $"  '{tmpPath}'," +
                $"  '{tmpPath2}'" +
                $");"
            );
            
            result.Type.ShouldBe(DynamicValueType.Boolean);
            result.ShouldBe(true);
            
            File.Exists(tmpPath).ShouldBe(false);
            File.Exists(tmpPath2).ShouldBe(true);

            File.Delete(tmpPath2);
        }

        [Test]
        public void FileLines()
        {
            var tmpPath = Path.Combine(
                Path.GetTempPath(),
                Path.GetRandomFileName()
            ).Replace('\\', '/');

            using (var sw = new StreamWriter(tmpPath))
            {
                sw.WriteLine("line 1");
                sw.WriteLine("line 2");
                sw.WriteLine("line 3");
            }

            var result = EvilTestResult(
                $"fn test() -> fs.file.get_lines('{tmpPath}');"
            );

            result.Type.ShouldBe(DynamicValueType.Array);
            
            var array = result.Array!;
            array.Length.ShouldBe(3);
            
            for (var i = 0; i < array.Length; i++)
            {
                array[i].ShouldBe($"line {i+1}");
            }
            
            File.Delete(tmpPath);
        }
    }
}