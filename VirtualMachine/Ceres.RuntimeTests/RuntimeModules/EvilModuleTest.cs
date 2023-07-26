using Ceres.ExecutionEngine.Collections;
using Ceres.Runtime.Modules;
using NUnit.Framework;
using Shouldly;

namespace Ceres.RuntimeTests.RuntimeModules
{
    public class EvilModuleTest : ModuleTest<EvilModule>
    {
        [Test]
        public void Compile()
        {
            var t = EvilTestResult(
                "fn test() {" +
                "   var result = evil.compile('fn test() {" +
                "       ret \"i was compiled from within evil :dobry_jezu:\";" +
                "   }');" +
                "" +
                "   ret {" +
                "       \"script_table\" => result," +
                "       \"test_result\" => result.script.chunks[\"test\"]()" +
                "   };" +
                "}"
            ).Table!;

            var scriptTable = (Table)t["script_table"];
            var testResult = (string)t["test_result"];

            scriptTable["success"].ShouldBe(true);
            testResult.ShouldBe("i was compiled from within evil :dobry_jezu:");
        }
    }
}