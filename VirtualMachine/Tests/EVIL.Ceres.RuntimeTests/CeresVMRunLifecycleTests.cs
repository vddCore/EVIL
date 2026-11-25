namespace EVIL.Ceres.RuntimeTests;

using System;
using System.Threading;
using System.Threading.Tasks;
using EVIL.Ceres.ExecutionEngine;
using NUnit.Framework;

[TestFixture]
public class CeresVMRunLifecycleTests
{
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(5);

    [Test]
    public async Task RunAndRunAsync_ConcurrentCallsShareOneRunnerAndStopCompletesIt()
    {
        var vm = new CeresVM();
        using var startBarrier = new Barrier(3);
        var runEntryTask = Task.Factory.StartNew(
            () =>
            {
                if (!startBarrier.SignalAndWait(TestTimeout))
                {
                    throw new TimeoutException("Run entry did not start concurrently.");
                }

                vm.Run();
            },
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default
        );
        
        var runAsyncEntryTask = Task.Factory.StartNew(
            () =>
            {
                if (!startBarrier.SignalAndWait(TestTimeout))
                {
                    throw new TimeoutException("RunAsync entry did not start concurrently.");
                }

                return vm.RunAsync();
            },
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default
        );

        if (!startBarrier.SignalAndWait(TestTimeout))
        {
            throw new TimeoutException("VM runner entry points did not start together.");
        }

        await Task.WhenAll(runEntryTask, runAsyncEntryTask).WaitAsync(TestTimeout);
        var runAsyncTask = await runAsyncEntryTask;
        Assert.That(vm.IsRunning, Is.True);

        vm.Run();
        var duplicateRunAsync = vm.RunAsync();
        await duplicateRunAsync.WaitAsync(TestTimeout);

        vm.Stop();
        await runAsyncTask.WaitAsync(TestTimeout);
        Assert.That(SpinWait.SpinUntil(() => !vm.IsRunning, TestTimeout), Is.True);
        Assert.That(vm.Scheduler.IsRunning, Is.False);

        var restartedTask = vm.RunAsync();
        Assert.That(vm.IsRunning, Is.True);
        vm.Stop();
        await restartedTask.WaitAsync(TestTimeout);
        Assert.That(vm.IsRunning, Is.False);
    }
}