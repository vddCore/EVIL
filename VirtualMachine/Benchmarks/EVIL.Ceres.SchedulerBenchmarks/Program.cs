namespace EVIL.Ceres.SchedulerBenchmarks;

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using EVIL.Ceres.ExecutionEngine;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;
using EVIL.Ceres.TranslationEngine;

internal static class Program
{
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(10);

    private static async Task<int> Main(string[] args)
    {
        var duration = TimeSpan.FromSeconds(args.Length > 0 ? int.Parse(args[0]) : 1);
        var repetitions = args.Length > 1 ? int.Parse(args[1]) : 3;
        const int latencySamples = 50;
        const int iterationsPerFiber = 100_000;
        const int completionWaitIterations = 10_000;

        Console.WriteLine($"# runtime={RuntimeInformation.FrameworkDescription}");
        Console.WriteLine($"# os={RuntimeInformation.OSDescription}");
        Console.WriteLine($"# processors={Environment.ProcessorCount}");
        Console.WriteLine($"# server_gc={GCSettings.IsServerGC}");
        Console.WriteLine($"# idle_duration_ms={duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture)}");
        Console.WriteLine($"# repetitions={repetitions}");
        Console.WriteLine($"# active_iterations_per_fiber={iterationsPerFiber}");
        Console.WriteLine($"# latency_samples_per_repetition={latencySamples}");
        Console.WriteLine($"# completion_wait_iterations={completionWaitIterations}");
        Console.WriteLine("metric,workload,fiber_count,repetition,wall_ms,cpu_ms,cpu_percent,allocated_bytes,operations_per_second,p50_us,p95_us");

        foreach (var state in new[] { "paused", "awaiting" })
        {
            foreach (var fiberCount in new[] { 0, 100, 1000 })
            {
                for (var repetition = 1; repetition <= repetitions; repetition++)
                {
                    WriteIdleResult(state, fiberCount, repetition, duration);
                }
            }
        }

        foreach (var activeFiberCount in new[] { 1, 8 })
        {
            for (var repetition = 1; repetition <= repetitions; repetition++)
            {
                WriteActiveResult(activeFiberCount, repetition, iterationsPerFiber);
            }
        }

        foreach (var runnableFiberCount in new[] { 1, 8 })
        {
            foreach (var waitingFiberCount in new[] { 100, 1000 })
            {
                for (var repetition = 1; repetition <= repetitions; repetition++)
                {
                    WriteMixedAwaitResult(
                        runnableFiberCount,
                        waitingFiberCount,
                        repetition,
                        duration
                    );
                }
            }
        }

        string[] workloads = [
            "schedule_to_completion",
            "schedule_to_parent_completion"
        ];
            
        foreach (var workload in workloads)
        {
            for (var repetition = 1; repetition <= repetitions; repetition++)
            {
                WriteLatencyResult(workload, repetition, latencySamples);
            }
        }

        string[] modes =
        [
            "async_polling_reference",
            "async_notification",
            "sync_polling_reference",
            "sync_notification"
        ];
            
        foreach (var waitMode in modes)
        {
            for (var repetition = 1; repetition <= repetitions; repetition++)
            {
                await WriteCompletionWaitResult(
                    waitMode,
                    repetition,
                    latencySamples,
                    completionWaitIterations
                );
            }
        }

        return 0;
    }

    private static void WriteIdleResult(string state, int fiberCount, int repetition, TimeSpan duration)
    {
        var vm = new CeresVM();
        PrepareIdleFibers(vm, state, fiberCount);
        var schedulerThread = StartScheduler(vm);

        try
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(250));

            using var process = Process.GetCurrentProcess();
            process.Refresh();
            var cpuStart = process.TotalProcessorTime;
            var allocationStart = GC.GetTotalAllocatedBytes();
            var wall = Stopwatch.StartNew();
            Thread.Sleep(duration);
            wall.Stop();
            process.Refresh();

            var cpu = process.TotalProcessorTime - cpuStart;
            var allocatedBytes = GC.GetTotalAllocatedBytes() - allocationStart;
            WriteRow(
                "idle_cpu",
                state,
                fiberCount,
                repetition,
                wall.Elapsed.TotalMilliseconds,
                cpu.TotalMilliseconds,
                cpu.TotalMilliseconds / wall.Elapsed.TotalMilliseconds * 100,
                allocatedBytes,
                null,
                null,
                null
            );
        }
        finally
        {
            StopScheduler(vm, schedulerThread);
        }
    }

    private static void PrepareIdleFibers(CeresVM vm, string state, int fiberCount)
    {
        if (state == "paused")
        {
            for (var i = 0; i < fiberCount; i++)
            {
                var fiber = vm.Scheduler.CreateFiber(false);
                fiber.Resume();
                fiber.Yield();
            }

            return;
        }

        if (fiberCount == 0)
        {
            return;
        }

        var target = vm.Scheduler.CreateFiber(false);
        target.Resume();
        target.Yield();

        for (var i = 0; i < fiberCount; i++)
        {
            var fiber = vm.Scheduler.CreateFiber(false);
            fiber.WaitFor(target);
        }
    }

    private static void WriteActiveResult(int fiberCount, int repetition, int iterationsPerFiber)
    {
        var vm = new CeresVM();
        var compiler = new Compiler();
        var source = $"fn work() {{ rw val i = 0; while (i < {iterationsPerFiber}) {{ i += 1; }} }}";
        var workChunk = compiler.Compile(source, "scheduler_active.vil")["work"]!;
        var crashCount = 0;
        var fibers = new Fiber[fiberCount];

        for (var i = 0; i < fibers.Length; i++)
        {
            fibers[i] = CreatePausedFiber(
                vm, 
                (_, _) => Interlocked.Increment(ref crashCount)
            );
        }

        var warmupFiber = CreatePausedFiber(vm);
        var schedulerThread = StartScheduler(vm);

        try
        {
            warmupFiber.Schedule(workChunk, false);
            warmupFiber.Resume();
            WaitForFiberCount(vm, fiberCount + 1);
            if (crashCount != 0)
            {
                throw new InvalidOperationException($"{crashCount} warmup fibers crashed.");
            }

            var allocationStart = GC.GetTotalAllocatedBytes(true);
            using var process = Process.GetCurrentProcess();
            process.Refresh();
            var cpuStart = process.TotalProcessorTime;
            var wall = Stopwatch.StartNew();

            for (var i = 0; i < fibers.Length; i++)
            {
                fibers[i].Schedule(workChunk, false);
                fibers[i].Resume();
            }

            WaitForFiberCount(vm, 1);
            if (crashCount != 0)
            {
                throw new InvalidOperationException($"{crashCount} active fibers crashed.");
            }

            wall.Stop();
            process.Refresh();

            var cpu = process.TotalProcessorTime - cpuStart;
            var allocatedBytes = GC.GetTotalAllocatedBytes() - allocationStart;
            var operations = (double)fiberCount * iterationsPerFiber;
            
            WriteRow(
                "active_throughput",
                "loop_iterations",
                fiberCount,
                repetition,
                wall.Elapsed.TotalMilliseconds,
                cpu.TotalMilliseconds,
                null,
                allocatedBytes,
                operations / wall.Elapsed.TotalSeconds,
                null,
                null
            );
        }
        finally
        {
            StopScheduler(vm, schedulerThread);
        }
    }

    private static void WriteMixedAwaitResult(
        int runnableFiberCount,
        int waitingFiberCount,
        int repetition,
        TimeSpan duration)
    {
        var vm = new CeresVM();
        var compiler = new Compiler();
        var chunk = compiler.Compile(
            "fn work() { while (true) { } } fn parent() { } fn complete() { }",
            "scheduler_mixed_await.vil"
        );
        var runnableFibers = new Fiber[runnableFiberCount];

        for (var i = 0; i < runnableFibers.Length; i++)
        {
            runnableFibers[i] = CreatePausedFiber(vm);
            runnableFibers[i].Schedule(chunk["work"]!, false);
        }

        var target = CreatePausedFiber(vm);
        var waitingFibers = new Fiber[waitingFiberCount];

        for (var i = 0; i < waitingFibers.Length; i++)
        {
            waitingFibers[i] = CreatePausedFiber(vm);
            waitingFibers[i].Schedule(chunk["parent"]!, false);
            waitingFibers[i].WaitFor(target);
        }

        var schedulerThread = StartScheduler(vm);

        try
        {
            for (var i = 0; i < runnableFibers.Length; i++)
            {
                runnableFibers[i].Resume();
            }

            Thread.Sleep(TimeSpan.FromMilliseconds(250));

            var workload = $"runnable_{runnableFiberCount}_waiting_{waitingFiberCount}";
            using var process = Process.GetCurrentProcess();
            process.Refresh();
            var cpuStart = process.TotalProcessorTime;
            var allocationStart = GC.GetTotalAllocatedBytes(true);
            var wall = Stopwatch.StartNew();
            Thread.Sleep(duration);
            wall.Stop();
            process.Refresh();

            var cpu = process.TotalProcessorTime - cpuStart;
            var allocatedBytes = GC.GetTotalAllocatedBytes() - allocationStart;
            WriteRow(
                "mixed_await_polling",
                workload,
                runnableFiberCount + waitingFiberCount + 1,
                repetition,
                wall.Elapsed.TotalMilliseconds,
                cpu.TotalMilliseconds,
                cpu.TotalMilliseconds / wall.Elapsed.TotalMilliseconds * 100,
                allocatedBytes,
                null,
                null,
                null
            );

            process.Refresh();
            var wakeCpuStart = process.TotalProcessorTime;
            var wakeAllocationStart = GC.GetTotalAllocatedBytes(true);
            var wakeStartedAt = Stopwatch.GetTimestamp();
            target.Schedule(chunk["complete"]!, false);
            target.Resume();
            if (!SpinWait.SpinUntil(
                    () => vm.Scheduler.Fibers.Count == runnableFiberCount + 1,
                    StartupTimeout
                ))
            {
                var registryStates = string.Join(
                    ",",
                    vm.Scheduler.Fibers.Entries.Select(entry => $"{entry.Key}:{entry.Value.State}")
                );
                throw new TimeoutException(
                    $"The mixed-await wake cycle stalled: target={target.State}, " +
                    $"waiter={waitingFibers[0].State}, dependencies={waitingFibers[0].WaitingFor.Count}, " +
                    $"runnable={runnableFibers[0].State}, registry={registryStates}."
                );
            }

            var wakeLatency = Stopwatch.GetElapsedTime(wakeStartedAt).TotalMicroseconds;
            process.Refresh();
            var wakeCpu = process.TotalProcessorTime - wakeCpuStart;
            var wakeAllocatedBytes = GC.GetTotalAllocatedBytes() - wakeAllocationStart;
            WriteRow(
                "mixed_await_wake_cycle",
                workload,
                runnableFiberCount + waitingFiberCount + 1,
                repetition,
                wakeLatency / 1000,
                wakeCpu.TotalMilliseconds,
                wakeCpu.TotalMilliseconds / (wakeLatency / 1000) * 100,
                wakeAllocatedBytes,
                null,
                wakeLatency,
                wakeLatency
            );
        }
        finally
        {
            StopScheduler(vm, schedulerThread);
        }
    }

    private static void WriteLatencyResult(string workload, int repetition, int sampleCount)
    {
        var vm = new CeresVM();
        var chunk = new Compiler().Compile("fn work() { }", "scheduler_latency.vil")["work"]!;
        var fiberCountPerSample = workload == "schedule_to_completion" ? 1 : 2;
        var baseFiberCount = 1 + fiberCountPerSample * sampleCount;
        var crashCount = 0;
        var fibers = new Fiber[sampleCount];
        var targets = workload == "schedule_to_completion" ? null : new Fiber[sampleCount];

        for (var i = 0; i < sampleCount; i++)
        {
            fibers[i] = CreatePausedFiber(vm, (_, _) => Interlocked.Increment(ref crashCount));

            if (targets != null)
            {
                targets[i] = CreatePausedFiber(vm);
                fibers[i].WaitFor(targets[i]);
            }
        }

        var warmupFiber = CreatePausedFiber(vm);
        var schedulerThread = StartScheduler(vm);
        var latencies = new double[sampleCount];

        try
        {
            warmupFiber.Schedule(chunk, false);
            warmupFiber.Resume();
            WaitForFiberCount(vm, baseFiberCount);
            
            if (crashCount != 0)
            {
                throw new InvalidOperationException($"{crashCount} warmup fibers crashed.");
            }

            var allocationStart = GC.GetTotalAllocatedBytes(true);
            var wall = Stopwatch.StartNew();

            for (var i = 0; i < sampleCount; i++)
            {
                var startedAt = Stopwatch.GetTimestamp();

                if (targets == null)
                {
                    fibers[i].Schedule(chunk, false);
                    fibers[i].Resume();
                }
                else
                {
                    targets[i].Schedule(chunk, false);
                    targets[i].Resume();
                }

                WaitForFiberCount(vm, baseFiberCount - fiberCountPerSample * (i + 1));
                latencies[i] = Stopwatch.GetElapsedTime(startedAt).TotalMicroseconds;
            }

            wall.Stop();
            if (crashCount != 0)
            {
                throw new InvalidOperationException($"{crashCount} latency workload fibers crashed.");
            }

            var allocatedBytes = GC.GetTotalAllocatedBytes() - allocationStart;
            Array.Sort(latencies);
            WriteRow(
                "latency",
                workload,
                workload == "schedule_to_completion" ? 1 : 2,
                repetition,
                wall.Elapsed.TotalMilliseconds,
                null,
                null,
                allocatedBytes,
                sampleCount / wall.Elapsed.TotalSeconds,
                Percentile(latencies, 0.50),
                Percentile(latencies, 0.95)
            );
        }
        finally
        {
            StopScheduler(vm, schedulerThread);
        }
    }

    private static async Task WriteCompletionWaitResult(
        string waitMode,
        int repetition,
        int sampleCount,
        int iterationsPerFiber)
    {
        var vm = new CeresVM();
        var source = $"fn work() {{ rw val i = 0; while (i < {iterationsPerFiber}) {{ i += 1; }} }}";
        var chunk = new Compiler().Compile(source, "scheduler_completion_wait.vil")["work"]!;
        var warmupFiber = CreatePausedFiber(vm);
        var schedulerThread = StartScheduler(vm);
        var latencies = new double[sampleCount];

        try
        {
            warmupFiber.Schedule(chunk, false);
            warmupFiber.Resume();
            await WaitForCompletion(warmupFiber, waitMode);

            var allocationStart = GC.GetTotalAllocatedBytes(true);
            var wall = Stopwatch.StartNew();

            for (var i = 0; i < sampleCount; i++)
            {
                var fiber = vm.Scheduler.CreateFiber(false);
                var startedAt = Stopwatch.GetTimestamp();
                fiber.Schedule(chunk);
                await WaitForCompletion(fiber, waitMode);
                latencies[i] = Stopwatch.GetElapsedTime(startedAt).TotalMicroseconds;
            }

            wall.Stop();
            var allocatedBytes = GC.GetTotalAllocatedBytes() - allocationStart;
            Array.Sort(latencies);
            WriteRow(
                "completion_wait",
                waitMode,
                1,
                repetition,
                wall.Elapsed.TotalMilliseconds,
                null,
                null,
                allocatedBytes,
                sampleCount / wall.Elapsed.TotalSeconds,
                Percentile(latencies, 0.50),
                Percentile(latencies, 0.95)
            );
        }
        finally
        {
            StopScheduler(vm, schedulerThread);
        }
    }

    private static Task WaitForCompletion(Fiber fiber, string waitMode)
    {
        var isAsync = waitMode.StartsWith("async_", StringComparison.Ordinal);
        var useNotification = waitMode.EndsWith("notification", StringComparison.Ordinal);

        if (isAsync)
        {
            return useNotification
                ? fiber.BlockUntilFinishedAsync()
                : WaitForCompletionByPollingAsync(fiber);
        }

        if (useNotification)
        {
            fiber.BlockUntilFinished();
        }
        else
        {
            WaitForCompletionByPolling(fiber);
        }

        return Task.CompletedTask;
    }

    private static async Task WaitForCompletionByPollingAsync(Fiber fiber)
    {
        while (fiber.State != FiberState.Finished && fiber.State != FiberState.Crashed)
        {
            await Task.Delay(1);
        }
    }

    private static void WaitForCompletionByPolling(Fiber fiber)
    {
        while (fiber.State != FiberState.Finished && fiber.State != FiberState.Crashed)
        {
            Thread.Sleep(1);
        }
    }

    private static Thread StartScheduler(CeresVM vm)
    {
        var thread = new Thread(vm.Scheduler.Run)
        {
            IsBackground = true,
            Name = "Scheduler benchmark"
        };
        thread.Start();

        if (!SpinWait.SpinUntil(() => vm.Scheduler.IsRunning, StartupTimeout))
        {
            vm.Scheduler.Stop();
            thread.Join(StartupTimeout);
            throw new TimeoutException("The scheduler did not start.");
        }

        return thread;
    }

    private static void StopScheduler(CeresVM vm, Thread thread)
    {
        vm.Scheduler.Stop();

        if (!thread.Join(StartupTimeout))
        {
            throw new TimeoutException("The scheduler did not stop.");
        }
    }

    private static Fiber CreatePausedFiber(CeresVM vm, FiberCrashHandler? crashHandler = null)
    {
        var fiber = vm.Scheduler.CreateFiber(false, crashHandler);
        fiber.Resume();
        fiber.Yield();
        
        return fiber;
    }

    private static void WaitForFiberCount(CeresVM vm, int expectedCount)
    {
        if (!SpinWait.SpinUntil(() => vm.Scheduler.Fibers.Count == expectedCount, StartupTimeout))
        {
            throw new TimeoutException($"The registry did not reach {expectedCount} fibers.");
        }
    }

    private static double Percentile(double[] sortedValues, double percentile)
    {
        var index = (int)Math.Ceiling(percentile * sortedValues.Length) - 1;
        return sortedValues[Math.Clamp(index, 0, sortedValues.Length - 1)];
    }

    private static void WriteRow(
        string metric,
        string workload,
        int fiberCount,
        int repetition,
        double wallMilliseconds,
        double? cpuMilliseconds,
        double? cpuPercent,
        long allocatedBytes,
        double? operationsPerSecond,
        double? p50Microseconds,
        double? p95Microseconds)
    {
        Console.WriteLine(string.Join(",",
            metric,
            workload,
            fiberCount.ToString(CultureInfo.InvariantCulture),
            repetition.ToString(CultureInfo.InvariantCulture),
            Format(wallMilliseconds),
            Format(cpuMilliseconds),
            Format(cpuPercent),
            allocatedBytes.ToString(CultureInfo.InvariantCulture),
            Format(operationsPerSecond),
            Format(p50Microseconds),
            Format(p95Microseconds)
        ));
    }

    private static string Format(double? value)
        => value?.ToString("F3", CultureInfo.InvariantCulture) ?? string.Empty;
}