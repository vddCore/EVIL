namespace EVIL.Ceres.RuntimeTests;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EVIL.Ceres.ExecutionEngine;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Disassembly;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.Ceres.Runtime;
using EVIL.Ceres.TranslationEngine;
using EVIL.Debugger;
using NUnit.Framework;
using Shouldly;

[TestFixture]
public class DebuggerTests
{
    private static (CeresVM VM, EvilRuntime Runtime, Compiler Compiler) CreateEnvironment()
    {
        var vm = new CeresVM();
        var runtime = new EvilRuntime(vm);
        runtime.RegisterBuiltInModules();
        runtime.RegisterBuiltInFunctions();
        var compiler = new Compiler();
        return (vm, runtime, compiler);
    }

    [Test]
    public async Task Debugger_BreakOnLine_PausesExecutionAtTargetLine()
    {
        var (vm, _, compiler) = CreateEnvironment();
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
        session.OnStopped += (fiber, frame, reason) =>
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
    public async Task Runtime_ExecutionFlow_WithBuiltinFunctions_ExecutesSuccessfully()
    {
        var vm = new CeresVM();
        
        var runtime = new EvilRuntime(vm);
        runtime.RegisterBuiltInModules();
        runtime.RegisterBuiltInFunctions();
        
        var compiler = new Compiler();
        var chunk = compiler.Compile(
            "a = 0; while (a < 1000) { a += 1; } final_a = a;", "test.vil"
        );
        
        vm.MainFiber.Schedule(chunk);
        vm.Run();
        await vm.MainFiber.BlockUntilFinishedAsync();
        vm.Stop();

        vm.Global["final_a"].Number.ShouldBe(1000);
    }

    [Test]
    public async Task Debugger_StepOver_StepsSequentialLines()
    {
        var (vm, _, compiler) = CreateEnvironment();
        var session = new DebuggerSession(vm);

        var source = """

                     val x = 10;
                     val y = 20;
                     val z = 30;

                     """;
        
        var chunk = compiler.Compile(source, "test_step_over.vil");

        var stoppedLines = new List<int>();

        session.AddBreakpoint("test_step_over.vil", 2);
        session.OnStopped += (fiber, frame, reason) =>
        {
            var line = frame.Chunk.DebugDatabase.GetLineForIP((int)frame.IP);
            stoppedLines.Add(line);

            if (stoppedLines.Count < 3)
            {
                session.StepOver();
            }
            else
            {
                session.Continue();
            }
        };

        vm.Run();
        vm.MainFiber.Schedule(chunk);
        await vm.MainFiber.BlockUntilFinishedAsync();
        vm.Stop();

        stoppedLines.Count.ShouldBeGreaterThanOrEqualTo(2);
        stoppedLines[0].ShouldBe(2);
        stoppedLines[1].ShouldBe(3);
    }

    [Test]
    public async Task Debugger_StepInto_DescendsIntoFunctionCall()
    {
        var (vm, _, compiler) = CreateEnvironment();
        var session = new DebuggerSession(vm);

        var source = """

                     fn compute(val1, val2) {
                         val res = val1 + val2;
                         ret res;
                     }

                     val r = compute(5, 10);

                     """;
        
        var chunk = compiler.Compile(source, "test_step_into.vil");

        var stoppedFunctions = new List<string>();

        session.AddBreakpoint("test_step_into.vil", 7);
        session.OnStopped += (fiber, frame, reason) =>
        {
            stoppedFunctions.Add(frame.Chunk.Name ?? "<unknown>");

            if (stoppedFunctions.Count == 1)
            {
                session.StepInto();
            }
            else
            {
                session.Continue();
            }
        };

        vm.Run();
        vm.MainFiber.Schedule(chunk);
        await vm.MainFiber.BlockUntilFinishedAsync();
        vm.Stop();

        stoppedFunctions.Count.ShouldBe(2);
        stoppedFunctions[0].ShouldBe("!_root_chunk!test_step_into.vil");
        stoppedFunctions[1].ShouldBe("compute");
    }

    [Test]
    public async Task Debugger_StepOut_ReturnsToCallerFrame()
    {
        var (vm, _, compiler) = CreateEnvironment();
        var session = new DebuggerSession(vm);

        var source = """

                     fn helper() {
                         val a = 1;
                         val b = 2;
                         ret a + b;
                     }

                     val ans = helper();
                     val after = ans * 2;

                     """;
        
        var chunk = compiler.Compile(source, "test_step_out.vil");

        var stoppedDepths = new List<int>();

        session.AddBreakpoint("test_step_out.vil", 3);
        session.OnStopped += (fiber, frame, reason) =>
        {
            stoppedDepths.Add(fiber.CallStack.Count);

            if (stoppedDepths.Count == 1)
            {
                session.StepOut();
            }
            else
            {
                session.Continue();
            }
        };

        vm.Run();
        vm.MainFiber.Schedule(chunk);
        await vm.MainFiber.BlockUntilFinishedAsync();
        vm.Stop();

        stoppedDepths.Count.ShouldBe(2);
        stoppedDepths[0].ShouldBe(2); // inside helper
        stoppedDepths[1].ShouldBe(1); // returned to root
    }

    [Test]
    public async Task Debugger_ConditionalBreakpoint_BreaksOnlyWhenConditionIsTrue()
    {
        var (vm, _, compiler) = CreateEnvironment();
        var session = new DebuggerSession(vm);

        var source = """

                     for (rw val i = 0; i < 10; i++) {
                         val square = i * i;
                     }

                     """;
        
        var chunk = compiler.Compile(source, "test_cond.vil");
        var hitValues = new List<double>();

        session.AddBreakpoint("test_cond.vil", 3, condition: "i == 7");
        session.OnStopped += (fiber, frame, reason) =>
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
        var (vm, _, compiler) = CreateEnvironment();
        var session = new DebuggerSession(vm);

        var source = """

                     for (rw val i = 0; i < 9; i++) {
                         val temp = i;
                     }

                     """;
        
        var chunk = compiler.Compile(source, "test_hit_cond.vil");
        var hits = 0;

        session.AddBreakpoint("test_hit_cond.vil", 3, hitCondition: "% 3");
        session.OnStopped += (fiber, frame, reason) =>
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
    public async Task Debugger_EvaluateInFrame_InspectsLocalsAndArguments()
    {
        var (vm, _, compiler) = CreateEnvironment();
        var session = new DebuggerSession(vm);

        var source = """

                     fn test_func(x, y) {
                         val z = 100;
                         val res = x + y + z;
                         ret res;
                     }

                     test_func(10, 20);

                     """;
        
        var chunk = compiler.Compile(source, "test_eval.vil");

        DynamicValue evalResult1 = DynamicValue.Nil;
        DynamicValue evalResult2 = DynamicValue.Nil;

        session.AddBreakpoint("test_eval.vil", 4);
        session.OnStopped += (fiber, frame, reason) =>
        {
            evalResult1 = session.EvaluateInFrame(frame, "x + y");
            evalResult2 = session.EvaluateInFrame(frame, "z * 2");
            session.Continue();
        };

        vm.Run();
        vm.MainFiber.Schedule(chunk);
        await vm.MainFiber.BlockUntilFinishedAsync();
        vm.Stop();

        evalResult1.Number.ShouldBe(30);
        evalResult2.Number.ShouldBe(200);
    }

    [Test]
    public async Task Debugger_SetFrameLocal_MutatesVariableAtRuntime()
    {
        var (vm, _, compiler) = CreateEnvironment();
        var session = new DebuggerSession(vm);

        var source = """

                     fn run() {
                         rw val multiplier = 2;
                         val total = 10 * multiplier;
                         ret total;
                     }

                     final_result = run();

                     """;
        
        var chunk = compiler.Compile(source, "test_mutate.vil");

        session.AddBreakpoint("test_mutate.vil", 4);
        session.OnStopped += (fiber, frame, reason) =>
        {
            session.SetFrameLocal(frame, "multiplier", new DynamicValue(5));
            session.Continue();
        };

        vm.Run();
        vm.MainFiber.Schedule(chunk);
        await vm.MainFiber.BlockUntilFinishedAsync();
        vm.Stop();

        var finalResult = vm.Global["final_result"];
        finalResult.Number.ShouldBe(50);
    }

    [Test]
    public async Task Debugger_MultiFiber_EnumeratesFibers()
    {
        var (vm, _, compiler) = CreateEnvironment();
        var session = new DebuggerSession(vm);

        var source = """

                     fn worker() {
                         val a = 1;
                         val b = 2;
                         ret a + b;
                     }

                     val f_res = yield worker();
                     val main_after = 42;

                     """;
        
        var chunk = compiler.Compile(source, "test_fibers.vil");
        int fiberCount = 0;

        session.AddBreakpoint("test_fibers.vil", 9);
        session.OnStopped += (fiber, frame, reason) =>
        {
            fiberCount = session.GetFibers().Count;
            session.Continue();
        };

        vm.Run();
        vm.MainFiber.Schedule(chunk);
        await vm.MainFiber.BlockUntilFinishedAsync();
        vm.Stop();

        fiberCount.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Test]
    public async Task Debugger_AddressBreakpoint_PausesAtExactIP()
    {
        var (vm, _, compiler) = CreateEnvironment();
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
        session.OnStopped += (fiber, frame, reason) =>
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
        var (vm, _, compiler) = CreateEnvironment();
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
        session.OnStopped += (fiber, frame, reason) =>
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
