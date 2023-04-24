using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine.Concurrency
{
    public sealed class Fiber
    {
        private Queue<(Chunk Chunk, DynamicValue[] Arguments)> _scheduledChunks;
        private HashSet<Fiber> _waitingFor;

        private Stack<DynamicValue> _evaluationStack;
        private Stack<StackFrame> _callStack;
        
        private ExecutionUnit _executionUnit;

        public IReadOnlySet<Fiber> WaitingFor => _waitingFor;

        public CeresVM VirtualMachine { get; }

        public FiberState State { get; private set; }
        public bool ImmuneToCollection { get; private set; }

        public IReadOnlyCollection<DynamicValue> EvaluationStack
        {
            get
            {
                lock (_evaluationStack)
                {
                    return _evaluationStack;
                }
            }
        }

        internal Fiber(CeresVM virtualMachine)
        {
            VirtualMachine = virtualMachine;

            _scheduledChunks = new Queue<(Chunk, DynamicValue[])>();
            _waitingFor = new HashSet<Fiber>();

            _evaluationStack = new Stack<DynamicValue>();
            _callStack = new Stack<StackFrame>();
            
            _executionUnit = new ExecutionUnit(
                virtualMachine.Global,
                this,
                _evaluationStack,
                _callStack
            );

            State = FiberState.Fresh;
        }

        public void Schedule(Chunk chunk, params DynamicValue[] args)
        {
            lock (_scheduledChunks)
            {
                _scheduledChunks.Enqueue((chunk, args));
            }

            Resume();
        }

        public async Task ScheduleAsync(Chunk chunk, params DynamicValue[] args)
        {
            Schedule(chunk, args);
            await WaitUntilFinished();
        }

        public void Step()
        {
            if (State != FiberState.Running)
                return;

            lock (_callStack)
            {
                if (!_callStack.Any())
                {
                    lock (_scheduledChunks)
                    {
                        if (!_scheduledChunks.Any())
                        {
                            State = FiberState.Finished;
                            return;
                        }
                        else
                        {
                            var info = _scheduledChunks.Dequeue();
                            Invoke(info.Chunk, info.Arguments);
                        }
                    }
                }
            }

            _executionUnit.Step();
        }

        public void Yield()
        {
            if (State != FiberState.Running)
            {
                return;
            }

            State = FiberState.Paused;
        }

        public void Yield(Fiber to)
        {
            if (State != FiberState.Running)
            {
                return;
            }

            State = FiberState.Paused;
            to.Resume();
        }

        public void Await()
        {
            if (!_waitingFor.Any())
            {
                return;
            }

            State = FiberState.Awaiting;
        }

        public void WaitFor(Fiber fiber)
        {
            if (fiber == this)
            {
                throw new FiberException("A fiber cannot wait for self.");
            }

            if (fiber.State == FiberState.Finished)
            {
                return;
            }

            _waitingFor.Add(fiber);
            Await();
        }

        public void StopWaitingFor(Fiber fiber)
        {
            _waitingFor.Remove(fiber);

            if (State == FiberState.Awaiting)
            {
                if (!_waitingFor.Any())
                {
                    State = FiberState.Running;
                }
            }
        }

        public void WaitForAll(params Fiber[] fibers)
        {
            for (var i = 0; i < fibers.Length; i++)
            {
                var fiber = fibers[i];

                if (fiber == this)
                {
                    throw new FiberException("A fiber cannot wait for self.");
                }

                if (fiber.State == FiberState.Finished)
                {
                    continue;
                }

                _waitingFor.Add(fibers[i]);
            }

            Await();
        }

        public void Resume()
        {
            if (State == FiberState.Awaiting)
            {
                if (_waitingFor.Any())
                {
                    return;
                }
            }

            State = FiberState.Running;
        }

        public void Stop()
        {
            lock (_scheduledChunks)
            {
                _scheduledChunks.Clear();
            }

            lock (_callStack)
            {
                foreach (var frame in _callStack)
                    frame.Dispose();

                _callStack.Clear();
            }
        }

        public void Immunize()
        {
            ImmuneToCollection = true;
        }

        public void DeImmunize()
        {
            ImmuneToCollection = false;
        }

        public void PushValue(DynamicValue value)
        {
            lock (_evaluationStack)
            {
                _evaluationStack.Push(value);
            }
        }

        public DynamicValue PeekValue()
        {
            lock (_evaluationStack)
            {
                return _evaluationStack.Peek();
            }
        }

        public bool TryPeekValue(out DynamicValue value)
        {
            lock (_evaluationStack)
            {
                return _evaluationStack.TryPeek(out value!);
            }
        }

        public DynamicValue PopValue()
        {
            lock (_evaluationStack)
            {
                return _evaluationStack.Pop();
            }
        }

        public bool TryPopValue(out DynamicValue value)
        {
            lock (_evaluationStack)
            {
                return _evaluationStack.TryPop(out value!);
            }
        }

        internal void EnterCrashState()
        {
            State = FiberState.Crashed;
        }

        internal void RemoveFinishedAwaitees()
        {
            _waitingFor.RemoveWhere(x => x.State == FiberState.Finished);
        }

        private void Invoke(Chunk chunk, DynamicValue[] args)
        {
            lock (_callStack)
            {
                _callStack.Push(new ScriptStackFrame(this, chunk, args));
            }
        }

        private async Task WaitUntilFinished()
        {
            while (State != FiberState.Finished && State != FiberState.Crashed)
            {
                await Task.Delay(1);
            }
        }
    }
}