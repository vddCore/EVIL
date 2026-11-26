namespace EVIL.Assessment.Tests.VM.Concurrency;

using System;
using System.Threading;
using System.Threading.Tasks;
using EVIL.VirtualMachine.ExecutionEngine;
using EVIL.VirtualMachine.ExecutionEngine.Concurrency;
using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.VirtualMachine.TranslationEngine;
using NUnit.Framework;

[TestFixture]
public class FiberCompletionTests
{
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(5);

    [TestCase(false)]
    [TestCase(true)]
    public async Task BlockUntilFinishedAsync_CompletesForTerminalFiber(bool crash)
    {
        var vm = new VirtualMachine();
        var fiber = vm.Scheduler.CreateFiber(false);
        var chunk = CompileChunk(crash);
        var completion = fiber.BlockUntilFinishedAsync();

        Assert.That(completion.IsCompleted, Is.False);

        await WithRunningScheduler(vm, async () =>
        {
            fiber.Schedule(chunk);
            await completion.WaitAsync(TestTimeout);
        });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(fiber.State, Is.EqualTo(crash 
                ? FiberState.Crashed 
                : FiberState.Finished
            ));
            
            Assert.That(fiber.BlockUntilFinishedAsync().IsCompleted, Is.True);
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task BlockUntilFinished_SynchronousWaitCompletesForTerminalFiber(bool crash)
    {
        var vm = new VirtualMachine();
        var fiber = vm.Scheduler.CreateFiber(false);
        var chunk = CompileChunk(crash);

        await WithRunningScheduler(vm, async () =>
        {
            fiber.Schedule(chunk);
            await Task.Run(fiber.BlockUntilFinished).WaitAsync(TestTimeout);
        });

        Assert.That(fiber.State, Is.EqualTo(crash 
            ? FiberState.Crashed 
            : FiberState.Finished
        ));
        
        Assert.DoesNotThrow(fiber.BlockUntilFinished);
    }

    [Test]
    public async Task BlockUntilFinishedAsync_TracksEachRunOfReusedMainFiber()
    {
        var vm = new VirtualMachine();
        var compiler = new Compiler();
        var firstChunk = compiler.Compile("fn work() -> true;", "fiber_completion_first.vil")["work"]!;
        var secondChunk = compiler.Compile(
            "fn work() { rw val i = 0; while (i < 10000) { i += 1; } ret false; }",
            "fiber_completion_second.vil"
        )["work"]!;

        await WithRunningScheduler(vm, async () =>
        {
            vm.MainFiber.Schedule(firstChunk);
            await vm.MainFiber.BlockUntilFinishedAsync().WaitAsync(TestTimeout);
            Assert.That(vm.MainFiber.PopValue().Boolean, Is.True);

            vm.MainFiber.Schedule(secondChunk);
            await vm.MainFiber.BlockUntilFinishedAsync().WaitAsync(TestTimeout);
            Assert.That(vm.MainFiber.PopValue().Boolean, Is.False);
        });
    }

    [Test]
    public async Task BlockUntilFinishedAsync_DoesNotMissConcurrentCompletion()
    {
        var vm = new VirtualMachine();
        var chunk = CompileChunk(false);

        await WithRunningScheduler(vm, async () =>
        {
            for (var i = 0; i < 32; i++)
            {
                var fiber = vm.Scheduler.CreateFiber(false);
                using var start = new Barrier(2);
                
                var waiter = Task.Run(async () =>
                {
                    // again. how do i make it look nice and
                    // function well. FUCK.
                    if (!start.SignalAndWait(TestTimeout))
                    {
                        throw new TimeoutException("The completion wait race did not start together.");
                    }

                    await fiber.BlockUntilFinishedAsync().WaitAsync(TestTimeout);
                });

                if (!start.SignalAndWait(TestTimeout))
                {
                    throw new TimeoutException("The completion wait race did not start together.");
                }

                fiber.Schedule(chunk);
                await waiter.WaitAsync(TestTimeout);
                Assert.That(fiber.State, Is.EqualTo(FiberState.Finished));
            }
        });
    }

    private static Chunk CompileChunk(bool crash)
    {
        var source = crash
            ? "fn work() { throw \"completion_failure\"; }"
            : "fn work() -> true;";
        
        return new Compiler().Compile(source, "fiber_completion_wait.vil")["work"]!;
    }

    private static async Task WithRunningScheduler(VirtualMachine vm, Func<Task> action)
    {
        var schedulerTask = Task.Run(vm.Scheduler.Run);

        try
        {
            await action();
        }
        finally
        {
            vm.Scheduler.Stop();
            await schedulerTask.WaitAsync(TestTimeout);
        }
    }
}