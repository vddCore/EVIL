namespace EVIL.Assessment.Tests.VM;

using EVIL.Core.Grammar;
using EVIL.VM.ExecutionEngine;
using EVIL.VM.ExecutionEngine.Diagnostics;
using EVIL.VM.ExecutionEngine.TypeSystem;
using EVIL.VM.TranslationEngine;
using EVIL.VM.TranslationEngine.Repl;
using Shouldly;

[TestFixture]
public class ReplCompilerTests
{
    private Parser _parser = null!;
    private Compiler _compiler = null!;
    private VirtualMachine _vm = null!;
    private ReplCompilerSession _session = null!;

    [SetUp]
    public void Setup()
    {
        _parser = new Parser();
        _compiler = new Compiler();
        _vm = new VirtualMachine();
        _vm.Run();
        _session = _compiler.CreateReplSession();
    }

    [TearDown]
    public void Teardown()
        => _vm.Stop();

    [Test]
    public void RootDeclarationsAndFunctionsPersistAcrossSubmissions()
    {
        Execute("val answer = 40;");
        Execute("fn meaning() { ret answer + 2; }");

        Execute("meaning();").Number.ShouldBe(42);
    }

    [Test]
    public void RootDestructuringBindingsPersistAcrossSubmissions()
    {
        Execute("val [first, second] <- [7, 9]; val {points: score} <- {points: 4};");

        Execute("first + second + score;").Number.ShouldBe(20);
    }

    [Test]
    public void ReplExpressionResultIsCapturedButStatementsAreNot()
    {
        Execute("rw val value = 10;");
        _compiler.HasReplResult.ShouldBeFalse();

        Execute("value + 5;").Number.ShouldBe(15);
        _compiler.HasReplResult.ShouldBeTrue();

        Execute("value = 12;");
        _compiler.HasReplResult.ShouldBeFalse();
    }

    [Test]
    public void RootBindingMutabilityIsPreservedAcrossSubmissions()
    {
        Execute("rw val counter = 1;");
        Execute("counter += 1;");

        Execute("counter;").Number.ShouldBe(2);

        Execute("val immutable = 1;");
        Assert.Throws<CompilerException>(() => Compile("immutable = 2;"));
    }

    [Test]
    public void FailedCompilationDoesNotCommitNewSessionBindings()
    {
        Execute("val existing = 1;");

        Assert.Throws<CompilerException>(() => Compile(
            "val uncommitted = 2; val existing = 3;"
        ));

        Assert.DoesNotThrow(() => Compile("val uncommitted = 4;"));
    }

    [Test]
    public void NestedDeclarationsRemainLocalAndDoNotConflictWithLaterRootBindings()
    {
        Execute("fn localValue() { val item = 11; ret item; }");

        Execute("localValue();").Number.ShouldBe(11);
        Assert.DoesNotThrow(() => Compile("val item = 12;"));
    }

    private DynamicValue Execute(string source)
    {
        var chunk = Compile(source);
        var hasResult = _compiler.HasReplResult;
        _vm.MainFiber.Schedule(chunk);
        _vm.MainFiber.BlockUntilFinished();
        return hasResult ? _vm.MainFiber.PopValue() : DynamicValue.Nil;
    }

    private Chunk Compile(string source)
        => _compiler.CompileForRepl(_parser.Parse(source), _session, "!repl!");
}