using Ceres.Runtime.Modules;
using NUnit.Framework;

namespace Ceres.RuntimeTests.RuntimeModules
{
    public class CoreModuleTest : ModuleTest<CoreModule>
    {
        [Test]
        public void StackTrace()
        {
            var t = RunEvilCode("fn test(a = 1, b = 2, c = 3) -> core.strace(true);").Table!;
            var frame = t[0].Table!;

            Assert.That(frame["is_script"].Boolean, Is.EqualTo(true));
            Assert.That(frame["fn_name"].String!, Is.EqualTo("test"));
            Assert.That(frame["def_on_line"].Number, Is.EqualTo(1));
            Assert.That(frame["args"].Table![0].Number, Is.EqualTo(1));
            Assert.That(frame["args"].Table![1].Number, Is.EqualTo(2));
            Assert.That(frame["args"].Table![2].Number, Is.EqualTo(3));
        }

        [Test]
        public void StackTraceString()
        {
            var s = RunEvilCode(
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

            Assert.That(s.Contains("at Ceres.Runtime.Modules.CoreModule::StackTraceString"));
            Assert.That(s.Contains("at nested_2 in !module_test_file!: line 2"));
            Assert.That(s.Contains("at nested_1 in !module_test_file!: line 5"));
            Assert.That(s.Contains("at nested_0 in !module_test_file!: line 8"));
            Assert.That(s.Contains("at test in !module_test_file!: line 14"));
        }
    }
}