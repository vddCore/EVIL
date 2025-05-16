namespace EVIL.Ceres.ExecutionEngine;

using System;
using System.Threading;
using System.Threading.Tasks;
using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;

public class CeresVM : VirtualMachineBase
{
    public bool IsRunning { get; private set; }

    public CeresVM(FiberCrashHandler? crashHandler = null) 
        : base(crashHandler)
    {
    }

    public CeresVM(Table global, FiberCrashHandler? crashHandler = null) 
        : base(global, crashHandler)
    {
    }

    public async Task RunAsync()
    {
        if (IsRunning)
            return;

        IsRunning = true;
        {
            await Task.Run(async () =>
            {
                Scheduler.Run();
                while (Scheduler.IsRunning)
                {
                    await Task.Delay(TimeSpan.FromTicks(1));
                }
            });
        }
        IsRunning = false;
    }

    public void Run()
    {
        if (IsRunning)
            return;

        new Thread(() =>
        {
            IsRunning = true;
            {
                Scheduler.Run();
                while (Scheduler.IsRunning)
                {
                    Thread.Sleep(TimeSpan.FromTicks(1));
                }
            }
            IsRunning = false;
        }).Start();
    }

    public void Stop()
    {
        if (!IsRunning)
            return;
        
        Scheduler.Stop();
    }
}