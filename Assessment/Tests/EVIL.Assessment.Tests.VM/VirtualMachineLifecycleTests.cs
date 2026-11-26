namespace EVIL.Assessment.Tests.VM;

using System;
using System.Threading;
using System.Threading.Tasks;
using EVIL.VirtualMachine.ExecutionEngine;
using EVIL.VirtualMachine.Runtime;
using EVIL.VirtualMachine.TranslationEngine;
using NUnit.Framework;
using Shouldly;

[TestFixture]
public class VirtualMachineLifecycleTests
{
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(5);

    [Test]
    public async Task RunAndRunAsync_ConcurrentCallsShareOneRunnerAndStopCompletesIt()
    {
        var vm = new VirtualMachine();
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
    
    [Test]
    public async Task Runtime_ExecutionFlow_WithBuiltinFunctions_ExecutesSuccessfully()
    {
        var vm = new VirtualMachine();

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
}