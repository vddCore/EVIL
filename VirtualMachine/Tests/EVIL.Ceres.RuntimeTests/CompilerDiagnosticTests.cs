namespace EVIL.Ceres.RuntimeTests;

using System.Linq;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Disassembly;
using EVIL.Ceres.TranslationEngine;
using EVIL.Diagnostics;
using NUnit.Framework;
using Shouldly;

[TestFixture]
public class CompilerDiagnosticTests
{
    [Test]
    public void Compiler_WithMultipleSyntaxErrors_CollectsAllErrorsInLog()
    {
        var compiler = new Compiler();
        var source = "val = 1;\nval b = 2;\nval = 3;\nval c = 4;";

        Should.Throw<CompilerException>(() => compiler.Compile(source, "test.vil"));

        compiler.Log.HasErrors.ShouldBeTrue();
        compiler.Log.ErrorCount.ShouldBe(2);
        compiler.Log.Messages.Count.ShouldBe(2);
    }

    [Test]
    public void Compiler_DuplicateSymbol_ReportsEV2001WithRelatedLocation()
    {
        var compiler = new Compiler();
        var source = "val a = 10;\nval a = 20;";

        var ex = Should.Throw<CompilerException>(() => compiler.Compile(source, "dup.vil"));

        compiler.Log.HasErrors.ShouldBeTrue();
        var diag = compiler.Log.Diagnostics.FirstOrDefault(d => d.Id == "EV2001");
        diag.ShouldNotBeNull();
        diag.Location.Line.ShouldBe(2);
        diag.Location.FilePath.ShouldBe("dup.vil");
        diag.RelatedLocations.ShouldNotBeNull();
        diag.RelatedLocations.Count.ShouldBe(1);
        diag.RelatedLocations[0].Location.Line.ShouldBe(1);
    }

    [Test]
    public void Compiler_WriteToReadOnly_ReportsEV2002()
    {
        var compiler = new Compiler();
        var source = "val a = 10;\na = 20;";

        Should.Throw<CompilerException>(() => compiler.Compile(source, "readonly.vil"));

        compiler.Log.HasErrors.ShouldBeTrue();
        var diag = compiler.Log.Diagnostics.FirstOrDefault(d => d.Id == "EV2002");
        diag.ShouldNotBeNull();
        diag.Message.ShouldContain("read-only");
    }

    [Test]
    public void Compiler_IllegalAssignmentTarget_ReportsEV4001()
    {
        var compiler = new Compiler();
        var source = "foo() = 20;";

        Should.Throw<CompilerException>(() => compiler.Compile(source, "illegal_target.vil"));

        compiler.Log.HasErrors.ShouldBeTrue();
        var diag = compiler.Log.Diagnostics.FirstOrDefault(d => d.Id == "EV4001");
        diag.ShouldNotBeNull();
    }

    [Test]
    public void Compiler_NamedFunctionRedefinition_EmitsWarningEV4006()
    {
        var compiler = new Compiler();
        var source = "fn foo() -> 1;\nfn foo() -> 2;";

        compiler.Compile(source, "redef.vil");

        compiler.Log.HasErrors.ShouldBeFalse();
        compiler.Log.WarningCount.ShouldBe(1);
        var diag = compiler.Log.Diagnostics.FirstOrDefault(d => d.Id == "EV4006");
        diag.ShouldNotBeNull();
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
    }

    [Test]
    public void Compiler_TargetedFunctionRedefinition_EmitsWarningEV4007()
    {
        var compiler = new Compiler();
        var source = "val obj = {};\nloc fn obj::bar() -> 1;\nloc fn obj::bar() -> 2;";

        compiler.Compile(source, "redef_targeted.vil");

        compiler.Log.HasErrors.ShouldBeFalse();
    }

    [Test]
    public void Compiler_NamedFunctionReplacement_UsesStableChildIdentityWithMixedChildren()
    {
        var compiler = new Compiler();
        var source = """
            fn foo() -> 1;
            val anonymous = fn() -> 2;
            fn other() -> 3;
            fn test() -> foo();
            fn foo() -> 4;
            fn padding() -> 99;
            """;

        var root = compiler.Compile(source, "redef_mixed.vil");

        compiler.Log.HasErrors.ShouldBeFalse();
        compiler.Log.Diagnostics.First(d => d.Id == "EV4006")
                .Severity
                .ShouldBe(DiagnosticSeverity.Warning);
        
        root.SubChunkCount.ShouldBe(5);
        root.NamedSubChunkLookup["foo"].ShouldBe(0);
        root["foo"].ShouldBeSameAs(root.SubChunks[0]);
        root.SubChunks[0].Name.ShouldBe("foo");
        root.NamedSubChunkLookup.Values.ShouldNotContain(1);
        root.SubChunks[2].Name.ShouldBe("other");
        root.SubChunks[3].Name.ShouldBe("test");
        root.SubChunks[4].Name.ShouldBe("padding");

        var subChunkIds = InstructionDecoder.Decode(root).Instructions
            .Where(instruction => instruction.OpCode == OpCode.LDCNK)
            .Select(instruction => (int)instruction.Operands[0].RawValue)
            .ToArray();

        subChunkIds.ShouldBe([0, 1, 2, 3, 0, 4]);
    }

    [Test]
    public void Compiler_IndexedFunctionRedefinition_PreservesEachDefinitionChunk()
    {
        var compiler = new Compiler();
        var source = """
            val obj = {};
            loc fn obj.bar() -> 1;
            loc fn obj.bar() -> 2;
            """;

        var root = compiler.Compile(source, "redef_indexed.vil");

        compiler.Log.HasErrors.ShouldBeFalse();
        compiler.Log.WarningCount.ShouldBe(1);
        compiler.Log.Diagnostics.First(d => d.Id == "EV4008")
                .Severity
                .ShouldBe(DiagnosticSeverity.Warning);
        root.SubChunkCount.ShouldBe(2);
        root.NamedSubChunkLookup["obj.bar"].ShouldBe(1);
        root.SubChunks.Select(chunk => chunk.Name).ShouldBe(["obj.bar", "obj.bar"]);
        root["obj.bar"].ShouldBeSameAs(root.SubChunks[1]);

        var subChunkIds = InstructionDecoder.Decode(root).Instructions
            .Where(instruction => instruction.OpCode == OpCode.LDCNK)
            .Select(instruction => (int)instruction.Operands[0].RawValue)
            .ToArray();

        subChunkIds.ShouldBe([0, 1]);
    }

    [Test]
    public void Compiler_TargetedFunctionRedefinition_PreservesEachDefinitionChunk()
    {
        var compiler = new Compiler();
        var source = """
            val obj = {};
            loc fn obj::bar() -> 1;
            loc fn obj::bar() -> 2;
            """;

        var root = compiler.Compile(source, "redef_targeted.vil");

        compiler.Log.HasErrors.ShouldBeFalse();
        compiler.Log.WarningCount.ShouldBe(1);
        compiler.Log.Diagnostics.First(d => d.Id == "EV4007")
                .Severity
                .ShouldBe(DiagnosticSeverity.Warning);
        root.SubChunkCount.ShouldBe(2);
        root.NamedSubChunkLookup["obj::bar"].ShouldBe(1);
        root.SubChunks.Select(chunk => chunk.Name).ShouldBe(["obj::bar", "obj::bar"]);
        root["obj::bar"].ShouldBeSameAs(root.SubChunks[1]);
        root.SubChunks.ShouldAllBe(chunk => chunk.IsSelfAware);
        root.SubChunks.ShouldAllBe(chunk => chunk.ParameterCount == 1);

        var subChunkIds = InstructionDecoder.Decode(root).Instructions
            .Where(instruction => instruction.OpCode == OpCode.LDCNK)
            .Select(instruction => (int)instruction.Operands[0].RawValue)
            .ToArray();

        subChunkIds.ShouldBe([0, 1]);
    }

    [Test]
    public void Compiler_IndexedFunctionPrefixAndSuffixNamesRemainDistinct()
    {
        var compiler = new Compiler();
        var source = """
            val obj = {};
            loc fn obj.barExtra() -> 1;
            loc fn obj.bar() -> 2;
            loc fn obj.bar_1() -> 3;
            loc fn obj.bar() -> 4;
            """;

        var root = compiler.Compile(source, "redef_indexed_collisions.vil");

        compiler.Log.HasErrors.ShouldBeFalse();
        root.SubChunkCount.ShouldBe(4);
        root.NamedSubChunkLookup["obj.barExtra"].ShouldBe(0);
        root.NamedSubChunkLookup["obj.bar"].ShouldBe(3);
        root.NamedSubChunkLookup["obj.bar_1"].ShouldBe(2);
        root.SubChunks
            .Select(chunk => chunk.Name)
            .ShouldBe(["obj.barExtra", "obj.bar", "obj.bar_1", "obj.bar"]);

        var subChunkIds = InstructionDecoder.Decode(root).Instructions
            .Where(instruction => instruction.OpCode == OpCode.LDCNK)
            .Select(instruction => (int)instruction.Operands[0].RawValue)
            .ToArray();

        subChunkIds.ShouldBe([0, 1, 2, 3]);
    }

    [Test]
    public void Compiler_TargetedFunctionPrefixAndSuffixNamesRemainDistinct()
    {
        var compiler = new Compiler();
        var source = """
            val obj = {};
            loc fn obj::barExtra() -> 1;
            loc fn obj::bar() -> 2;
            loc fn obj::bar_1() -> 3;
            loc fn obj::bar() -> 4;
            """;

        var root = compiler.Compile(source, "redef_targeted_collisions.vil");

        compiler.Log.HasErrors.ShouldBeFalse();
        compiler.Log.WarningCount.ShouldBe(1);
        root.SubChunkCount.ShouldBe(4);
        root.NamedSubChunkLookup["obj::barExtra"].ShouldBe(0);
        root.NamedSubChunkLookup["obj::bar"].ShouldBe(3);
        root.NamedSubChunkLookup["obj::bar_1"].ShouldBe(2);
        root.SubChunks
            .Select(chunk => chunk.Name)
            .ShouldBe(["obj::barExtra", "obj::bar", "obj::bar_1", "obj::bar"]);

        var subChunkIds = InstructionDecoder.Decode(root).Instructions
            .Where(instruction => instruction.OpCode == OpCode.LDCNK)
            .Select(instruction => (int)instruction.Operands[0].RawValue)
            .ToArray();

        subChunkIds.ShouldBe([0, 1, 2, 3]);
    }

    [Test]
    public void Compiler_TooManyInitializersForConstSizeArray_ReportsEV4005()
    {
        var compiler = new Compiler();
        var source = "val arr = array(2) { 1, 2, 3 };";

        Should.Throw<CompilerException>(() => compiler.Compile(source, "arr_overflow.vil"));

        compiler.Log.HasErrors.ShouldBeTrue();
        var diag = compiler.Log.Diagnostics.FirstOrDefault(d => d.Id == "EV4005");
        diag.ShouldNotBeNull();
    }

    [Test]
    public void Compiler_SelfInSelfUnawareFunction_ReportsEV4004()
    {
        var compiler = new Compiler();
        var source = "fn foo() { ret self; }";

        Should.Throw<CompilerException>(() => compiler.Compile(source, "self_err.vil"));

        compiler.Log.HasErrors.ShouldBeTrue();
        var diag = compiler.Log.Diagnostics.FirstOrDefault(d => d.Id == "EV4004");
        diag.ShouldNotBeNull();
    }

    [Test]
    public void Compiler_ByWithoutDefault_ReportsEV8002()
    {
        var compiler = new Compiler();
        var source = "val _x = 1;\nval _y = by _x { 1 => \"one\" };";

        compiler.Compile(source, "by_test.vil");

        compiler.Log.HasErrors.ShouldBeFalse();
        compiler.Log.WarningCount.ShouldBe(1);
        var diag = compiler.Log.Diagnostics.FirstOrDefault(d => d.Id == "EV8002");
        diag.ShouldNotBeNull();
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
    }

    [Test]
    public void Compiler_WhenEnableAnalyzersIsFalse_DoesNotRunAnalyzers()
    {
        var compiler = new Compiler(CompilerOptions.Default with { EnableAnalyzers = false });
        var source = "val _x = 1;\nval _y = by _x { 1 => \"one\" };";

        compiler.Compile(source, "no_analyzers.vil");

        compiler.Log.HasErrors.ShouldBeFalse();
        compiler.Log.WarningCount.ShouldBe(0);
    }

    [Test]
    public void Compiler_TableDuplicateKey_ReportsEV8006()
    {
        var compiler = new Compiler();
        var source = "val _t = { a: 1, a: 2 };";

        compiler.Compile(source, "dup_key.vil");

        compiler.Log.HasErrors.ShouldBeFalse();
        compiler.Log.WarningCount.ShouldBe(1);
        var diag = compiler.Log.Diagnostics.FirstOrDefault(d => d.Id == "EV8006");
        diag.ShouldNotBeNull();
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
    }

    [Test]
    public void Compiler_CatchlessTry_ReportsEV8005()
    {
        var compiler = new Compiler();
        var source = "try { val _x = 1; }";

        compiler.Compile(source, "catchless.vil");

        compiler.Log.HasErrors.ShouldBeFalse();
        compiler.Log.WarningCount.ShouldBe(1);
        var diag = compiler.Log.Diagnostics.FirstOrDefault(d => d.Id == "EV8005");
        diag.ShouldNotBeNull();
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
    }

    [Test]
    public void Compiler_UnreachableCode_ReportsEV8004()
    {
        var compiler = new Compiler();
        var source = "fn foo() { ret 1; val _x = 2; }";

        compiler.Compile(source, "unreachable.vil");

        compiler.Log.HasErrors.ShouldBeFalse();
        compiler.Log.WarningCount.ShouldBe(1);
        var diag = compiler.Log.Diagnostics.FirstOrDefault(d => d.Id == "EV8004");
        diag.ShouldNotBeNull();
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
    }

    [Test]
    public void Compiler_RedundantSelfParameter_ReportsEV8001()
    {
        var compiler = new Compiler();
        var source = "val _obj = {};\nloc fn _obj::bar(self, x) -> x;";

        compiler.Compile(source, "redundant_self.vil");

        compiler.Log.HasErrors.ShouldBeFalse();
        var diag = compiler.Log.Diagnostics.FirstOrDefault(d => d.Id == "EV8001");
        diag.ShouldNotBeNull();
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
    }

    [Test]
    public void Compiler_UnusedVariable_ReportsEV8003()
    {
        var compiler = new Compiler();
        var source = "val unusedVar = 10;\nval _used = 20;";

        compiler.Compile(source, "unused.vil");

        compiler.Log.HasErrors.ShouldBeFalse();
        var diag = compiler.Log.Diagnostics.FirstOrDefault(d => d.Id == "EV8003");
        diag.ShouldNotBeNull();
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diag.Message.ShouldContain("'unusedVar'");
    }

    [Test]
    public void Compiler_UnusedVariable_RecognizesInterpolatedStringUsage()
    {
        var compiler = new Compiler();
        var source = "val name = \"Bob\";\nval _greeting = \"Hello, $name!\";";

        compiler.Compile(source, "interpolated_usage.vil");

        compiler.Log.HasErrors.ShouldBeFalse();
        compiler.Log.WarningCount.ShouldBe(0);
    }

    [Test]
    public void Compiler_WarningsAsErrors_ElevatesAllWarningsToErrors()
    {
        var compiler = new Compiler(CompilerOptions.Default with { WarningsAsErrors = true });
        var source = "val unusedVar = 10;\nval _used = 20;";

        Should.Throw<CompilerException>(() => compiler.Compile(source, "warnaserror.vil"));

        compiler.Log.HasErrors.ShouldBeTrue();
        var diag = compiler.Log.Diagnostics.FirstOrDefault(d => d.Id == "EV8003");
        diag.ShouldNotBeNull();
        diag.Severity.ShouldBe(DiagnosticSeverity.Error);
    }

    [Test]
    public void Compiler_WarningsAsErrorsCodes_ElevatesSpecificWarningOnly()
    {
        var options = CompilerOptions.Default;
        options.WarningsAsErrorsCodes.Add("EV8003");
        
        var compiler = new Compiler(options);
        var source = "val unusedVar = 10;\nval _used = 20;";

        Should.Throw<CompilerException>(() => compiler.Compile(source, "warnaserror_code.vil"));

        compiler.Log.HasErrors.ShouldBeTrue();
        var diag = compiler.Log.Diagnostics.FirstOrDefault(d => d.Id == "EV8003");
        diag.ShouldNotBeNull();
        diag.Severity.ShouldBe(DiagnosticSeverity.Error);
    }

    [Test]
    public void Compiler_SuppressedCodes_IgnoresSpecifiedWarning()
    {
        var options = CompilerOptions.Default;
        options.SuppressedCodes.Add("EV8003");
        
        var compiler = new Compiler(options);
        var source = "val unusedVar = 10;\nval _used = 20;";

        compiler.Compile(source, "nowarn.vil");

        compiler.Log.HasErrors.ShouldBeFalse();
        compiler.Log.WarningCount.ShouldBe(0);
        compiler.Log.Diagnostics.Count.ShouldBe(0);
    }

    [Test]
    public void Compiler_MaxErrors_LimitsCollectedErrors()
    {
        var options = CompilerOptions.Default with { MaxErrors = 2 };
        var compiler = new Compiler(options);
        var source = "val = 1;\nval = 2;\nval = 3;\nval = 4;\nval = 5;";

        Should.Throw<CompilerException>(() => compiler.Compile(source, "max_errors.vil"));

        compiler.Log.ErrorCount.ShouldBeLessThanOrEqualTo(2);
    }
}