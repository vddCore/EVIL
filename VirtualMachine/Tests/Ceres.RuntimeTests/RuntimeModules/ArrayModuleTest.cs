using Ceres.Runtime.Modules;
using EVIL.CommonTypes.TypeSystem;
using NUnit.Framework;
using Shouldly;

namespace Ceres.RuntimeTests.RuntimeModules
{
    public class ArrayModuleTest : ModuleTest<ArrayModule>
    {
        [Test]
        public void IndexOf()
        {
            var result = EvilTestResult(
                "fn test() {" +
                "  val values = array() { 1, 2, 3, 'a string', false };" +
                "  ret array() {" +
                "    arr.indof(values, 'a string')," +
                "    arr.indof(values, 3)," +
                "    arr.indof(values, false)" +
                "  };" +
                "}"
            );

            result.Type.ShouldBe(DynamicValueType.Array);

            var arr = result.Array!;
            arr[0].ShouldBe(3);
            arr[1].ShouldBe(2);
            arr[2].ShouldBe(4);
        }

        [Test]
        public void Fill()
        {
            var result = EvilTestResult(
                "fn test() {" +
                "  val values = array(5);" +
                "  arr.fill(values, 'a string');" +
                "  ret values;" +
                "}"
            );

            result.Type.ShouldBe(DynamicValueType.Array);

            var arr = result.Array!;
            arr[0].ShouldBe("a string");
            arr[1].ShouldBe("a string");
            arr[2].ShouldBe("a string");
            arr[3].ShouldBe("a string");
            arr[4].ShouldBe("a string");
        }
    }
}