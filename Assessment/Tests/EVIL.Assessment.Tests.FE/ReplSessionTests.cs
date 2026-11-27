namespace EVIL.Assessment.Tests.FE;

using System.IO;
using EVIL.FE;
using Shouldly;

[TestFixture]
public class ReplSessionTests
{
    [Test]
    public async Task RunAsync_EmptyInputReachesPrimaryPromptAndExitsOnEof()
    {
        using var input = new StringReader(string.Empty);
        using var output = new StringWriter();

        await new ReplSession(input, output).RunAsync();

        output.ToString().ShouldContain(">>> ");
    }

    [Test]
    public async Task RunAsync_ExecutesAndEchoesFirstSubmission()
    {
        using var input = new StringReader("6 * 7;\n");
        using var output = new StringWriter();

        await new ReplSession(input, output).RunAsync();

        output.ToString().ShouldContain("42");
    }

    [Test]
    public async Task RunAsync_PersistsBindingsAndEchoesStandaloneExpressions()
    {
        using var input = new StringReader("rw val total = 40;\ntotal + 2;\n");
        using var output = new StringWriter();

        await new ReplSession(input, output).RunAsync();

        output.ToString().ShouldContain(">>> ");
        output.ToString().ShouldContain("42");
        output.ToString().ShouldNotContain("nil");
    }

    [Test]
    public async Task RunAsync_ContinuesIncompleteBlocksAndExecutesAfterClosingDelimiter()
    {
        using var input = new StringReader("fn answer() {\n  ret 42;\n}\nanswer();\n");
        using var output = new StringWriter();

        await new ReplSession(input, output).RunAsync();

        output.ToString().ShouldContain("... ");
        output.ToString().ShouldContain("42");
    }

    [Test]
    public async Task RunAsync_ReportsSyntaxAndRuntimeErrorsThenAcceptsMoreInput()
    {
        using var input = new StringReader("val = 1;\nthrow \"broken\";\n7;\n");
        using var output = new StringWriter();

        await new ReplSession(input, output).RunAsync();

        output.ToString().ShouldContain("Syntax error:");
        output.ToString().ShouldContain("Runtime error:");
        output.ToString().ShouldContain("7");
    }

    [Test]
    public async Task RunAsync_EofDuringIncompleteInputExitsWithoutAnError()
    {
        using var input = new StringReader("fn unfinished() {");
        using var output = new StringWriter();

        await new ReplSession(input, output).RunAsync();

        output.ToString().ShouldContain("... ");
        output.ToString().ShouldNotContain("Syntax error:");
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task FrontEnd_EntersReplWhenNoScriptOrReplOptionIsProvided(bool useReplOption)
    {
        var originalInput = Console.In;
        var originalOutput = Console.Out;
        using var input = new StringReader(string.Empty);
        using var output = new StringWriter();

        try
        {
            Console.SetIn(input);
            Console.SetOut(output);
            await new EvmFrontEnd().Run(useReplOption ? ["--repl"] : []);
        }
        finally
        {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
        }

        output.ToString().ShouldContain(">>> ");
    }

    [Test]
    public async Task FrontEnd_StillRunsScriptFilesWithoutStartingTheRepl()
    {
        var originalInput = Console.In;
        var originalOutput = Console.Out;
        var scriptPath = Path.Combine(Path.GetTempPath(), $"evil-{Guid.NewGuid():N}.vil");
        await File.WriteAllTextAsync(scriptPath, "result = 42;");
        using var input = new StringReader(string.Empty);
        using var output = new StringWriter();

        try
        {
            Console.SetIn(input);
            Console.SetOut(output);
            await new EvmFrontEnd().Run([scriptPath]);
        }
        finally
        {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
            File.Delete(scriptPath);
        }

        output.ToString().ShouldNotContain(">>> ");
    }
}