namespace EVIL.Assessment.Tests.VM.Debugger;

using System;
using System.Threading;
using System.Threading.Tasks;
using EVIL.VirtualMachine.ExecutionEngine;
using EVIL.VirtualMachine.Debugger;
using NUnit.Framework;

[TestFixture]
public class DebuggerSchedulerIntegrationTests
{
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(5);

    [Test]
    public async Task Scheduler_DebuggerStateChangesAndContinueWakeIdleScheduler()
    {
        var vm = new VirtualMachine();
        var debugger = new DebuggerSession(vm);
        var schedulerTask = Task.Run(vm.Scheduler.Run);

        try
        {
            WaitForIdlePass(vm, 1);

            var idlePass = vm.Scheduler.IdlePassCount + 1;
            debugger.IsAllFibersFrozen = false;
            WaitForIdlePass(vm, idlePass);

            idlePass = vm.Scheduler.IdlePassCount + 1;
            debugger.ActiveFiber = vm.MainFiber;
            WaitForIdlePass(vm, idlePass);

            idlePass = vm.Scheduler.IdlePassCount + 1;
            debugger.Continue();
            WaitForIdlePass(vm, idlePass);
        }
        finally
        {
            vm.Scheduler.Stop();
            await schedulerTask.WaitAsync(TestTimeout);
        }
    }

    private static void WaitForIdlePass(VirtualMachine vm, int idlePassCount)
    {
        var idle = SpinWait.SpinUntil(
            () => vm.Scheduler.IdlePassCount >= idlePassCount,
            TestTimeout
        );

        Assert.That(idle, Is.True, "The scheduler did not complete an idle pass.");
    }
}
