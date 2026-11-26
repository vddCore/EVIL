namespace EVIL.VirtualMachine.ExecutionEngine;

using System.Threading;
using System.Threading.Tasks;
using EVIL.VirtualMachine.ExecutionEngine.Collections;
using EVIL.VirtualMachine.ExecutionEngine.Diagnostics.Debugging;

public class VirtualMachine : VirtualMachineBase
{
    private sealed class RunnerState
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private readonly Lock _runnerLock = new();
    private RunnerState? _runner;

    public bool IsRunning => Volatile.Read(ref _runner) is not null;

    public VirtualMachine(FiberCrashHandler? crashHandler = null) 
        : base(crashHandler)
    {
    }

    public VirtualMachine(Table global, FiberCrashHandler? crashHandler = null) 
        : base(global, crashHandler)
    {
    }

    public async Task RunAsync()
    {
        var runner = TryClaimRunner();
        if (runner == null)
        {
            return;
        }

        try
        {
            await Task.Run(() => RunScheduler(runner));
        }
        catch
        {
            CompleteRunner(runner);
            throw;
        }
    }

    public void Run()
    {
        var runner = TryClaimRunner();
        if (runner == null)
        {
            return;
        }

        try
        {
            new Thread(() => RunScheduler(runner)).Start();
        }
        catch
        {
            CompleteRunner(runner);
            throw;
        }
    }

    public void Stop()
    {
        var runner = Volatile.Read(ref _runner);
        if (runner == null)
        {
            return;
        }

        runner.Started.Task.GetAwaiter().GetResult();

        lock (_runnerLock)
        {
            if (ReferenceEquals(Volatile.Read(ref _runner), runner))
            {
                Scheduler.Stop();
            }
        }
    }

    private RunnerState? TryClaimRunner()
    {
        lock (_runnerLock)
        {
            var runner = new RunnerState();
            return Interlocked.CompareExchange(ref _runner, runner, null) == null
                ? runner
                : null;
        }
    }

    private void RunScheduler(RunnerState runner)
    {
        try
        {
            Scheduler.Run(() => runner.Started.TrySetResult());
        }
        finally
        {
            CompleteRunner(runner);
        }
    }

    private void CompleteRunner(RunnerState runner)
    {
        runner.Started.TrySetResult();
        Interlocked.CompareExchange(ref _runner, null, runner);
    }
}