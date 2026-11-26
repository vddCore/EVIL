namespace EVIL.Assessment.Tests.VM.Debugger;

using System;
using System.IO;
using EVIL.VirtualMachine.ExecutionEngine.Diagnostics.Disassembly;
using EVIL.Assessment.Tests.Base;
using EVIL.VirtualMachine.Debugger;
using EVIL.VirtualMachine.Debugger.Cli;
using NUnit.Framework;
using Shouldly;

[TestFixture]
public class EvdbDisassemblyTests : EvilTest
{
    [Test]
    public void Evdb_Disassemble_RelativeRange_0To10_RendersCurrentPlusTenInstructions()
    {
        var script = """
            val a = 1;
            val b = 2;
            val c = 3;
            val d = 4;
            val e = 5;
            val f = 6;
            val g = 7;
            val h = 8;
            val i = 9;
            val j = 10;
            val k = 11;
            val l = 12;
            val m = 13;
            val n = 14;
            val o = 15;
            """;

        var chunk = Compiler.Compile(script, "test_rel_range.vil");
        var disasm = chunk.Disassemble();
        disasm.Instructions.Count.ShouldBeGreaterThan(15);

        var debugger = new DebuggerSession(VM);
        var session = new EvdbSession(VM, debugger, "test_rel_range.vil");

        var centerIdx = 3;
        var frame = VM.MainFiber.RentScriptFrame(chunk);
        frame.JumpAbsolute(disasm.Instructions[centerIdx].Offset);
        session.ActiveFrame = frame;

        var sw = new StringWriter();
        session.HandleDisassemble("r 0:10", sw);
        var output = sw.ToString();

        output.ShouldContain($"=>  0x{disasm.Instructions[centerIdx].Offset:X8}:");

        output.ShouldContain($"0x{disasm.Instructions[centerIdx].Offset:X8}:");
        output.ShouldContain($"0x{disasm.Instructions[centerIdx + 10].Offset:X8}:");

        output.ShouldNotContain($"0x{disasm.Instructions[centerIdx - 1].Offset:X8}:");
        output.ShouldNotContain($"0x{disasm.Instructions[centerIdx + 11].Offset:X8}:");
    }

    [Test]
    public void Evdb_Disassemble_RelativeRange_Neg10To10_RendersTenBeforeAndAfter()
    {
        var script = """
            val a = 1;
            val b = 2;
            val c = 3;
            val d = 4;
            val e = 5;
            val f = 6;
            val g = 7;
            val h = 8;
            val i = 9;
            val j = 10;
            val k = 11;
            val l = 12;
            val m = 13;
            val n = 14;
            val o = 15;
            val p = 16;
            val q = 17;
            """;

        var chunk = Compiler.Compile(script, "test_rel_neg.vil");
        var disasm = chunk.Disassemble();
        disasm.Instructions.Count.ShouldBeGreaterThan(22);

        var debugger = new DebuggerSession(VM);
        var session = new EvdbSession(VM, debugger, "test_rel_neg.vil");

        var centerIdx = 11;
        var frame = VM.MainFiber.RentScriptFrame(chunk);
        frame.JumpAbsolute(disasm.Instructions[centerIdx].Offset);
        session.ActiveFrame = frame;

        var sw = new StringWriter();
        session.HandleDisassemble("r -10:10", sw);
        var output = sw.ToString();

        output.ShouldContain($"=>  0x{disasm.Instructions[centerIdx].Offset:X8}:");

        output.ShouldContain($"0x{disasm.Instructions[centerIdx - 10].Offset:X8}:");
        output.ShouldContain($"0x{disasm.Instructions[centerIdx + 10].Offset:X8}:");

        output.ShouldNotContain($"0x{disasm.Instructions[0].Offset:X8}:");
        if (disasm.Instructions.Count > centerIdx + 11)
        {
            output.ShouldNotContain($"0x{disasm.Instructions[centerIdx + 11].Offset:X8}:");
        }
    }

    [Test]
    public void Evdb_Disassemble_MultipleFlags_AllAndMixed_RendersAllInstructionsWithSource()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_mixed_{Guid.NewGuid():N}.vil");
        try
        {
            var source = "val foo = 42;\nval bar = foo + 1;\nval baz = bar * 2;\n";
            File.WriteAllText(tempFile, source);

            var chunk = Compiler.Compile(source, tempFile);
            var debugger = new DebuggerSession(VM);
            var session = new EvdbSession(VM, debugger, tempFile);

            var frame = VM.MainFiber.RentScriptFrame(chunk);
            session.ActiveFrame = frame;

            var sw = new StringWriter();
            session.HandleDisassemble("-a -m", sw);
            var output = sw.ToString();

            var disasm = chunk.Disassemble();
            foreach (var inst in disasm.Instructions)
            {
                output.ShouldContain($"0x{inst.Offset:X8}:");
            }

            output.ShouldContain("; line 1: val foo = 42;");
            output.ShouldContain("; line 2: val bar = foo + 1;");
            output.ShouldContain("; line 3: val baz = bar * 2;");
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Test]
    public void Evdb_Disassemble_BundledFlags_Am_RendersAllWithMixedSource()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_bundled_am_{Guid.NewGuid():N}.vil");
        try
        {
            var source = "val x = 100;\nval y = x + 200;\n";
            File.WriteAllText(tempFile, source);

            var chunk = Compiler.Compile(source, tempFile);
            var debugger = new DebuggerSession(VM);
            var session = new EvdbSession(VM, debugger, tempFile);

            var frame = VM.MainFiber.RentScriptFrame(chunk);
            session.ActiveFrame = frame;

            var sw = new StringWriter();
            session.HandleDisassemble("-am", sw);
            var output = sw.ToString();

            var disasm = chunk.Disassemble();
            foreach (var inst in disasm.Instructions)
            {
                output.ShouldContain($"0x{inst.Offset:X8}:");
            }

            output.ShouldContain("; line 1: val x = 100;");
            output.ShouldContain("; line 2: val y = x + 200;");
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Test]
    public void Evdb_Disassemble_BundledFlags_Abm_RendersAllBytesAndMixed()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_bundled_abm_{Guid.NewGuid():N}.vil");
        try
        {
            var source = "val alpha = 10;\nval beta = 20;\n";
            File.WriteAllText(tempFile, source);

            var chunk = Compiler.Compile(source, tempFile);
            var debugger = new DebuggerSession(VM);
            var session = new EvdbSession(VM, debugger, tempFile);

            var frame = VM.MainFiber.RentScriptFrame(chunk);
            session.ActiveFrame = frame;

            var sw = new StringWriter();
            session.HandleDisassemble("-abm", sw);
            var output = sw.ToString();

            output.ShouldContain("; line 1: val alpha = 10;");
            output.ShouldContain("0x00000000:");
            output.ShouldContain("; line 2: val beta = 20;");
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [TestCase("r -5:5")]
    [TestCase("range -5:5")]
    [TestCase("r:-5:5")]
    [TestCase("range:-5:5")]
    public void Evdb_Disassemble_RelativeRangeSyntaxVariations_RenderExpectedRange(string rangeSyntax)
    {
        var script = """
            val a = 1; val b = 2; val c = 3; val d = 4; val e = 5;
            val f = 6; val g = 7; val h = 8; val i = 9; val j = 10;
            val k = 11; val l = 12; val m = 13; val n = 14; val o = 15;
            """;

        var chunk = Compiler.Compile(script, "test_syntax.vil");
        var disasm = chunk.Disassemble();

        var debugger = new DebuggerSession(VM);
        var session = new EvdbSession(VM, debugger, "test_syntax.vil");

        var centerIdx = 7;
        var frame = VM.MainFiber.RentScriptFrame(chunk);
        frame.JumpAbsolute(disasm.Instructions[centerIdx].Offset);
        session.ActiveFrame = frame;

        var sw = new StringWriter();
        session.HandleDisassemble(rangeSyntax, sw);
        var output = sw.ToString();

        output.ShouldContain($"=>  0x{disasm.Instructions[centerIdx].Offset:X8}:");
        output.ShouldContain($"0x{disasm.Instructions[centerIdx - 5].Offset:X8}:");
        output.ShouldContain($"0x{disasm.Instructions[centerIdx + 5].Offset:X8}:");
    }

    [TestCase("help")]
    [TestCase("-h")]
    [TestCase("--help")]
    public void Evdb_Disassemble_HelpFlag_ShowsDisassembleHelp(string helpArg)
    {
        var chunk = Compiler.Compile("val a = 1;", "test_help.vil");
        var debugger = new DebuggerSession(VM);
        var session = new EvdbSession(VM, debugger, "test_help.vil");
        session.ActiveFrame = VM.MainFiber.RentScriptFrame(chunk);

        var sw = new StringWriter();
        session.HandleDisassemble(helpArg, sw);
        var output = sw.ToString();

        output.ShouldContain("Command: dis, disasm");
        output.ShouldContain("Relative Range Windowing:");
        output.ShouldContain("r <from>:<to>");
    }

    [TestCase("dis")]
    [TestCase("disasm")]
    [TestCase("disassemble")]
    [TestCase("break")]
    [TestCase("b")]
    [TestCase("breakpoint")]
    [TestCase("delete")]
    [TestCase("d")]
    [TestCase("breakpoints")]
    [TestCase("bps")]
    [TestCase("continue")]
    [TestCase("c")]
    [TestCase("cont")]
    [TestCase("step")]
    [TestCase("s")]
    [TestCase("next")]
    [TestCase("n")]
    [TestCase("finish")]
    [TestCase("fin")]
    [TestCase("stepi")]
    [TestCase("si")]
    [TestCase("print")]
    [TestCase("p")]
    [TestCase("set")]
    [TestCase("backtrace")]
    [TestCase("bt")]
    [TestCase("where")]
    [TestCase("frame")]
    [TestCase("f")]
    [TestCase("loc")]
    [TestCase("locals")]
    [TestCase("args")]
    [TestCase("fibers")]
    [TestCase("fiber")]
    [TestCase("info")]
    [TestCase("list")]
    [TestCase("l")]
    [TestCase("quit")]
    [TestCase("q")]
    [TestCase("exit")]
    [TestCase("help")]
    [TestCase("h")]
    [TestCase("?")]
    public void Evdb_Help_Subcommand_ProvidesDetailedHelp(string subcmd)
    {
        var sw = new StringWriter();
        EvdbSession.HandleHelp(subcmd, sw);
        var output = sw.ToString();

        output.ShouldNotBeNullOrWhiteSpace();
        output.ShouldNotContain("Unknown command");
        if (subcmd is not "help" and not "h" and not "?")
        {
            output.ShouldContain("Command:");
        }
        else
        {
            output.ShouldContain("EVIL Command-Line Debugger (evdb)");
        }
    }

    [Test]
    public void Evdb_Help_NoArgs_ProvidesGeneralHelp()
    {
        var sw = new StringWriter();
        EvdbSession.HandleHelp("", sw);
        var output = sw.ToString();

        output.ShouldContain("EVIL Command-Line Debugger (evdb)");
        output.ShouldContain("Execution:");
        output.ShouldContain("Breakpoints:");
        output.ShouldContain("Inspection:");
        output.ShouldContain("Type 'help <command>' for detailed help on any command");
    }
}
