namespace EVIL.Assessment.Tests.VM.Debugger;

using System.Threading.Tasks;
using EVIL.Assessment.Tests.VM.Debugger.Utilities;
using EVIL.VirtualMachine.ExecutionEngine.TypeSystem;
using EVIL.VirtualMachine.Debugger;
using NUnit.Framework;
using Shouldly;

[TestFixture]
public class FrameInspectionTests
{
    [Test]
    public async Task Debugger_EvaluateInFrame_InspectsLocalsAndArguments()
    {
        var (vm, _, compiler) = DebuggerTestEnvironment.Create();
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
        session.OnStopped += (_, frame, _) =>
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
        var (vm, _, compiler) = DebuggerTestEnvironment.Create();
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
        session.OnStopped += (_, frame, _) =>
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

}
