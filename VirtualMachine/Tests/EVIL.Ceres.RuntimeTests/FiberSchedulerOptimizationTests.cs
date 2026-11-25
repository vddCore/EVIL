namespace EVIL.Ceres.RuntimeTests;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EVIL.Ceres.ExecutionEngine;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
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
    public async Task Scheduler_ResolvesWaitForAllForMultipleParentsAndAlreadyFinishedTarget()
    {
        var vm = new CeresVM();
        var chunks = new Compiler().Compile(
            "fn work() -> true; fn parent() -> false;",
            "scheduler_wait_for_all.vil"
        );
        var finishedTarget = vm.Scheduler.CreateFiber(false);
        finishedTarget.Schedule(chunks["work"]!);

        await WithRunningScheduler(vm, async () =>
        {
            WaitUntilCollected(vm, finishedTarget);

            var firstTarget = vm.Scheduler.CreateFiber(false);
            var finalTarget = vm.Scheduler.CreateFiber(false);
            var parents = new[]
            {
                vm.Scheduler.CreateFiber(false),
                vm.Scheduler.CreateFiber(false)
            };

            foreach (var parent in parents)
            {
                parent.Schedule(chunks["parent"]!, false);
                parent.WaitForAll(finishedTarget, firstTarget, finalTarget);
            }

            firstTarget.Schedule(chunks["work"]!);
            WaitUntilCollected(vm, firstTarget);

            foreach (var parent in parents)
            {
                Assert.That(parent.State, Is.EqualTo(FiberState.Awaiting));
                Assert.That(parent.WaitingFor, Is.EquivalentTo(new[] { finalTarget }));
            }

            finalTarget.Schedule(chunks["work"]!);
            WaitUntilCollected(vm, finalTarget);

            foreach (var parent in parents)
            {
                WaitUntilCollected(vm, parent);
                Assert.That(parent.WaitingFor, Is.Empty);
                Assert.That(parent.PopValue().Boolean, Is.False);
            }

            await Task.CompletedTask;
        });
    }

    [Test]
    public async Task Scheduler_ProcessesPendingCrashBeforeFinishingFinalWaitForAllDependency()
    {
        var vm = new CeresVM();
        var chunks = new Compiler().Compile(
            "fn finish() { } fn crash() { throw \"expected\"; } fn parent() -> true;",
            "scheduler_wait_for_all_crash.vil"
        );
        var schedulerThreadId = 0;
        var parentCrashThreadId = 0;
        var parentCrashCalls = 0;
        var parent = vm.Scheduler.CreateFiber(false, (_, _) =>
        {
            Interlocked.Increment(ref parentCrashCalls);
            Volatile.Write(ref parentCrashThreadId, Environment.CurrentManagedThreadId);
        });
        var crashingTarget = vm.Scheduler.CreateFiber(false);
        var finishingTarget = vm.Scheduler.CreateFiber(false);
        parent.Schedule(chunks["parent"]!, false);
        parent.WaitForAll(crashingTarget, finishingTarget);
        crashingTarget.Schedule(chunks["crash"]!);
        finishingTarget.Schedule(chunks["finish"]!);
        var schedulerTask = Task.Run(() =>
        {
            Volatile.Write(ref schedulerThreadId, Environment.CurrentManagedThreadId);
            vm.Scheduler.Run();
        });

        try
        {
            WaitUntilCollected(vm, crashingTarget);
            WaitUntilCollected(vm, finishingTarget);
            WaitUntilCollected(vm, parent);

            Assert.That(parent.State, Is.EqualTo(FiberState.Crashed));
            Assert.That(parent.WaitingFor, Is.Empty);
            Assert.That(parentCrashCalls, Is.EqualTo(1));
            Assert.That(parentCrashThreadId, Is.EqualTo(schedulerThreadId));
        }
        finally
        {
            vm.Scheduler.Stop();
            await schedulerTask.WaitAsync(TestTimeout);
        }
    }

    [Test]
    public async Task Scheduler_StopWaitingForDetachesReverseWaiterEdge()
    {
        var vm = new CeresVM();
        var chunk = new Compiler().Compile(
            "fn work() -> true;",
            "scheduler_stop_waiting.vil"
        )["work"]!;
        var parent = vm.Scheduler.CreateFiber(false);
        var target = vm.Scheduler.CreateFiber(false);
        parent.Schedule(chunk, false);
        parent.WaitFor(target);
        parent.StopWaitingFor(target);
        target.Schedule(chunk);

        Assert.That(parent.State, Is.EqualTo(FiberState.Running));

        await WithRunningScheduler(vm, () =>
        {
            WaitUntilCollected(vm, parent);
            WaitUntilCollected(vm, target);

            Assert.That(parent.WaitingFor, Is.Empty);
            Assert.That(parent.PopValue().Boolean, Is.True);
            return Task.CompletedTask;
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Scheduler_ResolvesWaitRegistrationRacingWithTargetTerminalState(bool targetCrashes)
    {
        const int raceCount = 16;
        var vm = new CeresVM();
        var compiler = new Compiler();
        var chunks = compiler.Compile(
            "fn target() -> true; fn crash() { throw \"expected\"; } fn parent() -> true;",
            "scheduler_wait_registration_race.vil"
        );
        var targetChunk = chunks[targetCrashes ? "crash" : "target"]!;
        var schedulerThreadId = 0;
        var schedulerTask = Task.Run(() =>
        {
            Volatile.Write(ref schedulerThreadId, Environment.CurrentManagedThreadId);
            vm.Scheduler.Run();
        });

        try
        {
            WaitForIdlePass(vm, 1);

            for (var i = 0; i < raceCount; i++)
            {
                var parentCrashCalls = 0;
                var parentCrashThreadId = 0;
                var parent = vm.Scheduler.CreateFiber(false, (_, _) =>
                {
                    Interlocked.Increment(ref parentCrashCalls);
                    Volatile.Write(ref parentCrashThreadId, Environment.CurrentManagedThreadId);
                });
                var target = vm.Scheduler.CreateFiber(false);
                parent.Schedule(chunks["parent"]!, false);
                target.Schedule(targetChunk, false);

                using var start = new Barrier(3);
                var registrationTask = Task.Factory.StartNew(
                    () =>
                    {
                        if (!start.SignalAndWait(TestTimeout))
                        {
                            throw new TimeoutException("Waiter registration did not start with target completion.");
                        }

                        parent.WaitFor(target);
                        parent.Resume();
                    },
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default
                );
                var targetTask = Task.Factory.StartNew(
                    () =>
                    {
                        if (!start.SignalAndWait(TestTimeout))
                        {
                            throw new TimeoutException("Target completion did not start with waiter registration.");
                        }

                        target.Resume();
                    },
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default
                );

                if (!start.SignalAndWait(TestTimeout))
                {
                    throw new TimeoutException("Waiter registration and target completion did not start together.");
                }

                await Task.WhenAll(registrationTask, targetTask).WaitAsync(TestTimeout);
                WaitUntilCollected(vm, target);
                WaitUntilCollected(vm, parent);

                Assert.That(parent.WaitingFor, Is.Empty);
                Assert.That(parentCrashCalls, Is.EqualTo(targetCrashes ? 1 : 0));

                if (targetCrashes)
                {
                    Assert.That(parent.State, Is.EqualTo(FiberState.Crashed));
                    Assert.That(parentCrashThreadId, Is.EqualTo(schedulerThreadId));
                }
                else
                {
                    Assert.That(parent.PopValue().Boolean, Is.True);
                }
            }
        }
        finally
        {
            vm.Scheduler.Stop();
            await schedulerTask.WaitAsync(TestTimeout);
        }
    }

    [Test]
    public async Task Scheduler_UsesStableRoundRobinTurnsAndRegistrySnapshotOrder()
    {
        var vm = new CeresVM();
        var invocationOrder = new ConcurrentQueue<Fiber>();
        vm.Global["record"] = new DynamicValue((NativeFunction)((fiber, _) =>
        {
            invocationOrder.Enqueue(fiber);
            return DynamicValue.Nil;
        }));

        var calls = string.Concat(Enumerable.Repeat("record();", 40));
        var chunk = new Compiler().Compile(
            $"fn first() {{ {calls} }} fn second() {{ {calls} }}",
            "scheduler_round_robin.vil"
        );
        var first = vm.Scheduler.CreateFiber(false);
        var second = vm.Scheduler.CreateFiber(false);
        Assert.That(vm.Scheduler.Fibers.ToArray(), Is.EqualTo(new[] { vm.MainFiber, first, second }));
        first.Schedule(chunk["first"]!, false);
        second.Schedule(chunk["second"]!, false);
        first.Resume();
        second.Resume();

        await WithRunningScheduler(vm, () =>
        {
            WaitUntilCollected(vm, first);
            WaitUntilCollected(vm, second);

            var executionOrder = invocationOrder.ToArray();
            var firstIndex = Array.IndexOf(executionOrder, first);
            var secondIndex = Array.IndexOf(executionOrder, second);
            var nextFirstIndex = Array.FindIndex(executionOrder, secondIndex + 1, fiber => fiber == first);

            Assert.That(firstIndex, Is.EqualTo(0));
            Assert.That(firstIndex, Is.LessThan(secondIndex));
            Assert.That(secondIndex, Is.LessThan(nextFirstIndex));
            return Task.CompletedTask;
        });
    }

    [Test]
    public async Task Scheduler_ProgressesWithManyPausedAndAwaitingFibers()
    {
        const int pausedFiberCount = 512;
        const int waitingFiberCount = 512;
        var vm = new CeresVM();
        var chunk = new Compiler().Compile("fn work() -> true;", "scheduler_many_idle.vil")["work"]!;

        for (var i = 0; i < pausedFiberCount; i++)
        {
            var pausedFiber = vm.Scheduler.CreateFiber(false);
            pausedFiber.Resume();
            pausedFiber.Yield();
        }

        var awaitedFiber = vm.Scheduler.CreateFiber(false);
        awaitedFiber.Resume();
        awaitedFiber.Yield();

        for (var i = 0; i < waitingFiberCount; i++)
        {
            var waitingFiber = vm.Scheduler.CreateFiber(false);
            waitingFiber.WaitFor(awaitedFiber);
        }

        var runnableFiber = vm.Scheduler.CreateFiber(false);
        runnableFiber.Schedule(chunk, false);
        runnableFiber.Resume();

        await WithRunningScheduler(vm, () =>
        {
            WaitUntilCollected(vm, runnableFiber);

            Assert.That(runnableFiber.PopValue().Boolean, Is.True);
            Assert.That(vm.Scheduler.Fibers.Count, Is.EqualTo(pausedFiberCount + waitingFiberCount + 2));
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
    public async Task Scheduler_PropagatesYieldedChildCrashToProtectedParentHandler()
    {
        var vm = new CeresVM();
        var compiler = new Compiler();
        var chunk = compiler.Compile(
            "fn child() { throw \"intentional_err\"; } " +
            "fn parent() { " +
            "try { yield child(); } catch (e) { ret true; } " +
            "ret false; }",
            "scheduler_protected_crash.vil"
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

            Assert.That(parentCrash, Is.Null);
            Assert.That(parent.PopValue().Boolean, Is.True);
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
                () => vm.Scheduler is { IsRunning: true, IdlePassCount: > 0 },
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

        Assert.That(collected, Is.True, $"Fiber {fiber.RegistryId} ({fiber.State}) was not removed.");
    }
    
       [Test]
    public async Task Scheduler_PreservesFreshFiberUntilScheduledAndResumed()
    {
        var vm = new CeresVM();
        var chunk = new Compiler().Compile("fn work() -> true;", "scheduler_phase_two.vil")["work"]!;
        var schedulerTask = Task.Run(vm.Scheduler.Run);

        try
        {
            WaitForIdlePass(vm, 1);

            var fiber = vm.Scheduler.CreateFiber(false);
            WaitForIdlePass(vm, 2);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(vm.Scheduler.Fibers.Entries.Values.Contains(fiber), Is.True);
                Assert.That(fiber.State, Is.EqualTo(FiberState.Fresh));
            }

            fiber.Schedule(chunk, false);
            WaitForIdlePass(vm, 3);
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
    public async Task Scheduler_ContinuesAfterFiberCrashHandlerThrows()
    {
        var vm = new CeresVM();
        var chunk = new Compiler().Compile(
            "fn fail() { throw \"expected\"; } fn succeed() -> true;",
            "scheduler_fiber_failure.vil"
        );
        var crashHandlerCalls = 0;
        var failingFiber = vm.Scheduler.CreateFiber(false, (_, _) =>
        {
            Interlocked.Increment(ref crashHandlerCalls);
            throw new InvalidOperationException("intentional crash-handler failure");
        });
        var succeedingFiber = vm.Scheduler.CreateFiber(false);
        failingFiber.Schedule(chunk["fail"]!, false);
        succeedingFiber.Schedule(chunk["succeed"]!, false);
        failingFiber.Resume();
        succeedingFiber.Resume();

        await WithRunningScheduler(vm, () =>
        {
            WaitUntilCollected(vm, failingFiber);
            WaitUntilCollected(vm, succeedingFiber);

            Assert.That(crashHandlerCalls, Is.EqualTo(1));
            Assert.That(succeedingFiber.PopValue().Boolean, Is.True);
            return Task.CompletedTask;
        });
    }

    [Test]
    public async Task Scheduler_ProcessesWorkWhileWakeAndRegistryMutationsRace()
    {
        const int mutationCount = 128;
        const int wakeCount = 512;
        const int producerCount = 3;
        var vm = new CeresVM();
        var chunk = new Compiler().Compile("fn work() -> true;", "scheduler_wake_race.vil");
        var fiber = vm.Scheduler.CreateFiber(false);
        fiber.Schedule(chunk["work"]!, false);
        var schedulerTask = Task.Run(vm.Scheduler.Run);

        try
        {
            WaitForIdlePass(vm, 1);
            using var barrier = new Barrier(producerCount + 1);
            var mutationTask = Task.Factory.StartNew(
                () =>
                {
                    if (!barrier.SignalAndWait(TestTimeout))
                    {
                        throw new TimeoutException("Registry mutations did not start together.");
                    }

                    for (var i = 0; i < mutationCount; i++)
                    {
                        var transientFiber = vm.Scheduler.CreateFiber(false);
                        var id = vm.Scheduler.Fibers.Entries.Single(
                            entry => ReferenceEquals(entry.Value, transientFiber)
                        ).Key;
                        vm.Scheduler.Fibers.Remove(id);
                    }
                },
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );
            var wakeTask = Task.Factory.StartNew(
                () =>
                {
                    if (!barrier.SignalAndWait(TestTimeout))
                    {
                        throw new TimeoutException("Wake producers did not start together.");
                    }

                    for (var i = 0; i < wakeCount; i++)
                    {
                        vm.Scheduler.Wake();
                    }
                },
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );
            var resumeTask = Task.Factory.StartNew(
                () =>
                {
                    if (!barrier.SignalAndWait(TestTimeout))
                    {
                        throw new TimeoutException("Fiber resume did not start with the registry mutations.");
                    }

                    fiber.Resume();
                },
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );

            if (!barrier.SignalAndWait(TestTimeout))
            {
                throw new TimeoutException("Concurrent scheduler operations did not start together.");
            }

            await Task.WhenAll(mutationTask, wakeTask, resumeTask).WaitAsync(TestTimeout);
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
    public async Task Scheduler_DebuggerStateChangesAndContinueWakeIdleScheduler()
    {
        var vm = new CeresVM();
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
            WaitForIdlePass(vm, 1);
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

    [Test]
    public async Task Scheduler_CollectsFibersCreatedConcurrentlyAfterIdle()
    {
        const int creatorCount = 8;
        const int fibersPerCreator = 8;
        var vm = new CeresVM();
        var chunk = new Compiler().Compile("fn work() -> true;", "scheduler_concurrent_creation.vil")["work"]!;
        var fibers = new Fiber[creatorCount][];
        var schedulerTask = Task.Run(vm.Scheduler.Run);

        try
        {
            WaitForIdlePass(vm, 1);
            using var barrier = new Barrier(creatorCount + 1);
            var createTasks = Enumerable.Range(0, creatorCount).Select(creator => Task.Factory.StartNew(
                () =>
                {
                    if (!barrier.SignalAndWait(TestTimeout))
                    {
                        throw new TimeoutException("Concurrent fiber creation did not start together.");
                    }

                    var createdFibers = new Fiber[fibersPerCreator];
                    for (var i = 0; i < createdFibers.Length; i++)
                    {
                        createdFibers[i] = vm.Scheduler.CreateFiber(false);
                        createdFibers[i].Schedule(chunk);
                    }

                    fibers[creator] = createdFibers;
                },
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            )).ToArray();

            if (!barrier.SignalAndWait(TestTimeout))
            {
                throw new TimeoutException("Concurrent fiber creation did not start together.");
            }

            await Task.WhenAll(createTasks).WaitAsync(TestTimeout);

            foreach (var creatorFibers in fibers)
            {
                foreach (var fiber in creatorFibers)
                {
                    WaitUntilCollected(vm, fiber);
                    Assert.That(fiber.PopValue().Boolean, Is.True);
                }
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