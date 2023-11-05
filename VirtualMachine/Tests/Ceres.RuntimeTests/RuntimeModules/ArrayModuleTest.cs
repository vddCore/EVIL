using Ceres.Runtime.Modules;
using EVIL.CommonTypes.TypeSystem;
using NUnit.Framework;
using Shouldly;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

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

        [Test]
        public void Resize()
        {
            var result = EvilTestResult(
                "fn test() {" +
                "  val values = array() { 21.37 };" +
                "  ret {" +
                "    arr: values, // remember tables and arrays are passed by ref\n" +
                "    pre_resize_sz: #values," +
                "    resize_ret: arr.resize(values, 5)," +
                "    post_resize_sz: #values" +
                "  };" +
                "}"
            );

            result.Type.ShouldBe(DynamicValueType.Table);
            var table = result.Table!;

            table["pre_resize_sz"].ShouldBe(1);
            table["resize_ret"].ShouldBe(5);
            table["post_resize_sz"].ShouldBe(5);
            
            table["arr"].Type.ShouldBe(DynamicValueType.Array);
            var array = table["arr"].Array!;

            array[0].ShouldBe(21.37);
            array[1].ShouldBe(Nil);
            array[2].ShouldBe(Nil);
            array[3].ShouldBe(Nil);
            array[4].ShouldBe(Nil);
        }
    }
}