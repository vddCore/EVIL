namespace EVIL.Assessment.Benchmarks.Scheduler;

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;
using EVIL.VirtualMachine.ExecutionEngine;
using EVIL.VirtualMachine.ExecutionEngine.Concurrency;
using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.VirtualMachine.TranslationEngine;

internal static class ProtectedRegionBenchmarks
{
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(10);

    private sealed record Workload(string Name, string Body);

    public static int Run(string[] args)
    {
        var iterations = args.Length > 0 ? int.Parse(args[0]) : 100_000;
        var repetitions = args.Length > 1 ? int.Parse(args[1]) : 5;
        var warmups = args.Length > 2 ? int.Parse(args[2]) : 2;
        var retryCount = args.Length > 3 ? int.Parse(args[3]) : 2;

        if (iterations <= 0 || repetitions <= 0 || warmups < 0 || retryCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(args), "Benchmark parameters must be positive, except warmups and retry count may be zero.");
        }

        Console.WriteLine($"# runtime={RuntimeInformation.FrameworkDescription}");
        Console.WriteLine($"# os={RuntimeInformation.OSDescription}");
        Console.WriteLine($"# architecture={RuntimeInformation.ProcessArchitecture}");
        Console.WriteLine($"# processors={Environment.ProcessorCount}");
        Console.WriteLine($"# server_gc={GCSettings.IsServerGC}");
        Console.WriteLine($"# iterations_per_repetition={iterations}");
        Console.WriteLine($"# repetitions={repetitions}");
        Console.WriteLine($"# warmups_per_workload={warmups}");
        Console.WriteLine($"# retry_count={retryCount}");
        Console.WriteLine("workload,repetition,iterations,wall_ms,allocated_bytes,allocated_bytes_per_iteration,operations_per_second,bytecode_bytes");

        var workloads = CreateWorkloads(retryCount);
        var compiler = new Compiler();
        var vm = new VirtualMachine();
        var chunks = new (Workload Workload, Chunk Chunk)[workloads.Length];

        for (var i = 0; i < workloads.Length; i++)
        {
            var workload = workloads[i];
            var source = $"fn work() {{ rw val i = 0; while (i < {iterations}) {{ {workload.Body} i += 1; }} }}";
            var chunk = compiler.Compile(source, $"protected_region_{workload.Name}.vil")["work"]!;
            chunks[i] = (workload, chunk);
        }

        vm.Run();
        if (!SpinWait.SpinUntil(() => vm.Scheduler.IsRunning, StartupTimeout))
        {
            vm.Stop();
            throw new TimeoutException("The scheduler did not start.");
        }

        try
        {
            foreach (var (workload, chunk) in chunks)
            {
                for (var warmup = 0; warmup < warmups; warmup++)
                {
                    ExecuteOnce(vm, chunk);
                }

                for (var repetition = 1; repetition <= repetitions; repetition++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    Exception? crash = null;
                    var fiber = vm.Scheduler.CreateFiber(
                        false,
                        (_, exception) => crash = exception
                    );

                    var allocatedStart = GC.GetTotalAllocatedBytes(true);
                    var wall = Stopwatch.StartNew();
                    fiber.Schedule(chunk);
                    fiber.BlockUntilFinished();
                    wall.Stop();

                    if (crash != null || fiber.State != FiberState.Finished)
                    {
                        throw new InvalidOperationException(
                            $"Workload '{workload.Name}' failed.",
                            crash
                        );
                    }

                    var allocatedBytes = GC.GetTotalAllocatedBytes() - allocatedStart;
                    var operationsPerSecond = iterations / wall.Elapsed.TotalSeconds;

                    Console.WriteLine(string.Join(",",
                        workload.Name,
                        repetition.ToString(CultureInfo.InvariantCulture),
                        iterations.ToString(CultureInfo.InvariantCulture),
                        wall.Elapsed.TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture),
                        allocatedBytes.ToString(CultureInfo.InvariantCulture),
                        (allocatedBytes / (double)iterations).ToString("F3", CultureInfo.InvariantCulture),
                        operationsPerSecond.ToString("F3", CultureInfo.InvariantCulture),
                        chunk.Code.Length.ToString(CultureInfo.InvariantCulture)
                    ));
                }
            }
        }
        finally
        {
            vm.Stop();
        }

        return 0;
    }

    private static void ExecuteOnce(VirtualMachine vm, Chunk chunk)
    {
        Exception? crash = null;
        var fiber = vm.Scheduler.CreateFiber(
            false,
            (_, exception) => crash = exception
        );

        fiber.Schedule(chunk);
        fiber.BlockUntilFinished();

        if (crash != null || fiber.State != FiberState.Finished)
        {
            throw new InvalidOperationException("A protected-region benchmark warmup failed.", crash);
        }
    }

    private static Workload[] CreateWorkloads(int retryCount)
    {
        var retryCondition = retryCount == 0
            ? "false"
            : $"attempts++ < {retryCount}";

        return
        [
            new Workload("no_protection", string.Empty),
            new Workload("empty_protected", "try { }"),
            new Workload("nested_protected", "try { try { try { } } }"),
            new Workload("body_retry", $"rw val attempts = 0; try {{ if ({retryCondition}) retry; }}"),
            new Workload("catch_retry", $"rw val attempts = 0; try {{ throw 1; }} catch (e) {{ if ({retryCondition}) retry; }}"),
            new Workload("caught_throw", "try { throw 1; } catch (e) { }")
        ];
    }
}