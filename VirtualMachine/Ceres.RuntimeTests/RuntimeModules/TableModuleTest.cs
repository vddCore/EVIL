using Ceres.Runtime.Modules;
using NUnit.Framework;
using Shouldly;

namespace Ceres.RuntimeTests.RuntimeModules
{
    public class TableModuleTest : ModuleTest<TableModule>
    {
        [Test]
        public void Clear()
        {
            var t = EvilTestResult(
                "fn test() {" +
                "   var t = { 1, 2, 3, 4 };" +
                "   tbl.clear(t);" +
                "   ret t;" +
                "}"
            ).Table!;

            t.Length.ShouldBe(0);
        }
    }
}