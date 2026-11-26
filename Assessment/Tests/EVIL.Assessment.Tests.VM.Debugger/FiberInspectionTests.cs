namespace EVIL.Assessment.Tests.VM.Debugger;

using System.Threading.Tasks;
using EVIL.Assessment.Tests.VM.Debugger.Utilities;
using EVIL.VirtualMachine.Debugger;
using NUnit.Framework;
using Shouldly;

[TestFixture]
public class FiberInspectionTests
{
    [Test]
    public async Task Debugger_MultiFiber_EnumeratesFibers()
    {
        var (vm, _, compiler) = DebuggerTestEnvironment.Create();
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
        session.OnStopped += (_, _, _) =>
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

}
