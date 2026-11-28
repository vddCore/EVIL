namespace EVIL.Assessment.Tests.FE;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EVIL.FE;
using EVIL.FE.REPL;
using EVIL.FE.REPL.SyntaxHighlighting;
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
    public async Task RunAsync_HighlightsSyntaxAndInterpolatedExpressionsWhileEditing()
    {
        const string source = "6 * 7;";
        var keys = new Queue<ConsoleKeyInfo>(source
            .Select(Character)
            .Append(Special(ConsoleKey.Enter))
            .Append(new ConsoleKeyInfo('\u0004', ConsoleKey.D, false, false, true)));
        using var output = new StringWriter();

        await new ReplSession(TextReader.Null, output, readKey: keys.Dequeue).RunAsync();

        output.ToString().ShouldContain("\u001b[36m6");
        output.ToString().ShouldContain("42");
    }

    [Test]
    public void ReplSyntaxHighlighter_HighlightsInterpolatedExpressionsAsCode()
    {
        const string source = "\"value ${1 + 2}\"";
        var rendered = new SyntaxHighlighter().Render(source, 0, source.Length);

        rendered.ShouldContain("\u001b[32m\"value ");
        rendered.ShouldContain("\u001b[36m1");
        rendered.ShouldContain("\u001b[33m+");
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

    [Test]
    public async Task RunAsync_ExecutesEditedInputAndCanResubmitItFromHistory()
    {
        using var output = new StringWriter();
        var keys = new Queue<ConsoleKeyInfo>([
            Character('6'),
            Character('*'),
            Character('7'),
            Character(';'),
            Special(ConsoleKey.LeftArrow),
            Special(ConsoleKey.LeftArrow),
            Character('2'),
            Special(ConsoleKey.Enter),
            Special(ConsoleKey.UpArrow),
            Special(ConsoleKey.Enter),
            new ConsoleKeyInfo('\u0004', ConsoleKey.D, false, false, true)
        ]);

        await new ReplSession(TextReader.Null, output, readKey: keys.Dequeue).RunAsync();

        output.ToString().Split(Environment.NewLine).Count(line => line == "162").ShouldBe(2);
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

    private static ConsoleKeyInfo Character(char character)
    {
        return new ConsoleKeyInfo(character, ConsoleKey.NoName, false, false, false);
    }

    private static ConsoleKeyInfo Special(ConsoleKey key)
    {
        return new ConsoleKeyInfo('\0', key, false, false, false);
    }
}