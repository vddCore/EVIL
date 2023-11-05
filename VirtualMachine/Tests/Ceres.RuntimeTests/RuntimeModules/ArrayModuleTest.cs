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
                "    arr: values, /* remember tables and arrays are passed by ref */" +
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

        [Test]
        public void Push()
        {
            var result = EvilTestResult(
                "fn test() {" +
                "  val values = array() { 2, 1, 3, 7 };" +
                "  arr.push(values, 0, 0, 0, 0);" +
                "" +
                "  ret values;" +
                "}"
            );

            result.Type.ShouldBe(DynamicValueType.Array);
            var arr = result.Array!;

            arr.Length.ShouldBe(8);
            
            arr[0].ShouldBe(2);
            arr[1].ShouldBe(1);
            arr[2].ShouldBe(3);
            arr[3].ShouldBe(7);
            arr[4].ShouldBe(0);
            arr[5].ShouldBe(0);
            arr[6].ShouldBe(0);
            arr[7].ShouldBe(0);
        }

        [Test]
        public void InsertFront()
        {
            var result = EvilTestResult(
                "fn test() {" +
                "  val values = array() { 2, 1, 3, 7 };" +
                "  arr.insert(values, 0, 0, 0, 0, 0);" +
                "" +
                "  ret values;" +
                "}"
            );

            result.Type.ShouldBe(DynamicValueType.Array);
            var arr = result.Array!;

            arr.Length.ShouldBe(8);
            
            arr[0].ShouldBe(0);
            arr[1].ShouldBe(0);
            arr[2].ShouldBe(0);
            arr[3].ShouldBe(0);
            arr[4].ShouldBe(2);
            arr[5].ShouldBe(1);
            arr[6].ShouldBe(3);
            arr[7].ShouldBe(7);
        }
        
        [Test]
        public void InsertMiddle()
        {
            var result = EvilTestResult(
                "fn test() {" +
                "  val values = array() { 2, 1, 3, 7 };" +
                "  arr.insert(values, 2, 0, 0, 0, 0);" +
                "" +
                "  ret values;" +
                "}"
            );

            result.Type.ShouldBe(DynamicValueType.Array);
            var arr = result.Array!;

            arr.Length.ShouldBe(8);
            
            arr[0].ShouldBe(2);
            arr[1].ShouldBe(1);
            arr[2].ShouldBe(0);
            arr[3].ShouldBe(0);
            arr[4].ShouldBe(0);
            arr[5].ShouldBe(0);
            arr[6].ShouldBe(3);
            arr[7].ShouldBe(7);
        }
        
        [Test]
        public void InsertBack()
        {
            var result = EvilTestResult(
                "fn test() {" +
                "  val values = array() { 2, 1, 3, 7 };" +
                "  arr.insert(values, #values, 0, 0, 0, 0);" +
                "" +
                "  ret values;" +
                "}"
            );

            result.Type.ShouldBe(DynamicValueType.Array);
            var arr = result.Array!;

            arr.Length.ShouldBe(8);
            
            arr[0].ShouldBe(2);
            arr[1].ShouldBe(1);
            arr[2].ShouldBe(3);
            arr[3].ShouldBe(7);
            arr[4].ShouldBe(0);
            arr[5].ShouldBe(0);
            arr[6].ShouldBe(0);
            arr[7].ShouldBe(0);
        }

        [Test]
        public void RightShift()
        {
            var result = EvilTestResult(
                "fn test() {" +
                "  val values = array() { 2, 1, 3, 7 };" +
                "" +
                "  ret {" +
                "    shifted: array() {" +
                "      arr.rsh(values)," +
                "      arr.rsh(values)" +
                "    }," +
                "    values: values" +
                "  };" +
                "}"
            );
            
            result.Type.ShouldBe(DynamicValueType.Table);
            
            var tbl = result.Table!;
            tbl["shifted"].Type.ShouldBe(DynamicValueType.Array);
            
            var shiftedValues = tbl["shifted"].Array!;
            shiftedValues.Length.ShouldBe(2);
            shiftedValues[0].ShouldBe(7);
            shiftedValues[1].ShouldBe(3);

            tbl["values"].Type.ShouldBe(DynamicValueType.Array);
            var remainingValues = tbl["values"].Array!;
            remainingValues.Length.ShouldBe(2);
            remainingValues[0].ShouldBe(2);
            remainingValues[1].ShouldBe(1);
        }
        
        [Test]
        public void LeftShift()
        {
            var result = EvilTestResult(
                "fn test() {" +
                "  val values = array() { 2, 1, 3, 7 };" +
                "" +
                "  ret {" +
                "    shifted: array() {" +
                "      arr.lsh(values)," +
                "      arr.lsh(values)" +
                "    }," +
                "    values: values" +
                "  };" +
                "}"
            );
            
            result.Type.ShouldBe(DynamicValueType.Table);
            
            var tbl = result.Table!;
            tbl["shifted"].Type.ShouldBe(DynamicValueType.Array);
            
            var shiftedValues = tbl["shifted"].Array!;
            shiftedValues.Length.ShouldBe(2);
            shiftedValues[0].ShouldBe(2);
            shiftedValues[1].ShouldBe(1);

            tbl["values"].Type.ShouldBe(DynamicValueType.Array);
            var remainingValues = tbl["values"].Array!;
            remainingValues.Length.ShouldBe(2);
            remainingValues[0].ShouldBe(3);
            remainingValues[1].ShouldBe(7);
        }
    }
}