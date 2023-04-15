using System;
using System.Threading;
using System.Threading.Tasks;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;

namespace Ceres.ExecutionEngine
{
    public class CeresVM : IDisposable
    {
        private CancellationTokenSource? _schedulerCancellationTokenSource;
        private Task? _schedulerTask;

        public Table Global { get; }
        
        public FiberScheduler Scheduler { get; }
        public Fiber MainFiber { get; }

        public CeresVM()
            : this(new Table())
        {
        }
        
        public CeresVM(Table global)
        {
            Global = global;
            Scheduler = new FiberScheduler(this);
            Scheduler.SetCrashHandler(DefaultCrashHandler);
            
            MainFiber = Scheduler.CreateFiber(true);
        }

        public void Run()
        {
            if (_schedulerTask != null)
            {
                if (_schedulerTask.Status == TaskStatus.Running)
                {
                    return;
                }
            }
            
            _schedulerCancellationTokenSource?.Dispose();
            _schedulerTask?.Dispose();
            
            _schedulerTask = new Task(() => Scheduler.Run());
            _schedulerCancellationTokenSource = new CancellationTokenSource();
            _schedulerTask.Start();
        }

        public void Stop()
        {
            if (_schedulerTask == null || _schedulerCancellationTokenSource == null)
            {
                return;
            }
            
            if (_schedulerTask.Status != TaskStatus.Running)
            {
                return;
            }

            _schedulerCancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            _schedulerCancellationTokenSource?.Dispose();
            _schedulerTask?.Dispose();
        }

        private void DefaultCrashHandler(FiberScheduler scheduler, Fiber fiber, Exception exception)
        {
            throw new VirtualMachineException("A fiber has crashed.", exception);
        }
    }
}