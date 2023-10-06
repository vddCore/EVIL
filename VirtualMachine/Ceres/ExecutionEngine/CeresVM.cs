using System;
using System.Threading.Tasks;
using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics.Debugging;

namespace Ceres.ExecutionEngine
{
    public class CeresVM : IDisposable
    {
        private Task? _schedulerTask;

        internal ChunkInvokeHandler? OnChunkInvoke { get; private set; }
        
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

        public void SetOnChunkInvoke(ChunkInvokeHandler? handler)
        {
            OnChunkInvoke = handler;
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

        private void DefaultCrashHandler(FiberScheduler scheduler, Fiber fiber, Exception exception)
        {
            throw new VirtualMachineException("A fiber has crashed.", exception);
        }
    }
}