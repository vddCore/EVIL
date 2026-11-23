namespace EVIL.Ceres.RuntimeTests;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EVIL.Ceres.ExecutionEngine;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.TranslationEngine;
using EVIL.Debugger;
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
                () => vm.Scheduler.IsRunning && vm.Scheduler.IdlePassCount > 0,
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
    
       [Test]
    public async Task Scheduler_PreservesFreshFiberUntilScheduledAndResumed()
    {
        var vm = new CeresVM();
        var chunk = new Compiler().Compile("fn work() -> true;", "scheduler_phase_two.vil")["work"]!;
        var schedulerTask = Task.Run(vm.Scheduler.Run);

        try
        {
            WaitForIdlePass(vm, 2);

            var fiber = vm.Scheduler.CreateFiber(false);
            WaitForIdlePass(vm, 3);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(vm.Scheduler.Fibers.Entries.Values.Contains(fiber), Is.True);
                Assert.That(fiber.State, Is.EqualTo(FiberState.Fresh));
            }

            fiber.Schedule(chunk, false);
            WaitForIdlePass(vm, 4);
            Assert.That(fiber.State, Is.EqualTo(FiberState.Fresh));

            fiber.Resume();
            WaitUntilCollected(vm, fiber);
            Assert.That(fiber.PopValue().Boolean, Is.True);
        }
        finally
        {
            vm.Scheduler.Stop();
            await schedulerTask.WaitAsync(TestTimeout);
        }
    }

    [Test]
    public async Task Scheduler_StopWakesIdleScheduler()
    {
        var vm = new CeresVM();
        var schedulerTask = Task.Run(vm.Scheduler.Run);

        try
        {
            WaitForIdlePass(vm, 1);
            vm.Scheduler.Stop();
            await schedulerTask.WaitAsync(TestTimeout);

            Assert.That(vm.Scheduler.IsRunning, Is.False);
        }
        finally
        {
            vm.Scheduler.Stop();
            await schedulerTask.WaitAsync(TestTimeout);
        }
    }

    [Test]
    public async Task Scheduler_DebuggerStateChangesAndContinueWakeIdleScheduler()
    {
        var vm = new CeresVM();
        var debugger = new DebuggerSession(vm);
        var schedulerTask = Task.Run(vm.Scheduler.Run);

        try
        {
            WaitForIdlePass(vm, 2);

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

    [Test]
    public async Task Scheduler_RechecksAwaitingParentWhenTargetCompletesAfterParking()
    {
        var vm = new CeresVM();
        var chunk = new Compiler().Compile("fn work() -> true;", "scheduler_await_wakeup.vil")["work"]!;
        var parent = vm.Scheduler.CreateFiber(false);
        var target = vm.Scheduler.CreateFiber(false);
        parent.WaitFor(target);
        var schedulerTask = Task.Run(vm.Scheduler.Run);

        try
        {
            WaitForIdlePass(vm, 1);

            target.Schedule(chunk, false);
            WaitForIdlePass(vm, 2);
            Assert.That(target.State, Is.EqualTo(FiberState.Fresh));

            target.Resume();
            WaitUntilCollected(vm, target);
            WaitUntilCollected(vm, parent);

            Assert.That(parent.WaitingFor, Is.Empty);
            Assert.That(target.PopValue().Boolean, Is.True);
        }
        finally
        {
            vm.Scheduler.Stop();
            await schedulerTask.WaitAsync(TestTimeout);
        }
    }

    [Test]
    public async Task Scheduler_PropagatesAwaitedTargetCrashAfterParking()
    {
        var vm = new CeresVM();
        var chunk = new Compiler().Compile("fn work() { throw \"expected\"; }", "scheduler_await_crash.vil")["work"]!;
        Exception? parentCrash = null;
        var parent = vm.Scheduler.CreateFiber(false, (_, exception) => parentCrash = exception);
        var target = vm.Scheduler.CreateFiber(false);
        parent.WaitFor(target);
        var schedulerTask = Task.Run(vm.Scheduler.Run);

        try
        {
            WaitForIdlePass(vm, 1);

            target.Schedule(chunk);
            WaitUntilCollected(vm, target);
            WaitUntilCollected(vm, parent);

            Assert.That(parentCrash, Is.Not.Null);
            Assert.That(parent.WaitingFor, Is.Empty);
        }
        finally
        {
            vm.Scheduler.Stop();
            await schedulerTask.WaitAsync(TestTimeout);
        }
    }

    [Test]
    public async Task Scheduler_KeepsFinishedImmuneFiberRegisteredWhileIdle()
    {
        var vm = new CeresVM();
        var chunk = new Compiler().Compile("fn work() { }", "scheduler_immune.vil")["work"]!;
        var fiber = vm.Scheduler.CreateFiber(true);
        fiber.Schedule(chunk);
        var schedulerTask = Task.Run(vm.Scheduler.Run);

        try
        {
            WaitForIdlePass(vm, 1);

            Assert.That(fiber.State, Is.EqualTo(FiberState.Finished));
            Assert.That(vm.Scheduler.Fibers.Entries.Values.Contains(fiber), Is.True);
        }
        finally
        {
            vm.Scheduler.Stop();
            await schedulerTask.WaitAsync(TestTimeout);
        }
    }

    [Test]
    public async Task Scheduler_ProcessesConcurrentResumesAfterIdle()
    {
        const int fiberCount = 8;
        var vm = new CeresVM();
        var chunk = new Compiler().Compile("fn work() -> true;", "scheduler_concurrent_resume.vil")["work"]!;
        var fibers = new Fiber[fiberCount];

        for (var i = 0; i < fibers.Length; i++)
        {
            fibers[i] = vm.Scheduler.CreateFiber(false);
            fibers[i].Schedule(chunk, false);
        }

        var schedulerTask = Task.Run(vm.Scheduler.Run);

        try
        {
            WaitForIdlePass(vm, 2);
            using var barrier = new Barrier(fiberCount + 1);
            
            var resumeTasks = fibers.Select(fiber => Task.Factory.StartNew(
                () =>
                {
                    // FIXME:
                    // bruh i hope this doesn't get disposed
                    // before it manages to execute.
                    
                    if (!barrier.SignalAndWait(TestTimeout))
                    {
                        throw new TimeoutException("Concurrent fiber resumes did not start together.");
                    }

                    fiber.Resume();
                },
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            )).ToArray();

            if (!barrier.SignalAndWait(TestTimeout))
            {
                throw new TimeoutException("Concurrent fiber resumes did not start together.");
            }

            await Task.WhenAll(resumeTasks).WaitAsync(TestTimeout);

            foreach (var fiber in fibers)
            {
                WaitUntilCollected(vm, fiber);
                Assert.That(fiber.PopValue().Boolean, Is.True);
            }
        }
        finally
        {
            vm.Scheduler.Stop();
            await schedulerTask.WaitAsync(TestTimeout);
        }
    }

    private static void WaitForIdlePass(CeresVM vm, int idlePassCount)
    {
        var idle = SpinWait.SpinUntil(
            () => vm.Scheduler.IdlePassCount >= idlePassCount,
            TestTimeout
        );
        
        Assert.That(idle, Is.True, "The scheduler did not complete an idle pass.");
    }
}