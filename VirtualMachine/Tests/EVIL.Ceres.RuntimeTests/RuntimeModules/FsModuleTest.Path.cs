namespace EVIL.Ceres.RuntimeTests.RuntimeModules;

using System.IO;
using EVIL.CommonTypes.TypeSystem;
using NUnit.Framework;
using Shouldly;

public partial class FsModuleTest
{
    [Test]
    public void PathGetPathSeparator()
    {
        var result = EvilTestResult(
            "fn test() -> fs.path.sep;"
        );

        result.Type.ShouldBe(DynamicValueType.String);
        result.ShouldBe(Path.PathSeparator.ToString());
    }

    [Test]
    public void PathGetAltPathSeparator()
    {
        var result = EvilTestResult(
            "fn test() -> fs.path.alt_sep;"
        );

        result.Type.ShouldBe(DynamicValueType.String);
        result.ShouldBe(Path.AltDirectorySeparatorChar.ToString());
    }

    [Test]
    public void PathGetBadPathChars()
    {
        var result = EvilTestResult(
            "fn test() -> fs.path.bad_path_chars;"
        );

        result.Type.ShouldBe(DynamicValueType.Array);
            
        var chars = Path.GetInvalidPathChars();
        var array = result.Array!;
        for (var i = 0; i < array.Length; i++)
        {
            array[i].Type.ShouldBe(DynamicValueType.String);
            array[i].String.ShouldBe(chars[i].ToString());
        }
    }
        
    [Test]
    public void PathGetBadNameChars()
    {
        var result = EvilTestResult(
            "fn test() -> fs.path.bad_fname_chars;"
        );

        result.Type.ShouldBe(DynamicValueType.Array);
            
        var chars = Path.GetInvalidFileNameChars();
        var array = result.Array!;
        for (var i = 0; i < array.Length; i++)
        {
            array[i].Type.ShouldBe(DynamicValueType.String);
            array[i].String.ShouldBe(chars[i].ToString());
        }
    }
        
    [Test]
    public void PathGetTempDirPath()
    {
        var tempDir = Path.GetTempPath();
        var result = EvilTestResult(
            "fn test() -> fs.path.temp_dir;"
        );
            
        result.Type.ShouldBe(DynamicValueType.String);
        result.ShouldBe(tempDir);
    }
        
    [Test]
    public void PathGetRandomFileName()
    {
        var result = EvilTestResult(
            $"fn test() -> fs.path.rand_fname;"
        );
            
        result.Type.ShouldBe(DynamicValueType.String);
        string.IsNullOrEmpty(result.String!).ShouldBe(false);
    }
        
    [Test]
    public void PathCombine()
    {
        var result = EvilTestResult(
            "fn test() -> fs.path.cmb('c:', 'weird stuff', 'test');"
        );

        result.Type.ShouldBe(DynamicValueType.String);
        result.ShouldBe("c:/weird stuff/test");
    }

    [Test]
    public void PathGetFileNameWithExtension()
    {
        var result = EvilTestResult(
            $"fn test() -> fs.path.get_fname('/var/lib/something.so.1');"
        );
            
        result.Type.ShouldBe(DynamicValueType.String);
        result.ShouldBe("something.so.1");
    }
        
    [Test]
    public void PathGetFileNameWithoutExtension()
    {
        var result = EvilTestResult(
            $"fn test() -> fs.path.get_fname(" +
            $"  '/var/lib/something.so.1'," +
            $"  true" +
            $");"
        );
            
        result.Type.ShouldBe(DynamicValueType.String);
        result.ShouldBe("something.so");
    }
        
    [Test]
    public void PathGetDirectoryName()
    {
        var result = EvilTestResult(
            $"fn test() -> fs.path.get_dname(" +
            $"  'c:/program files (x86)/btw/buy/deadlink.exe'" +
            $");"
        );
            
        result.Type.ShouldBe(DynamicValueType.String);
        result.ShouldBe("c:/program files (x86)/btw/buy");
    }

    [Test]
    public void PathExists()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            Path.GetTempFileName()
        ).Replace('\\', '/');
            
        File.Create(path).Dispose();

        var resultFile = EvilTestResult(
            $"fn test() -> fs.path.exists('{path}');"
        );
        resultFile.Type.ShouldBe(DynamicValueType.Boolean);
        resultFile.ShouldBe(true);
        File.Delete(path);

        Directory.CreateDirectory(path);

        var resultDirectory = EvilTestResult(
            $"fn test() -> fs.path.exists('{path}');"
        );
        resultDirectory.Type.ShouldBe(DynamicValueType.Boolean);
        resultDirectory.ShouldBe(true);
        Directory.Delete(path, true);
    }

    [Test]
    public void PathGetExtension()
    {
        var result = EvilTestResult(
            $"fn test() -> fs.path.get_ext('dobry_jezu.exe');"
        );

        result.Type.ShouldBe(DynamicValueType.String);
        result.ShouldBe(".exe");
    }
        
    [Test]
    public void PathHasExtension()
    {
        var result = EvilTestResult(
            $"fn test() -> fs.path.has_ext('dobry_jezu.exe');"
        );

        result.Type.ShouldBe(DynamicValueType.Boolean);
        result.ShouldBe(true);
            
        result = EvilTestResult(
            $"fn test() -> fs.path.has_ext('dobry_jezu');"
        );
            
        result.Type.ShouldBe(DynamicValueType.Boolean);
        result.ShouldBe(false);
    }

    [Test]
    public void PathChangeExtension()
    {
        var result = EvilTestResult(
            $"fn test() -> fs.path.chg_ext(" +
            $"  '/root/dobry_jezu.exe'," +
            $"  '.png'" +
            $");"
        );

        result.Type.ShouldBe(DynamicValueType.String);
        result.ShouldBe("/root/dobry_jezu.png");
    }
        
    [Test]
    public void PathRemoveExtension()
    {
        var result = EvilTestResult(
            $"fn test() -> fs.path.chg_ext(" +
            $"  '/root/dobry_jezu.exe'," +
            $"  nil" +
            $");"
        );

        result.Type.ShouldBe(DynamicValueType.String);
        result.ShouldBe("/root/dobry_jezu");
    }
}