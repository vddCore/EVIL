namespace EVIL.Ceres.RuntimeTests.RuntimeModules;

using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.Runtime.Modules;
using NUnit.Framework;
using Shouldly;

public class DebugModuleTest : ModuleTest<DebugModule>
{
    [Test]
    public void StackTrace()
    {
        var t = EvilTestResult(
            "fn test(a = 1, b = 2, c = 3) -> debug.strace(true);"
        ).Array!;
        var frame = (Table)t[0];

        frame["is_script"].ShouldBe(true);
        frame["fn_name"].ShouldBe("test");
        frame["def_on_line"].ShouldBe(1);

        var args = (Table)frame["args"];
        args[0].ShouldBe(1);
        args[1].ShouldBe(2);
        args[2].ShouldBe(3);
    }

    [Test]
    public void StackTraceString()
    {
        var s = (string)EvilTestResult(
            "fn nested_2() {\n" +
            "   ret debug.strace_s();\n" +
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
            "   val test = 20;\n" +
            "   val test2 = 21;\n" +
            "\n" +
            "   ret nested_0();\n" +
            "}\n"
        );

        s.ShouldContain(
            "at clr!EVIL.Ceres.Runtime.Modules.DebugModule::StackTraceString",
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