using System.Collections.Generic;
using System.IO;
using Ceres.Runtime.Modules;
using EVIL.CommonTypes.TypeSystem;
using NUnit.Framework;
using Shouldly;

namespace Ceres.RuntimeTests.RuntimeModules
{
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
                "fn test() -> fs.path.bad_name_chars;"
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
        public void PathCombine()
        {
            var result = EvilTestResult(
                "fn test() -> fs.path.cmb('c:', 'weird stuff', 'test');"
            );

            result.Type.ShouldBe(DynamicValueType.String);
            result.ShouldBe("c:/weird stuff/test");
        }
        
    }
}