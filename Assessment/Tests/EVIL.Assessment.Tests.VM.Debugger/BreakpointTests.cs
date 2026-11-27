namespace EVIL.Assessment.Tests.VM.Debugger;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EVIL.Assessment.Tests.VM.Debugger.Utilities;
using EVIL.VM.Debugger;
using EVIL.VM.ExecutionEngine.Diagnostics.Disassembly;
using NUnit.Framework;
using Shouldly;

[TestFixture]
public class BreakpointTests
{
    [Test]
    public async Task Debugger_BreakOnLine_PausesExecutionAtTargetLine()
    {
        var (vm, _, compiler) = DebuggerTestEnvironment.Create();
        var session = new DebuggerSession(vm);

        var source = """

                     val a = 1;
                     val b = 2;
                     val c = 3;
                     val d = a + b + c;

                     """;

        var chunk = compiler.Compile(source, "test_break.vil");

        var hitCount = 0;
        int hitLine = -1;

        session.AddBreakpoint("test_break.vil", 4);
        session.OnStopped += (_, frame, _) =>
        {
            hitCount++;
            hitLine = frame.Chunk.DebugDatabase.GetLineForIP((int)frame.IP);
            session.Continue();
        };

        vm.Run();
        vm.MainFiber.Schedule(chunk);
        await vm.MainFiber.BlockUntilFinishedAsync();
        vm.Stop();

        hitCount.ShouldBe(1);
        hitLine.ShouldBe(4);
    }

    [Test]
    public async Task Debugger_ConditionalBreakpoint_BreaksOnlyWhenConditionIsTrue()
    {
        var (vm, _, compiler) = DebuggerTestEnvironment.Create();
        var session = new DebuggerSession(vm);

        var source = """

                     for (rw val i = 0; i < 10; i++) {
                         val square = i * i;
                     }

                     """;

        var chunk = compiler.Compile(source, "test_cond.vil");
        var hitValues = new List<double>();

        session.AddBreakpoint("test_cond.vil", 3, condition: "i == 7");
        session.OnStopped += (_, frame, _) =>
        {
            var iVal = session.EvaluateInFrame(frame, "i");
            hitValues.Add(iVal.Number);
            session.Continue();
        };

        vm.Run();
        vm.MainFiber.Schedule(chunk);
        await vm.MainFiber.BlockUntilFinishedAsync();
        vm.Stop();

        hitValues.Count.ShouldBe(1);
        hitValues[0].ShouldBe(7);
    }

    [Test]
    public async Task Debugger_HitCondition_Modulo_BreaksOnEveryNthHit()
    {
        var (vm, _, compiler) = DebuggerTestEnvironment.Create();
        var session = new DebuggerSession(vm);

        var source = """

                     for (rw val i = 0; i < 9; i++) {
                         val temp = i;
                     }

                     """;

        var chunk = compiler.Compile(source, "test_hit_cond.vil");
        var hits = 0;

        session.AddBreakpoint("test_hit_cond.vil", 3, hitCondition: "% 3");
        session.OnStopped += (_, _, _) =>
        {
            hits++;
            session.Continue();
        };

        vm.Run();
        vm.MainFiber.Schedule(chunk);
        await vm.MainFiber.BlockUntilFinishedAsync();
        vm.Stop();

        hits.ShouldBe(3); // 3rd, 6th, 9th iteration
    }

    [Test]
    public async Task Debugger_AddressBreakpoint_PausesAtExactIP()
    {
        var (vm, _, compiler) = DebuggerTestEnvironment.Create();
        var session = new DebuggerSession(vm);

        var source = """

                     val a = 10;
                     val b = 20;
                     val c = a + b;

                     """;

        var chunk = compiler.Compile(source, "test_addr_break.vil");
        var disasm = chunk.Disassemble();
        disasm.Instructions.Count.ShouldBeGreaterThan(1);
        var targetInstr = disasm.Instructions[1];

        var hitCount = 0;
        int hitIP = -1;

        session.AddAddressBreakpoint(chunk, targetInstr.Offset);
        session.OnStopped += (_, frame, _) =>
        {
            hitCount++;
            hitIP = (int)frame.IP;
            session.Continue();
        };

        vm.Run();
        vm.MainFiber.Schedule(chunk);
        await vm.MainFiber.BlockUntilFinishedAsync();
        vm.Stop();

        hitCount.ShouldBe(1);
        hitIP.ShouldBe(targetInstr.Offset);
    }

    [Test]
    public async Task Debugger_AddressBreakpoint_WithCondition_EvaluatesConditionCorrectly()
    {
        var (vm, _, compiler) = DebuggerTestEnvironment.Create();
        var session = new DebuggerSession(vm);

        var source = """

                     rw val x = 0;
                     while (x < 5) {
                         x = x + 1;
                     }

                     """;

        var chunk = compiler.Compile(source, "test_addr_cond.vil");
        var disasm = chunk.Disassemble();

        var loopInstr = disasm.Instructions.FirstOrDefault(i => i.Line == 4);
        loopInstr.ShouldNotBeNull();

        var hitValues = new List<double>();

        session.AddAddressBreakpoint(chunk, loopInstr.Offset, condition: "x == 3");
        session.OnStopped += (_, frame, _) =>
        {
            var val = session.EvaluateInFrame(frame, "x");
            hitValues.Add(val.Number);
            session.Continue();
        };

        vm.Run();
        vm.MainFiber.Schedule(chunk);
        await vm.MainFiber.BlockUntilFinishedAsync();
        vm.Stop();

        hitValues.Count.ShouldBe(1);
        hitValues[0].ShouldBe(3);
    }

}
