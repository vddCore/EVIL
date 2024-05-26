using System;
using System.Threading.Tasks;
using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;

namespace EVIL.Ceres.ExecutionEngine
{
    public class CeresVM : IDisposable
    {
        private Task? _schedulerTask;
        
        public Table Global { get; }
        public FiberScheduler Scheduler { get; }
        public Fiber MainFiber { get; }

        public CeresVM(FiberCrashHandler? crashHandler = null)
            : this(new Table(), crashHandler)
        {
        }
        
        public CeresVM(Table global, FiberCrashHandler? crashHandler = null)
        {
            Global = global;
            Scheduler = new FiberScheduler(this, crashHandler ?? DefaultCrashHandler);
            MainFiber = Scheduler.CreateFiber(true);
        }

        public void Start()
        {
            if (_schedulerTask != null)
            {
                if (_schedulerTask.Status == TaskStatus.Running)
                {
                    return;
                }
            }
            
            _schedulerTask?.Dispose();
            _schedulerTask = new Task(Scheduler.Run);
            
            _schedulerTask.Start();
        }

        public void Stop()
        {
            if (_schedulerTask == null)
            {
                return;
            }
            
            if (_schedulerTask.Status != TaskStatus.Running)
            {
                return;
            }

            Scheduler.Stop();
        }

        public void Dispose()
        {
            if (_schedulerTask != null && _schedulerTask.Status == TaskStatus.Running)
            {
                Stop();
            }
            
            _schedulerTask?.Wait();
            _schedulerTask?.Dispose();
        }

        private void DefaultCrashHandler(Fiber fiber, Exception exception)
        {
            throw new VirtualMachineException("A fiber has crashed.", exception);
        }
    }
}