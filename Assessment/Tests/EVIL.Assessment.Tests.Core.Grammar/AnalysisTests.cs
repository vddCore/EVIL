namespace EVIL.Assessment.Tests.Core.Grammar;

using System.Linq;
using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar;
using EVIL.Core.Analysis;
using EVIL.Core.Analysis.Analyzers;
using EVIL.Core.Grammar.AST.Miscellaneous;
using EVIL.Core.Grammar.AST.Statements;
using NUnit.Framework;
using Shouldly;

[TestFixture]
public class AnalysisTests
{
    private static (ProgramNode Program, DiagnosticBag Bag) ParseAndAnalyze(
        string source,
        params IDiagnosticAnalyzer[] analyzers)
    {
        var parserBag = new DiagnosticBag();
        var parser = new Parser(parserBag, "test.vil");
        var program = parser.Parse(source);

        var driver = new AnalyzerDriver(analyzers);
        var analysisBag = new DiagnosticBag();
        driver.Analyze(program, "test.vil", analysisBag);

        return (program, analysisBag);
    }

    [Test]
    public void AnalyzerDriver_CreateDefault_IncludesAllFourteenAnalyzers()
    {
        var driver = AnalyzerDriver.CreateDefault();
        driver.Analyzers.Count.ShouldBe(14);
        driver.Analyzers.ShouldContain(a => a is MissingByElseArmAnalyzer);
        driver.Analyzers.ShouldContain(a => a is RedundantSelfParameterAnalyzer);
        driver.Analyzers.ShouldContain(a => a is UnusedVariableAnalyzer);
        driver.Analyzers.ShouldContain(a => a is UnreachableCodeAnalyzer);
        driver.Analyzers.ShouldContain(a => a is CatchlessTryAnalyzer);
        driver.Analyzers.ShouldContain(a => a is TableDuplicateKeyAnalyzer);
        driver.Analyzers.ShouldContain(a => a is DangerousUnsafeDerefAfterElvisAnalyzer);
        driver.Analyzers.ShouldContain(a => a is RedundantSafeNavigationAnalyzer);
        driver.Analyzers.ShouldContain(a => a is InvalidConditionalAssignmentAnalyzer);
        driver.Analyzers.ShouldContain(a => a is RedundantNilCoalesceAnalyzer);
        driver.Analyzers.ShouldContain(a => a is UncheckedNilInvocationAnalyzer);
        driver.Analyzers.ShouldContain(a => a is NilComparisonTypeMismatchAnalyzer);
        driver.Analyzers.ShouldContain(a => a is NilBranchExhaustivenessAnalyzer);
        driver.Analyzers.ShouldContain(a => a is UncheckedGlobalDereferenceAnalyzer);
    }

    [Test]
    public void MissingByElseArmAnalyzer_ReportsEV8002_WhenElseMissing()
    {
        var source = "val _y = by 1 { 1 => \"a\", 2 => \"b\" };";
        var (_, bag) = ParseAndAnalyze(source, new MissingByElseArmAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8002");
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
    }

    [Test]
    public void MissingByElseArmAnalyzer_NoWarning_WhenElsePresent()
    {
        var source = "val _y = by 1 { 1 => \"a\", else => \"b\" };";
        var (_, bag) = ParseAndAnalyze(source, new MissingByElseArmAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void RedundantSelfParameterAnalyzer_ReportsEV8001_WhenSelfExplicitInTargetedFn()
    {
        var source = "val _obj = {};\nloc fn _obj::action(self, x) { ret x; }";
        var (_, bag) = ParseAndAnalyze(source, new RedundantSelfParameterAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8001");
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
    }

    [Test]
    public void RedundantSelfParameterAnalyzer_NoWarning_WhenSelfNotExplicit()
    {
        var source = "val _obj = {};\nloc fn _obj::action(x) { ret x; }";
        var (_, bag) = ParseAndAnalyze(source, new RedundantSelfParameterAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void UnusedVariableAnalyzer_ReportsEV8003_WhenLocalNeverRead()
    {
        var source = "val unused = 42;\nval used = 100;\nret used;";
        var (_, bag) = ParseAndAnalyze(source, new UnusedVariableAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8003");
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diag.Message.ShouldContain("'unused'");
    }

    [Test]
    public void UnusedVariableAnalyzer_IgnoresUnderscorePrefixedVariables()
    {
        var source = "val _discard = 42;\nval _ = 100;";
        var (_, bag) = ParseAndAnalyze(source, new UnusedVariableAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void UnusedVariableAnalyzer_TakesIntoAccountIdentifierInInterpolatedString()
    {
        var source = "val name = \"Alice\";\nval greeting = $\"Hello, $name!\";\nret greeting;";
        var (_, bag) = ParseAndAnalyze(source, new UnusedVariableAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void UnusedVariableAnalyzer_TakesIntoAccountExpressionInInterpolatedString()
    {
        var source = "val x = 10;\nval y = 20;\nval msg = $\"sum: ${x + y}\";\nret msg;";
        var (_, bag) = ParseAndAnalyze(source, new UnusedVariableAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void UnusedVariableAnalyzer_ReportsUnusedVariable_WhenNotInInterpolatedString()
    {
        var source = "val used = \"Alice\";\nval unused = 42;\nval msg = $\"Hello, $used\";\nret msg;";
        var (_, bag) = ParseAndAnalyze(source, new UnusedVariableAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8003");
        diag.Message.ShouldContain("'unused'");
    }

    [Test]
    public void UnusedVariableAnalyzer_IgnoresEscapedDollarInInterpolatedString()
    {
        var source = "val unused = 100;\nval msg = $\"Price is \\$unused\";\nret msg;";
        var (_, bag) = ParseAndAnalyze(source, new UnusedVariableAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8003");
        diag.Message.ShouldContain("'unused'");
    }

    [Test]
    public void UnusedVariableAnalyzer_TakesIntoAccountSelfInvocationTarget()
    {
        var source = "val t = {};\nt:func();";
        var (_, bag) = ParseAndAnalyze(source, new UnusedVariableAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void UnusedVariableAnalyzer_TakesIntoAccountTargetedFnPrimaryTarget()
    {
        var source = "val obj = {};\nloc fn obj::say_hi() {}";
        var (_, bag) = ParseAndAnalyze(source, new UnusedVariableAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void UnusedVariableAnalyzer_TakesIntoAccountSelfFnExpression()
    {
        var source = "val t = [1, 2];\nval _tbl = { __get: self::fn(k) -> t[k] };";
        var (_, bag) = ParseAndAnalyze(source, new UnusedVariableAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void UnusedVariableAnalyzer_TakesIntoAccountIsExpressionManagedTypeName()
    {
        var source = "val type_name = \"System.String\";\nval _b = 123 is NativeObject \"${type_name}\";";
        var (_, bag) = ParseAndAnalyze(source, new UnusedVariableAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void UnreachableCodeAnalyzer_ReportsEV8004_AfterRetStatement()
    {
        var source = "fn foo() {\n  ret 1;\n  val _a = 2;\n}";
        var (_, bag) = ParseAndAnalyze(source, new UnreachableCodeAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8004");
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diag.Message.ShouldContain("unreachable");
        diag.Message.ShouldContain("'ret'");
    }

    [Test]
    public void UnreachableCodeAnalyzer_ReportsEV8004_AfterThrowStatement()
    {
        var source = "fn foo() {\n  throw \"error\";\n  val _a = 2;\n}";
        var (_, bag) = ParseAndAnalyze(source, new UnreachableCodeAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8004");
        diag.Message.ShouldContain("'throw'");
    }

    [Test]
    public void CatchlessTryAnalyzer_ReportsEV8005_WhenCatchOmitted()
    {
        var source = "try {\n  val _x = 1;\n}";
        var (_, bag) = ParseAndAnalyze(source, new CatchlessTryAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8005");
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diag.Message.ShouldContain("try statement has no catch handler");
    }

    [Test]
    public void CatchlessTryAnalyzer_NoWarning_WhenCatchPresent()
    {
        var source = "try {\n  val _x = 1;\n} catch (e) {\n  ret 0;\n}";
        var (_, bag) = ParseAndAnalyze(source, new CatchlessTryAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void TableDuplicateKeyAnalyzer_ReportsEV8006_OnDuplicateIdentifierKey()
    {
        var source = "val _t = { a: 1, b: 2, a: 3 };";
        var (_, bag) = ParseAndAnalyze(source, new TableDuplicateKeyAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8006");
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diag.Message.ShouldContain("'a'");
    }

    [Test]
    public void TableDuplicateKeyAnalyzer_ReportsEV8006_OnDuplicateStringKey()
    {
        var source = "val _t = { \"key\" => 1, \"key\" => 2 };";
        var (_, bag) = ParseAndAnalyze(source, new TableDuplicateKeyAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8006");
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diag.Message.ShouldContain("'key'");
    }

    [Test]
    public void CustomAnalyzer_CanRegisterSyntaxNodeAction()
    {
        var customDescriptor = new DiagnosticDescriptor(
            "CUSTOM001",
            "Custom warning",
            "Found custom return",
            DiagnosticCategory.Inspection,
            DiagnosticSeverity.Warning
        );

        var customAnalyzer = new CustomReturnAnalyzer(customDescriptor);
        var source = "ret 42;";
        var (_, bag) = ParseAndAnalyze(source, customAnalyzer);

        bag.Count.ShouldBe(1);
        bag.Diagnostics.First().Id.ShouldBe("CUSTOM001");
    }

    private sealed class CustomReturnAnalyzer : IDiagnosticAnalyzer
    {
        private readonly DiagnosticDescriptor _descriptor;

        public CustomReturnAnalyzer(DiagnosticDescriptor descriptor)
        {
            _descriptor = descriptor;
            SupportedDiagnostics = [descriptor];
        }

        public System.Collections.Generic.IReadOnlyCollection<DiagnosticDescriptor> SupportedDiagnostics { get; }

        public void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction<RetStatement>(ctx =>
            {
                ctx.ReportDiagnostic(_descriptor.Create(ctx.GetLocation()));
            });
        }
    }

    [Test]
    public void DangerousUnsafeDerefAfterElvisAnalyzer_ReportsEV8007_WhenUnsafeFollowsSafe()
    {
        var source = "val a = nil; val _x = a?.b.c;";
        var (_, bag) = ParseAndAnalyze(source, new DangerousUnsafeDerefAfterElvisAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8007");
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diag.Message.ShouldContain("'c'");
    }

    [Test]
    public void DangerousUnsafeDerefAfterElvisAnalyzer_NoWarning_WhenChainIsAllSafe()
    {
        var source = "val a = nil; val _x = a?.b?.c;";
        var (_, bag) = ParseAndAnalyze(source, new DangerousUnsafeDerefAfterElvisAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void DangerousUnsafeDerefAfterElvisAnalyzer_NoWarning_WhenNoSafeNavigation()
    {
        var source = "val a = { b: { c: 1 } }; val _x = a.b.c;";
        var (_, bag) = ParseAndAnalyze(source, new DangerousUnsafeDerefAfterElvisAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void DangerousUnsafeDerefAfterElvisAnalyzer_NoWarning_WhenProtectedByNonNilAssertion()
    {
        var source = "val a = nil; val _x = a?.b!.c;";
        var (_, bag) = ParseAndAnalyze(source, new DangerousUnsafeDerefAfterElvisAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void RedundantSafeNavigationAnalyzer_ReportsEV8008_OnTableLiteral()
    {
        var source = "val _x = { prop: 1 }?.prop;";
        var (_, bag) = ParseAndAnalyze(source, new RedundantSafeNavigationAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8008");
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
    }

    [Test]
    public void RedundantSafeNavigationAnalyzer_ReportsEV8008_OnArrayLiteral()
    {
        var source = "val _x = [1, 2]?[0];";
        var (_, bag) = ParseAndAnalyze(source, new RedundantSafeNavigationAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8008");
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
    }

    [Test]
    public void RedundantSafeNavigationAnalyzer_ReportsEV8008_OnBuiltinSingleton()
    {
        var source = "val _x = math?.sqrt;";
        var (_, bag) = ParseAndAnalyze(source, new RedundantSafeNavigationAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8008");
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diag.Message.ShouldContain("math");
    }

    [Test]
    public void RedundantSafeNavigationAnalyzer_ReportsEV8008_OnNonNilAssertion()
    {
        var source = "val user = nil; val _x = user!?.prop;";
        var (_, bag) = ParseAndAnalyze(source, new RedundantSafeNavigationAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8008");
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
    }

    [Test]
    public void RedundantSafeNavigationAnalyzer_NoWarning_OnAmbientGlobal()
    {
        var source = "val _x = CONFIG?.port;";
        var (_, bag) = ParseAndAnalyze(source, new RedundantSafeNavigationAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void RedundantSafeNavigationAnalyzer_NoWarning_OnLocalVariable()
    {
        var source = "val a = nil; val _x = a?.prop;";
        var (_, bag) = ParseAndAnalyze(source, new RedundantSafeNavigationAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void InvalidConditionalAssignmentAnalyzer_ReportsEV8009_OnUnsafeChainTarget()
    {
        var source = "val a = { b: nil }; a?.b.c = 10;";
        var (_, bag) = ParseAndAnalyze(source, new InvalidConditionalAssignmentAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8009");
        diag.Severity.ShouldBe(DiagnosticSeverity.Error);
    }

    [Test]
    public void InvalidConditionalAssignmentAnalyzer_ReportsEV8009_OnImmutableLiteral()
    {
        var source = "val _x = ({ }?.prop = 10);";
        var (_, bag) = ParseAndAnalyze(source, new InvalidConditionalAssignmentAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8009");
        diag.Severity.ShouldBe(DiagnosticSeverity.Error);
    }

    [Test]
    public void InvalidConditionalAssignmentAnalyzer_NoWarning_OnValidSafeAssignment()
    {
        var source = "val a = nil; a?.prop = 10;";
        var (_, bag) = ParseAndAnalyze(source, new InvalidConditionalAssignmentAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void InvalidConditionalAssignmentAnalyzer_NoWarning_OnChainedSafeAssignment()
    {
        var source = "val a = nil; a?.b?.c = 10;";
        var (_, bag) = ParseAndAnalyze(source, new InvalidConditionalAssignmentAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void RedundantNilCoalesceAnalyzer_ReportsEV8010_OnGuaranteedNonNilLiteral()
    {
        var source = "val _x = { } ?? { default: 1 };";
        var (_, bag) = ParseAndAnalyze(source, new RedundantNilCoalesceAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8010");
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
    }

    [Test]
    public void RedundantNilCoalesceAnalyzer_ReportsEV8010_OnRightHandNil()
    {
        var source = "val a = nil; val _x = a ?? nil;";
        var (_, bag) = ParseAndAnalyze(source, new RedundantNilCoalesceAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8010");
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diag.Message.ShouldContain("nil");
    }

    [Test]
    public void RedundantNilCoalesceAnalyzer_ReportsEV8010_OnNonNilAssertion()
    {
        var source = "val user = nil; val _x = user! ?? \"fallback\";";
        var (_, bag) = ParseAndAnalyze(source, new RedundantNilCoalesceAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8010");
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
    }

    [Test]
    public void RedundantNilCoalesceAnalyzer_NoWarning_OnAmbientGlobal()
    {
        var source = "val _x = CONFIG ?? { default: 1 };";
        var (_, bag) = ParseAndAnalyze(source, new RedundantNilCoalesceAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void RedundantNilCoalesceAnalyzer_NoWarning_OnNilableExpression()
    {
        var source = "val user = nil; val _x = user?.name ?? \"anon\";";
        var (_, bag) = ParseAndAnalyze(source, new RedundantNilCoalesceAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void UncheckedNilInvocationAnalyzer_ReportsEV8011_OnConditionalIndexInvocation()
    {
        var source = "val obj = nil; (obj?.callback)();";
        var (_, bag) = ParseAndAnalyze(source, new UncheckedNilInvocationAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8011");
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diag.Message.ShouldContain("'callback'");
    }

    [Test]
    public void UncheckedNilInvocationAnalyzer_NoWarning_OnNormalInvocation()
    {
        var source = "val obj = { callback: fn() {} }; obj.callback();";
        var (_, bag) = ParseAndAnalyze(source, new UncheckedNilInvocationAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void UncheckedNilInvocationAnalyzer_NoWarning_OnSafeFunctionInvocation()
    {
        var source = "val obj = nil; obj?.callback?();";
        var (_, bag) = ParseAndAnalyze(source, new UncheckedNilInvocationAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void UncheckedNilInvocationAnalyzer_ReportsEV8011_OnUncheckedSelfInvocation()
    {
        var source = "val obj = nil; obj?.child:method();";
        var (_, bag) = ParseAndAnalyze(source, new UncheckedNilInvocationAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8011");
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diag.Message.ShouldContain("'method'");
    }

    [Test]
    public void UncheckedNilInvocationAnalyzer_NoWarning_OnSafeSelfInvocation()
    {
        var source = "val obj = nil; obj?.child?:method();";
        var (_, bag) = ParseAndAnalyze(source, new UncheckedNilInvocationAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void NilComparisonTypeMismatchAnalyzer_ReportsEV8012_OnRelationalComparison()
    {
        var source = "val score = 10; val _x = score > nil;";
        var (_, bag) = ParseAndAnalyze(source, new NilComparisonTypeMismatchAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8012");
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diag.Message.ShouldContain("'>'");
    }

    [Test]
    public void NilComparisonTypeMismatchAnalyzer_ReportsEV8012_OnReversedRelationalComparison()
    {
        var source = "val count = 5; val _y = nil <= count;";
        var (_, bag) = ParseAndAnalyze(source, new NilComparisonTypeMismatchAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8012");
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diag.Message.ShouldContain("'<='");
    }

    [Test]
    public void NilComparisonTypeMismatchAnalyzer_NoWarning_OnEqualityComparison()
    {
        var source = "val score = 10; val _x = score == nil; val _y = score != nil;";
        var (_, bag) = ParseAndAnalyze(source, new NilComparisonTypeMismatchAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void NilBranchExhaustivenessAnalyzer_ReportsEV8013_WhenNilAndElseMissing()
    {
        var source = "val user = nil; val _x = by (user?.status) { 1 => \"active\", 2 => \"inactive\" };";
        var (_, bag) = ParseAndAnalyze(source, new NilBranchExhaustivenessAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8013");
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
    }

    [Test]
    public void NilBranchExhaustivenessAnalyzer_NoWarning_WhenElsePresent()
    {
        var source = "val user = nil; val _x = by (user?.status) { 1 => \"active\", else: \"unknown\" };";
        var (_, bag) = ParseAndAnalyze(source, new NilBranchExhaustivenessAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void NilBranchExhaustivenessAnalyzer_NoWarning_WhenNilArmPresent()
    {
        var source = "val user = nil; val _x = by (user?.status) { 1 => \"active\", nil => \"none\" };";
        var (_, bag) = ParseAndAnalyze(source, new NilBranchExhaustivenessAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void NilBranchExhaustivenessAnalyzer_NoWarning_WhenQualifierNotConditional()
    {
        var source = "val user = { status: 1 }; val _x = by (user.status) { 1 => \"active\" };";
        var (_, bag) = ParseAndAnalyze(source, new NilBranchExhaustivenessAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void UncheckedGlobalDereferenceAnalyzer_ReportsEV8014_OnAmbientGlobal()
    {
        var source = "val _port = CONFIG.port;";
        var (_, bag) = ParseAndAnalyze(source, new UncheckedGlobalDereferenceAnalyzer());

        bag.Count.ShouldBe(1);
        var diag = bag.Diagnostics.First();
        diag.Id.ShouldBe("EV8014");
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diag.Message.ShouldContain("'CONFIG'");
    }

    [Test]
    public void UncheckedGlobalDereferenceAnalyzer_NoWarning_OnSafeNavigation()
    {
        var source = "val _port = CONFIG?.port;";
        var (_, bag) = ParseAndAnalyze(source, new UncheckedGlobalDereferenceAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void UncheckedGlobalDereferenceAnalyzer_NoWarning_OnBuiltinSingleton()
    {
        var source = "val _x = math.sqrt;";
        var (_, bag) = ParseAndAnalyze(source, new UncheckedGlobalDereferenceAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void UncheckedGlobalDereferenceAnalyzer_NoWarning_OnLocalVariable()
    {
        var source = "val local_cfg = { port: 8080 }; val _port = local_cfg.port;";
        var (_, bag) = ParseAndAnalyze(source, new UncheckedGlobalDereferenceAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void UncheckedGlobalDereferenceAnalyzer_NoWarning_OnParameter()
    {
        var source = "fn test(param_cfg) { val _port = param_cfg.port; }";
        var (_, bag) = ParseAndAnalyze(source, new UncheckedGlobalDereferenceAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void UncheckedGlobalDereferenceAnalyzer_NoWarning_OnLoopVariable()
    {
        var source = "val items = [{ a: 1 }]; each (rw val item : items) { val _x = item.a; }";
        var (_, bag) = ParseAndAnalyze(source, new UncheckedGlobalDereferenceAnalyzer());

        bag.Count.ShouldBe(0);
    }

    [Test]
    public void DefaultAnalyzerDriver_RunsAllAnalyzersCleanlyOnIdiomaticScript()
    {
        var source = @"
            fn process_user(user) {
                val name = user?.name ?? ""anonymous"";
                val age = user?.profile?.age;
                val active = by (user?.status) {
                    1 -> true,
                    nil -> false,
                    else: false
                };
                ret { name: name, age: age, active: active };
            }

            val u = { name: ""Alice"", profile: { age: 30 }, status: 1 };
            val _res = process_user(u);
        ";

        var driver = AnalyzerDriver.CreateDefault();
        var parserBag = new DiagnosticBag();
        var parser = new Parser(parserBag, "test.vil");
        var program = parser.Parse(source);

        var analysisBag = new DiagnosticBag();
        driver.Analyze(program, "test.vil", analysisBag);

        analysisBag.Count.ShouldBe(0);
    }
}
