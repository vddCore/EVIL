﻿namespace EVIL.Ceres.RuntimeTests.RuntimeModules;

using static EVIL.Ceres.ExecutionEngine.TypeSystem.DynamicValue;

using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.Runtime.Modules;
using EVIL.CommonTypes.TypeSystem;
using NUnit.Framework;
using Shouldly;

public class EvilModuleTest : ModuleTest<EvilModule>
{
    [Test]
    public void Compile()
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
    }

    [Test]
    public void Reflect()
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