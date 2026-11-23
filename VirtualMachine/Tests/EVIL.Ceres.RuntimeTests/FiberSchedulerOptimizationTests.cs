namespace EVIL.Ceres.RuntimeTests;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EVIL.Ceres.ExecutionEngine;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.TranslationEngine;
using NUnit.Framework;

[TestFixture]
public class FiberSchedulerOptimizationTests
{
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(5);

    [Test]
    public async Task Scheduler_ProcessesResumedFiberAfterIdleAndPreservesDeferredScheduling()
    {
        var vm = new CeresVM();
        Exception? fiberCrash = null;
        var fiber = vm.Scheduler.CreateFiber(false, (_, exception) => fiberCrash = exception);
        fiber.Resume();
        fiber.Yield();
        var chunk = new Compiler().Compile("fn test() { }", "scheduler_resume.vil");

        await WithRunningScheduler(vm, () =>
        {
            fiber.Schedule(chunk["test"]!, false);

            Assert.That(fiber.State, Is.EqualTo(FiberState.Paused));

            fiber.Resume();
            WaitUntilCollected(vm, fiber);

            Assert.That(fiberCrash, Is.Null);
            return Task.CompletedTask;
        });
    }

    [Test]
    public async Task Scheduler_ResumesAwaitingFiberAndRemovesCompletedDependency()
    {
        var vm = new CeresVM();
        var childChunk = new Compiler().Compile("fn child() -> true;", "scheduler_await.vil");
        var parent = vm.Scheduler.CreateFiber(false);
        var child = vm.Scheduler.CreateFiber(false);
        parent.WaitFor(child);
        child.Schedule(childChunk["child"]!);

        await WithRunningScheduler(vm, () =>
        {
            WaitUntilCollected(vm, child);
            WaitUntilCollected(vm, parent);

            Assert.That(parent.WaitingFor, Is.Empty);
            Assert.That(child.PopValue().Boolean, Is.True);
            return Task.CompletedTask;
        });
    }

    [Test]
    public async Task Scheduler_PropagatesYieldedChildCrashToParent()
    {
        var vm = new CeresVM();
        var compiler = new Compiler();
        var chunk = compiler.Compile(
            "fn child() { throw \"intentional_err\"; } fn parent() { yield child(); }",
            "scheduler_crash.vil"
        );
        var parent = vm.Scheduler.CreateFiber(false);
        Exception? parentCrash = null;
        parent.SetCrashHandler((_, exception) => parentCrash = exception);
        parent.Schedule(chunk);
        parent.Schedule(chunk["parent"]!, false);
        parent.Resume();

        await WithRunningScheduler(vm, () =>
        {
            WaitUntilCollected(vm, parent);

            Assert.That(parentCrash, Is.Not.Null);
            Assert.That(parent.WaitingFor, Is.Empty);
            return Task.CompletedTask;
        });
    }

    [Test]
    public async Task Scheduler_StopReturnsAfterSchedulerHasNoRunnableWork()
    {
        var vm = new CeresVM();

        await WithRunningScheduler(vm, () => Task.CompletedTask);

        Assert.That(vm.Scheduler.IsRunning, Is.False);
    }

    private static async Task WithRunningScheduler(CeresVM vm, Func<Task> action)
    {
        var schedulerTask = Task.Run(vm.Scheduler.Run);

        try
        {
            var schedulerReady = SpinWait.SpinUntil(
                () => vm.Scheduler.IsRunning && vm.MainFiber.State == FiberState.Finished,
                TestTimeout
            );
            Assert.That(schedulerReady, Is.True, "The scheduler did not complete its initial idle pass.");

            await action();
        }
        finally
        {
            vm.Scheduler.Stop();
            await schedulerTask.WaitAsync(TestTimeout);
        }
    }

    private static void WaitUntilCollected(CeresVM vm, Fiber fiber)
    {
        var collected = SpinWait.SpinUntil(
            () => !vm.Scheduler.Fibers.Entries.Values.Contains(fiber),
            TestTimeout
        );
        
        Assert.That(collected, Is.True, "The terminal fiber was not removed from the registry.");
    }
}