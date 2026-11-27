namespace EVIL.Assessment.Tests.VM.Debugger;

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EVIL.Assessment.Tests.Base;
using EVIL.VM.Debugger;
using EVIL.VM.ExecutionEngine;
using EVIL.VM.ExecutionEngine.Diagnostics;
using EVIL.VM.ExecutionEngine.Diagnostics.Debugging;
using EVIL.VM.ExecutionEngine.Diagnostics.Disassembly;
using EVIL.VM.ExecutionEngine.Diagnostics.Disassembly.Rendering;
using EVIL.VM.Runtime;
using EVIL.VM.TranslationEngine;
using NUnit.Framework;

[TestFixture]
public class DisassemblyFormatterTests : EvilTest
{
    private sealed class MockBreakpoint : IBreakpoint
    {
        public int Id { get; init; } = 1;
        public string FilePath { get; init; } = "test.vil";
        public int Line { get; init; } = 1;
        public int Column { get; init; } = 0;
        public bool Enabled { get; set; } = true;
        public bool IsVerified => true;
        public string? Condition => null;
        public string? HitCondition => null;
        public int HitCount => 0;
        public int BoundIP { get; init; } = 0;
        public Chunk? BoundChunk { get; init; }
    }

    [Test]
    public void DisassemblyFormatter_ShowsCanonicalIndexedAndTargetedChunkNames()
    {
        var script = """
            val obj = {};
            loc fn obj.bar() -> 1;
            loc fn obj::baz() -> 2;
            loc fn obj.bar() -> 3;
            """;

        var chunk = Compiler.Compile(script, "generated_chunk_names.vil");
        var sw = new StringWriter();
        DisassemblyFormatter.FormatChunk(
            chunk.Disassemble(),
            sw,
            new DisassemblyFormatOptions
            {
                ShowChunkHeader = true,
                IncludeSubChunks = true,
                ColorMode = ColorMode.Never
            }
        );

        var output = sw.ToString();

        Assert.That(output, Does.Contain(".CHUNK obj.bar"));
        Assert.That(output, Does.Contain(".CHUNK obj::baz"));
        Assert.That(output, Does.Contain("subchunk 0 'obj.bar'"));
        Assert.That(output, Does.Contain("subchunk 1 'obj::baz'"));
    }

    [Test]
    public void DisassemblyFormatter_ShowsReadableRootChunkName()
    {
        var chunk = Compiler.Compile("val value = 1;", "src/disassembly_root.vil");
        var sw = new StringWriter();

        DisassemblyFormatter.FormatChunk(
            chunk.Disassemble(),
            sw,
            new DisassemblyFormatOptions
            {
                ShowChunkHeader = true,
                ColorMode = ColorMode.Never
            }
        );

        Assert.That(sw.ToString(), Does.Contain(".CHUNK !_root_chunk!disassembly_root.vil"));
    }

    [Test]
    public void DisassemblyFormatter_HighlightsActiveIPWithGutterCursor()
    {
        var script = """
            fn test(x) {
                val y = x + 10;
                ret y;
            }
            """;

        var chunk = Compiler.Compile(script, "test_gutter");
        var testChunk = chunk["test"]!;
        var disasm = testChunk.Disassemble();

        var targetInst = disasm.Instructions[2];
        var options = new DisassemblyFormatOptions
        {
            HighlightIP = targetInst.Offset,
            ColorMode = ColorMode.Never
        };

        var sw = new StringWriter();
        DisassemblyFormatter.FormatChunk(disasm, sw, options);
        var output = sw.ToString();

        Assert.That(output, Does.Contain($"=>  0x{targetInst.Offset:X8}:"));
    }

    [Test]
    public void DisassemblyFormatter_RendersBreakpointMarkerInGutterAndAnnotation()
    {
        var script = """
            fn test(x) {
                val y = x * 2;
                ret y;
            }
            """;

        var chunk = Compiler.Compile(script, "test_bp_gutter");
        var testChunk = chunk["test"]!;
        var disasm = testChunk.Disassemble();

        var bpInst = disasm.Instructions[1];
        var bp = new MockBreakpoint
        {
            Id = 42,
            BoundIP = bpInst.Offset,
            BoundChunk = testChunk
        };

        var options = new DisassemblyFormatOptions
        {
            Breakpoints = new List<IBreakpoint> { bp },
            ColorMode = ColorMode.Never
        };

        var sw = new StringWriter();
        DisassemblyFormatter.FormatChunk(disasm, sw, options);
        var output = sw.ToString();

        Assert.That(output, Does.Contain($" *  0x{bpInst.Offset:X8}:"));
        Assert.That(output, Does.Contain("[BP #42]"));
    }

    [Test]
    public void DisassemblyFormatter_RendersBothIPAndBreakpointGutterMarker()
    {
        var script = """
            fn test(x) {
                ret x + 1;
            }
            """;

        var chunk = Compiler.Compile(script, "test_both_gutter");
        var testChunk = chunk["test"]!;
        var disasm = testChunk.Disassemble();

        var inst = disasm.Instructions[0];
        var bp = new MockBreakpoint
        {
            Id = 5,
            BoundIP = inst.Offset,
            BoundChunk = testChunk
        };

        var options = new DisassemblyFormatOptions
        {
            HighlightIP = inst.Offset,
            Breakpoints = new List<IBreakpoint> { bp },
            ColorMode = ColorMode.Never
        };

        var sw = new StringWriter();
        DisassemblyFormatter.FormatChunk(disasm, sw, options);
        var output = sw.ToString();

        Assert.That(output, Does.Contain($"=>* 0x{inst.Offset:X8}:"));
        Assert.That(output, Does.Contain("[BP #5]"));
    }

    [Test]
    public void DisassemblyFormatter_RendersRawHexBytesWhenEnabled()
    {
        var script = """
            fn test() {
                ret 123;
            }
            """;

        var chunk = Compiler.Compile(script, "test_raw_bytes");
        var testChunk = chunk["test"]!;
        var disasm = testChunk.Disassemble();

        var options = new DisassemblyFormatOptions
        {
            ShowRawBytes = true,
            ColorMode = ColorMode.Never
        };

        var sw = new StringWriter();
        DisassemblyFormatter.FormatChunk(disasm, sw, options);
        var output = sw.ToString();

        Assert.That(output, Does.Match(@"[0-9A-Fa-f]{2} "));
    }

    [Test]
    public void DisassemblyFormatter_InterleavesSourceCodeLines()
    {
        var sourceCode = """
            fn test(a, b) {
                val sum = a + b;
                ret sum;
            }
            """;

        var chunk = Compiler.Compile(sourceCode, "sample.vil");
        var testChunk = chunk["test"]!;
        var disasm = testChunk.Disassemble();

        var sourceLines = sourceCode.Split("\n");
        var options = new DisassemblyFormatOptions
        {
            ShowSourceInterleaving = true,
            SourceProvider = _ => sourceLines,
            ColorMode = ColorMode.Never
        };

        var sw = new StringWriter();
        DisassemblyFormatter.FormatChunk(disasm, sw, options);
        var output = sw.ToString();

        Assert.That(output, Does.Contain("; line 2: val sum = a + b;"));
        Assert.That(output, Does.Contain("; line 3: ret sum;"));
    }

    [Test]
    public void DisassemblyFormatter_RendersLabelsAndBranchOffsets()
    {
        var script = """
            fn test(cond) {
                if (cond) {
                    ret 1;
                }
                ret 2;
            }
            """;

        var chunk = Compiler.Compile(script, "test_flow");
        var testChunk = chunk["test"]!;
        var disasm = testChunk.Disassemble();

        var options = new DisassemblyFormatOptions
        {
            ShowBranchLabels = true,
            ShowBranchOffsets = true,
            ColorMode = ColorMode.Never
        };

        var sw = new StringWriter();
        DisassemblyFormatter.FormatChunk(disasm, sw, options);
        var output = sw.ToString();

        Assert.That(output, Does.Match(@"L_[0-9A-Fa-f]{4}:"));
        Assert.That(output, Does.Contain("bytes)"));
    }

    [Test]
    public void DisassemblyFormatter_RendersAnsiColorsWhenActive()
    {
        var script = """
            fn test(x) {
                ret x + 1;
            }
            """;

        var chunk = Compiler.Compile(script, "test_ansi");
        var testChunk = chunk["test"]!;
        var disasm = testChunk.Disassemble();

        var options = new DisassemblyFormatOptions
        {
            HighlightIP = 0,
            ColorMode = ColorMode.Always
        };

        var sw = new StringWriter();
        DisassemblyFormatter.FormatChunk(disasm, sw, options);
        var output = sw.ToString();

        Assert.That(output, Does.Contain("\u001b["));
        Assert.That(output, Does.Contain("\u001b[0m"));
    }

    [Test]
    public async Task DisassemblyFormatter_RendersLiveFrameVariableValues()
    {
        var vm = new VirtualMachine();
        var runtime = new EvilRuntime(vm);
        runtime.RegisterBuiltInModules();
        runtime.RegisterBuiltInFunctions();
        var compiler = new Compiler();

        var session = new DebuggerSession(vm);

        var script = """
            fn test_func() {
                val secret = 42;
                val multiplier = 3;
                val result = secret * multiplier;
                ret result;
            }
            test_func();
            """;

        var chunk = compiler.Compile(script, "live_values.vil");
        ScriptStackFrame? capturedFrame = null;

        string capturedOutput = string.Empty;

        session.AddBreakpoint("live_values.vil", 4); // at val result = ...
        session.OnStopped += (_, frame, _) =>
        {
            capturedFrame = frame;
            var disasm = frame.Chunk.Disassemble();
            var options = new DisassemblyFormatOptions
            {
                HighlightIP = (int)frame.IP,
                FrameContext = frame,
                ShowRuntimeValues = true,
                ColorMode = ColorMode.Never
            };
            var sw = new StringWriter();
            DisassemblyFormatter.FormatChunk(disasm, sw, options);
            capturedOutput = sw.ToString();

            session.Continue();
        };

        vm.Run();
        vm.MainFiber.Schedule(chunk);
        await vm.MainFiber.BlockUntilFinishedAsync();
        vm.Stop();

        Assert.That(capturedFrame, Is.Not.Null);
        Assert.That(capturedOutput, Does.Contain("(secret = 42)"));
    }
}
