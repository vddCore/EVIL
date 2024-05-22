using System.IO;
using EVIL.Ceres.Runtime.Modules;
using EVIL.CommonTypes.TypeSystem;
using NUnit.Framework;
using Shouldly;

namespace EVIL.Ceres.RuntimeTests.RuntimeModules
{
    public partial class FsModuleTest : ModuleTest<FsModule>
    {
        [Test]
        public void OriginValuesMatch()
        {
            var result = EvilTestResult(
                $"fn test() -> array() {{ " +
                $"  fs.origin.BEGIN, " +
                $"  fs.origin.CURRENT, " +
                $"  fs.origin.END " +
                $"}};"
            );

            result.Type.ShouldBe(DynamicValueType.Array);
            var array = result.Array!;

            array[0].Type.ShouldBe(DynamicValueType.Number);
            array[1].Type.ShouldBe(DynamicValueType.Number);
            array[2].Type.ShouldBe(DynamicValueType.Number);

            array[0].ShouldBe((long)SeekOrigin.Begin);
            array[1].ShouldBe((long)SeekOrigin.Current);
            array[2].ShouldBe((long)SeekOrigin.End);
        }
    }
}