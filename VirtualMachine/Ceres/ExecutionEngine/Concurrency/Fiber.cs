using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.Diagnostics.Debugging;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine.Concurrency
{
    public sealed class Fiber
    {
        private Queue<(Chunk Chunk, DynamicValue[] Arguments)> _scheduledChunks;
        private HashSet<Fiber> _waitingFor;

        private Stack<DynamicValue> _evaluationStack;
        private CallStack _callStack;

        private ExecutionUnit _executionUnit;
        private FiberCrashHandler? _crashHandler;

        internal ChunkInvokeHandler? OnChunkInvoke { get; private set; }
        internal NativeFunctionInvokeHandler? OnNativeFunctionInvoke { get; private set; }

        public FiberCrashHandler? CrashHandler => _crashHandler;
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

        public CallStack CallStack
        {
            get
            {
                lock (_callStack)
                {
                    return _callStack;
                }
            }
        }

        internal Fiber(CeresVM virtualMachine)
        {
            VirtualMachine = virtualMachine;

            _scheduledChunks = new Queue<(Chunk, DynamicValue[])>();
            _waitingFor = new HashSet<Fiber>();

            _evaluationStack = new Stack<DynamicValue>();
            _callStack = new CallStack();

            _executionUnit = new ExecutionUnit(
                virtualMachine.Global,
                this,
                _evaluationStack,
                _callStack
            );

            State = FiberState.Fresh;
        }

        public void Schedule(Chunk chunk, params DynamicValue[] args)
            => Schedule(chunk, true, args);

        public void Schedule(Chunk chunk, bool resumeImmediately, params DynamicValue[] args)
        {
            lock (_scheduledChunks)
            {
                _scheduledChunks.Enqueue((chunk, args));
            }

            if (resumeImmediately)
            {
                Resume();
            }
        }

        public async Task ScheduleAsync(Chunk chunk, params DynamicValue[] args)
            => await ScheduleAsync(chunk, true, args);

        public async Task ScheduleAsync(Chunk chunk, bool resumeImmediately, params DynamicValue[] args)
        {
            Schedule(chunk, resumeImmediately, args);
            await BlockUntilFinishedAsync();
        }

        public void BlockUntilFinished()
        {
            while (State != FiberState.Finished && 
                   State != FiberState.Crashed)
            {
                Thread.Sleep(1);
            }
        }
        
        public async Task BlockUntilFinishedAsync()
        {
            var hasAnyChunks = false;
            lock (_scheduledChunks)
            {
                hasAnyChunks = _scheduledChunks.Any();
            }

            while (hasAnyChunks &&
                   State != FiberState.Finished &&
                   State != FiberState.Crashed)
            {
                await Task.Delay(1);
            }
        }

        public string StackTrace(bool skipNativeFrames)
        {
            var sb = new StringBuilder();

            var callStack = CallStack.ToArray(skipNativeFrames);
            for (var i = 0; i < callStack.Length; i++)
            {
                if (callStack[i] is ScriptStackFrame ssf)
                {
                    sb.Append($"at {ssf.Chunk.Name ?? "<unknown>"}");

                    if (!string.IsNullOrEmpty(ssf.Chunk.DebugDatabase.DefinedInFile))
                    {
                        sb.Append($" in {ssf.Chunk.DebugDatabase.DefinedInFile}");
                    }
                    
                    sb.Append($": line {ssf.Chunk.DebugDatabase.GetLineForIP((int)ssf.PreviousOpCodeIP)} (IP: {ssf.PreviousOpCodeIP:X8})");
                    sb.AppendLine();
                }
                else if (callStack[i] is NativeStackFrame nsf)
                {
                    sb.AppendLine($"at clr!{nsf.NativeFunction.Method.DeclaringType!.FullName}::{nsf.NativeFunction.Method.Name}");
                }
            }

            return sb.ToString();
        }

        public void Step()
        {
            try
            {
                if (State != FiberState.Running)
                    return;

                lock (_callStack)
                {
                    if (_callStack.Count == 0)
                    {
                        lock (_scheduledChunks)
                        {
                            if (_scheduledChunks.TryDequeue(out var info))
                            {
                                Invoke(info.Chunk, info.Arguments);
                            }
                            else
                            {
                                State = FiberState.Finished;
                                return;
                            }
                        }
                    }
                }

                _executionUnit.Step();
            }
            catch (Exception e)
            {
                EnterCrashState();

                if (_crashHandler != null)
                {
                    _crashHandler(this, e);
                }
            }
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
                _callStack.DisposeAllAndClear();
            }
        }
        
        public void Reset()
        {
            Stop();
            
            lock (_evaluationStack)
            {
                _evaluationStack.Clear();
            }

            lock (_waitingFor)
            {
                _waitingFor.Clear();
            }
            
            State = FiberState.Fresh;
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

        public void SetCrashHandler(FiberCrashHandler? crashHandler)
        {
            _crashHandler = crashHandler;
        }

        public void SetOnChunkInvoke(ChunkInvokeHandler? handler)
        {
            OnChunkInvoke = handler;
        }

        public void SetOnNativeFunctionInvoke(NativeFunctionInvokeHandler? handler)
        {
            OnNativeFunctionInvoke = handler;
        }

        internal void RemoveFinishedAwaitees()
        {
            _waitingFor.RemoveWhere(x => x.State == FiberState.Finished);
        }

        private void EnterCrashState()
        {
            State = FiberState.Crashed;
        }

        private void Invoke(Chunk chunk, DynamicValue[] args)
        {
            lock (_callStack)
            {
                _callStack.Push(new ScriptStackFrame(this, chunk, args));
            }
        }
    }
}