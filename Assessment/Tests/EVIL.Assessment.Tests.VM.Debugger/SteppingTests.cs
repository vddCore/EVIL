namespace EVIL.Assessment.Tests.VM.Debugger;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EVIL.Assessment.Tests.VM.Debugger.Utilities;
using EVIL.VirtualMachine.Debugger;
using NUnit.Framework;
using Shouldly;

[TestFixture]
public class SteppingTests
{
    [Test]
    public async Task Debugger_StepOver_StepsSequentialLines()
    {
        var (vm, _, compiler) = DebuggerTestEnvironment.Create();
        var session = new DebuggerSession(vm);

        var source = """

                     val x = 10;
                     val y = 20;
                     val z = 30;

                     """;

        var chunk = compiler.Compile(source, "test_step_over.vil");

        var stoppedLines = new List<int>();

        session.AddBreakpoint("test_step_over.vil", 2);
        session.OnStopped += (_, frame, _) =>
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
    public async Task Debugger_StepInstruction_CrossesSchedulerSliceBoundary()
    {
        const int instructionStepCount = 40;
        var (vm, _, compiler) = DebuggerTestEnvironment.Create();
        var session = new DebuggerSession(vm);
        var source = string.Join(
            "\n",
            Enumerable.Range(0, 48).Select(index => $"val value{index} = {index};")
        );
        var chunk = compiler.Compile(source, "test_step_instruction_slice.vil");
        var instructionPointers = new List<int>();

        session.OnStopped += (_, frame, _) =>
        {
            instructionPointers.Add((int)frame.IP);
            if (instructionPointers.Count < instructionStepCount)
            {
                session.StepInstruction();
            }
            else
            {
                session.Continue();
            }
        };

        session.StepInstruction();
        vm.Run();
        vm.MainFiber.Schedule(chunk);
        await vm.MainFiber.BlockUntilFinishedAsync();
        vm.Stop();

        instructionPointers.Count.ShouldBe(instructionStepCount);
        for (var index = 1; index < instructionPointers.Count; index++)
        {
            instructionPointers[index].ShouldBeGreaterThan(instructionPointers[index - 1]);
        }
    }

    [Test]
    public async Task Debugger_StepInto_DescendsIntoFunctionCall()
    {
        var (vm, _, compiler) = DebuggerTestEnvironment.Create();
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
        session.OnStopped += (_, frame, _) =>
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
        var (vm, _, compiler) = DebuggerTestEnvironment.Create();
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
        session.OnStopped += (fiber, _, _) =>
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

}
