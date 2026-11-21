namespace EVIL.CoreTests;

using System.Linq;
using EVIL.CommonTypes.Diagnostics;
using EVIL.Grammar;
using EVIL.Grammar.Analysis;
using EVIL.Grammar.Analysis.Analyzers;
using EVIL.Grammar.AST.Statements;
using NUnit.Framework;
using Shouldly;

[TestFixture]
public class AnalysisTests
{
    private static (EVIL.Grammar.AST.Miscellaneous.ProgramNode Program, DiagnosticBag Bag) ParseAndAnalyze(
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
    public void AnalyzerDriver_CreateDefault_IncludesAllSixAnalyzers()
    {
        var driver = AnalyzerDriver.CreateDefault();
        driver.Analyzers.Count.ShouldBe(6);
        driver.Analyzers.ShouldContain(a => a is MissingByElseArmAnalyzer);
        driver.Analyzers.ShouldContain(a => a is RedundantSelfParameterAnalyzer);
        driver.Analyzers.ShouldContain(a => a is UnusedVariableAnalyzer);
        driver.Analyzers.ShouldContain(a => a is UnreachableCodeAnalyzer);
        driver.Analyzers.ShouldContain(a => a is CatchlessTryAnalyzer);
        driver.Analyzers.ShouldContain(a => a is TableDuplicateKeyAnalyzer);
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
}
