namespace EVIL.Assessment.Tests.VM.Runtime.ModuleTests;

using EVIL.VirtualMachine.ExecutionEngine.Collections;
using EVIL.VirtualMachine.Runtime.Modules;
using EVIL.Core.Common.TypeSystem;
using NUnit.Framework;
using Shouldly;
using static EVIL.VirtualMachine.ExecutionEngine.TypeSystem.DynamicValue;

public class EvilModuleTest : ModuleTest<EvilModule>
{
    [Test]
    public void Compile_ReturnsCompiledChunk_WhenSourceValid()
    {
        var t = EvilTestResult(
            "fn test() {" +
            "   val result = evil.compile('fn test() {" +
            "       ret \"i was compiled from within evil :dobry_jezu:\";" +
            "   }');" +
            "" +
            "   ret {" +
            "     script_table: result," +
            "     test_result: result.chunk['test']()" +
            "   };" +
            "}"
        ).Table!;

        var scriptTable = (Table)t["script_table"];
        var testResult = (string)t["test_result"];

        scriptTable["success"].ShouldBe(true);
        testResult.ShouldBe("i was compiled from within evil :dobry_jezu:");
        scriptTable["diagnostics"].Type.ShouldBe(DynamicValueType.Array);
    }

    [Test]
    public void Compile_ReturnsStructuredDiagnostics_OnWarning()
    {
        var t = EvilTestResult(
            "fn test() {" +
            "   val result = evil.compile('val unused = 42; val _used = 100;');" +
            "   ret result;" +
            "}"
        ).Table!;

        t["success"].ShouldBe(true);
        var diags = t["diagnostics"].Array!;
        diags.Length.ShouldBe(1);
        var first = diags[0].Table!;
        first["id"].ShouldBe("EV8003");
        first["severity_name"].ShouldBe("Warning");
    }

    [Test]
    public void Compile_ReturnsStructuredDiagnostics_OnError()
    {
        var t = EvilTestResult(
            "fn test() {" +
            "   val result = evil.compile('val = 42;');" +
            "   ret result;" +
            "}"
        ).Table!;

        t["success"].ShouldBe(false);
        var diags = t["diagnostics"].Array!;
        diags.Length.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Test]
    public void DiagnosticSeverity_ReturnsExpectedValues()
    {
        var t = EvilTestResult(
            "fn test() -> evil.diagnostic.severity;"
        ).Table!;

        t["HIDDEN"].ShouldBe(0);
        t["INFO"].ShouldBe(1);
        t["WARNING"].ShouldBe(2);
        t["ERROR"].ShouldBe(3);
        t["FATAL"].ShouldBe(4);
        t["INTERNAL_ERROR"].ShouldBe(5);
    }

    [Test]
    public void Reflect_ReturnsAllRequiredInformation()
    {
        var t = EvilTestResult(
            "#[attrib] fn reflected(a, b = 'hi', rw c = 21) {" +
            "   val local_1 = 12;" +
            "   rw val local_2 = 10;" +
            "}" +
            "" +
            "fn test() -> evil.reflect(reflected);"
        ).Table!;

        t["name"].ShouldBe("reflected");
        t["attributes"].Type.ShouldBe(DynamicValueType.Array);

        var attributes = t["attributes"].Array!;
        attributes[0].Type.ShouldBe(DynamicValueType.Table);
        attributes[0].Table!["name"].ShouldBe("attrib");

        t["local_info"].Type.ShouldBe(DynamicValueType.Array);
        var locals = t["local_info"].Array!;
        locals[0].Type.ShouldBe(DynamicValueType.Table);
        locals[0].Table!["id"].ShouldBe(0);
        locals[0].Table!["name"].ShouldBe("local_1");
        locals[0].Table!["is_rw"].ShouldBe(false);
            
        locals[1].Type.ShouldBe(DynamicValueType.Table);
        locals[1].Table!["id"].ShouldBe(1);
        locals[1].Table!["name"].ShouldBe("local_2");
        locals[1].Table!["is_rw"].ShouldBe(true);
            
        t["param_info"].Type.ShouldBe(DynamicValueType.Array);
        var parameters = t["param_info"].Array!;
        parameters[0].Type.ShouldBe(DynamicValueType.Table);
        parameters[0].Table!["id"].ShouldBe(0);
        parameters[0].Table!["name"].ShouldBe("a");
        parameters[0].Table!["default_value"].ShouldBe(Nil);
        parameters[0].Table!["is_rw"].ShouldBe(false);
            
        parameters[1].Type.ShouldBe(DynamicValueType.Table);
        parameters[1].Table!["id"].ShouldBe(1);
        parameters[1].Table!["name"].ShouldBe("b");
        parameters[1].Table!["default_value"].ShouldBe("hi");
        parameters[1].Table!["is_rw"].ShouldBe(false);
            
        parameters[2].Type.ShouldBe(DynamicValueType.Table);
        parameters[2].Table!["id"].ShouldBe(2);
        parameters[2].Table!["name"].ShouldBe("c");
        parameters[2].Table!["default_value"].ShouldBe(21);
        parameters[2].Table!["is_rw"].ShouldBe(true);
    }
}