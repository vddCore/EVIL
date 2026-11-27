namespace EVIL.Assessment.Tests.VM.Debugger;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EVIL.VM.Debugger;
using EVIL.VM.Debugger.Dap;
using EVIL.VM.ExecutionEngine;
using EVIL.VM.ExecutionEngine.Diagnostics;
using EVIL.VM.Runtime;
using EVIL.VM.TranslationEngine;
using NUnit.Framework;
using Shouldly;

[TestFixture]
public class DapServerTests
{
    private static (VirtualMachine VM, DebuggerSession Session, Chunk RootChunk) CreateTestContext(string source, string fileName = "test.vil")
    {
        var vm = new VirtualMachine();
        var runtime = new EvilRuntime(vm);
        runtime.RegisterBuiltInModules();
        runtime.RegisterBuiltInFunctions();

        var compiler = new Compiler();
        var chunk = compiler.Compile(source, fileName);
        var session = new DebuggerSession(vm);

        return (vm, session, chunk);
    }

    private static void WriteDapRequest(Stream stream, string command, object? arguments = null, int seq = 1)
    {
        var reqObj = new
        {
            seq,
            type = "request",
            command,
            arguments
        };
        var json = JsonSerializer.Serialize(reqObj);
        var bytes = Encoding.UTF8.GetBytes(json);
        var header = $"Content-Length: {bytes.Length}\r\n\r\n";
        var headerBytes = Encoding.UTF8.GetBytes(header);

        stream.Write(headerBytes);
        stream.Write(bytes);
    }

    private static List<JsonElement> ReadAllDapMessages(MemoryStream stream)
    {
        stream.Position = 0;
        var reader = new StreamReader(stream, Encoding.UTF8);
        var results = new List<JsonElement>();

        while (true)
        {
            var line = reader.ReadLine();
            if (line == null) break;
            if (!line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase)) continue;

            var len = int.Parse(line.Split(':')[1].Trim());
            reader.ReadLine(); // blank line separator

            var buffer = new char[len];
            var read = 0;
            while (read < len)
            {
                var n = reader.Read(buffer, read, len - read);
                if (n == 0) break;
                read += n;
            }

            var json = new string(buffer, 0, read);
            results.Add(JsonDocument.Parse(json).RootElement);
        }

        return results;
    }

    [Test]
    public async Task DapServer_Initialize_AdvertisesDisassemblyAndInstructionBreakpoints()
    {
        var (vm, session, rootChunk) = CreateTestContext("val a = 1;");
        using var inStream = new MemoryStream();
        using var outStream = new MemoryStream();

        WriteDapRequest(inStream, "initialize", new { adapterID = "evil-debug" });
        WriteDapRequest(inStream, "disconnect", seq: 2);
        inStream.Position = 0;

        var server = new DapServer(inStream, outStream, vm, session, rootChunk);
        await server.RunAsync();

        var messages = ReadAllDapMessages(outStream);
        var initResp = messages.Find(m => m.TryGetProperty("command", out var cmd) && cmd.GetString() == "initialize");
        initResp.ValueKind.ShouldNotBe(JsonValueKind.Undefined);
        initResp.GetProperty("success").GetBoolean().ShouldBeTrue();

        var body = initResp.GetProperty("body");
        body.GetProperty("supportsDisassembleRequest").GetBoolean().ShouldBeTrue();
        body.GetProperty("supportsInstructionBreakpoints").GetBoolean().ShouldBeTrue();
        body.GetProperty("supportsSteppingGranularity").GetBoolean().ShouldBeTrue();
    }

    [Test]
    public async Task DapServer_Disassemble_ReturnsDecodedBytecodeWithSourceMapping()
    {
        var source = @"
val x = 10;
val y = 20;
val z = x + y;
";
        var (vm, session, rootChunk) = CreateTestContext(source, "math_test.vil");
        using var inStream = new MemoryStream();
        using var outStream = new MemoryStream();

        WriteDapRequest(inStream, "disassemble", new
        {
            memoryReference = "0x0000",
            instructionCount = 10,
            resolveSymbols = true
        }, seq: 1);
        WriteDapRequest(inStream, "disconnect", seq: 2);
        inStream.Position = 0;

        var server = new DapServer(inStream, outStream, vm, session, rootChunk);
        await server.RunAsync();

        var messages = ReadAllDapMessages(outStream);
        var disResp = messages.Find(m => m.TryGetProperty("command", out var cmd) && cmd.GetString() == "disassemble");
        disResp.ValueKind.ShouldNotBe(JsonValueKind.Undefined);
        disResp.GetProperty("success").GetBoolean().ShouldBeTrue();

        var instructions = disResp.GetProperty("body").GetProperty("instructions");
        instructions.GetArrayLength().ShouldBeGreaterThan(0);

        var first = instructions[0];
        first.GetProperty("address").GetString().ShouldBe("0x0000");
        first.GetProperty("instructionBytes").GetString().ShouldNotBeNullOrEmpty();
        first.GetProperty("instruction").GetString().ShouldNotBeNullOrEmpty();

        // Source mapping verification
        first.TryGetProperty("location", out var loc).ShouldBeTrue();
        loc.GetProperty("name").GetString().ShouldBe("math_test.vil");
        loc.GetProperty("path").GetString().ShouldBe("math_test.vil");
        first.GetProperty("line").GetInt32().ShouldBe(2);
    }

    [Test]
    public async Task DapServer_Disassemble_WithInstructionOffsetAndCount_ReturnsWindowedSubset()
    {
        var source = @"
val a = 1;
val b = 2;
val c = 3;
val d = 4;
val e = 5;
";
        var (vm, session, rootChunk) = CreateTestContext(source, "window_test.vil");
        using var inStream = new MemoryStream();
        using var outStream = new MemoryStream();

        WriteDapRequest(inStream, "disassemble", new
        {
            memoryReference = "0x0000",
            instructionOffset = 2,
            instructionCount = 2,
            resolveSymbols = true
        }, seq: 1);
        WriteDapRequest(inStream, "disconnect", seq: 2);
        inStream.Position = 0;

        var server = new DapServer(inStream, outStream, vm, session, rootChunk);
        await server.RunAsync();

        var messages = ReadAllDapMessages(outStream);
        var disResp = messages.Find(m => m.TryGetProperty("command", out var cmd) && cmd.GetString() == "disassemble");
        disResp.GetProperty("success").GetBoolean().ShouldBeTrue();

        var instructions = disResp.GetProperty("body").GetProperty("instructions");
        instructions.GetArrayLength().ShouldBe(2);
    }

    [Test]
    public async Task DapServer_Disassemble_SubchunkFunction_ReturnsSubchunkInstructions()
    {
        var source = @"
fn add(x, y) {
    ret x + y;
}

val res = add(10, 20);
";
        var (vm, session, rootChunk) = CreateTestContext(source, "subchunk_test.vil");
        using var inStream = new MemoryStream();
        using var outStream = new MemoryStream();

        // Query disassembly targeting subchunk "add"
        WriteDapRequest(inStream, "disassemble", new
        {
            memoryReference = "add",
            instructionCount = 10,
            resolveSymbols = true
        }, seq: 1);
        WriteDapRequest(inStream, "disconnect", seq: 2);
        inStream.Position = 0;

        var server = new DapServer(inStream, outStream, vm, session, rootChunk);
        await server.RunAsync();

        var messages = ReadAllDapMessages(outStream);
        var disResp = messages.Find(m => m.TryGetProperty("command", out var cmd) && cmd.GetString() == "disassemble");
        disResp.GetProperty("success").GetBoolean().ShouldBeTrue();

        var instructions = disResp.GetProperty("body").GetProperty("instructions");
        instructions.GetArrayLength().ShouldBeGreaterThan(0);

        // First instruction should have symbol "add"
        var first = instructions[0];
        first.GetProperty("symbol").GetString().ShouldBe("add");
    }

    [Test]
    public async Task DapServer_SetInstructionBreakpoints_BindsAddressBreakpoint()
    {
        var source = @"
val a = 100;
val b = 200;
val c = a + b;
";
        var (vm, session, rootChunk) = CreateTestContext(source, "inst_bp_test.vil");
        using var inStream = new MemoryStream();
        using var outStream = new MemoryStream();

        WriteDapRequest(inStream, "setInstructionBreakpoints", new
        {
            breakpoints = new[]
            {
                new
                {
                    instructionReference = "0x0000",
                    offset = 0,
                    condition = "a == 100"
                }
            }
        }, seq: 1);
        WriteDapRequest(inStream, "disconnect", seq: 2);
        inStream.Position = 0;

        var server = new DapServer(inStream, outStream, vm, session, rootChunk);
        await server.RunAsync();

        var messages = ReadAllDapMessages(outStream);
        var bpResp = messages.Find(m => m.TryGetProperty("command", out var cmd) && cmd.GetString() == "setInstructionBreakpoints");
        bpResp.GetProperty("success").GetBoolean().ShouldBeTrue();

        var breakpoints = bpResp.GetProperty("body").GetProperty("breakpoints");
        breakpoints.GetArrayLength().ShouldBe(1);
        breakpoints[0].GetProperty("verified").GetBoolean().ShouldBeTrue();
        breakpoints[0].GetProperty("instructionReference").GetString().ShouldBe("0x0000");
        breakpoints[0].GetProperty("id").GetInt32().ShouldBeGreaterThan(0);
    }

    [Test]
    public async Task DapServer_GranularStepping_HandlesInstructionGranularity()
    {
        var (vm, session, rootChunk) = CreateTestContext("val a = 1;");
        using var inStream = new MemoryStream();
        using var outStream = new MemoryStream();

        WriteDapRequest(inStream, "stepIn", new { granularity = "instruction" }, seq: 1);
        WriteDapRequest(inStream, "next", new { granularity = "instruction" }, seq: 2);
        WriteDapRequest(inStream, "disconnect", seq: 3);
        inStream.Position = 0;

        var server = new DapServer(inStream, outStream, vm, session, rootChunk);
        await server.RunAsync();

        var messages = ReadAllDapMessages(outStream);
        var stepInResp = messages.Find(m => m.TryGetProperty("command", out var cmd) && cmd.GetString() == "stepIn");
        stepInResp.GetProperty("success").GetBoolean().ShouldBeTrue();

        var nextResp = messages.Find(m => m.TryGetProperty("command", out var cmd) && cmd.GetString() == "next");
        nextResp.GetProperty("success").GetBoolean().ShouldBeTrue();
    }
}
