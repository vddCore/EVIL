using Ceres.Runtime.Modules;
using NUnit.Framework;
using Shouldly;

namespace Ceres.RuntimeTests.RuntimeModules
{
    public class CoreModuleTest : ModuleTest<CoreModule>
    {
        [Test]
        public void StackTrace()
        {
            var t = EvilTestResult(
                "fn test(a = 1, b = 2, c = 3) -> core.strace(true);"
            ).Table!;
            var frame = t[0].Table!;

            frame["is_script"].ShouldBe(true);
            frame["fn_name"].ShouldBe("test");
            frame["def_on_line"].ShouldBe(1);

            var args = frame["args"].Table!;
            args[0].ShouldBe(1);
            args[1].ShouldBe(2);
            args[2].ShouldBe(3);
        }

        [Test]
        public void StackTraceString()
        {
            var s = EvilTestResult(
                "fn nested_2() {\n" +
                "   ret core.strace_s();\n" +
                "}\n" +
                "" +
                "fn nested_1() {\n" +
                "   ret nested_2();\n" +
                "}\n" +
                "" +
                "fn nested_0() {\n" +
                "   ret nested_1();\n" +
                "}\n" +
                "" +
                "fn test() {\n" +
                "   var test = 20;\n" +
                "   var test2 = 21;\n" +
                "\n" +
                "   ret nested_0();\n" +
                "}\n"
            ).String!;

            s.ShouldContain(
                "at Ceres.Runtime.Modules.CoreModule::StackTraceString",
                Case.Sensitive
            );

            s.ShouldContain(
                "at nested_2 in !module_test_file!: line 2",
                Case.Sensitive
            );

            s.ShouldContain(
                "at nested_1 in !module_test_file!: line 5",
                Case.Sensitive
            );

            s.ShouldContain(
                "at nested_0 in !module_test_file!: line 8",
                Case.Sensitive
            );

            s.ShouldContain(
                "at test in !module_test_file!: line 14",
                Case.Sensitive
            );
        }
    }
}