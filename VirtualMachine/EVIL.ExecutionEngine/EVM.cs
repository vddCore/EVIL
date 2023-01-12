using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Diagnostics;
using EVIL.ExecutionEngine.Interop;
using EVIL.Intermediate.CodeGeneration;

namespace EVIL.ExecutionEngine
{
    public class EVM
    {
        private StackFrame _currentStackFrame;

        private Executable Executable { get; }
        private RuntimeConstPool RuntimeConstPool { get; }
        
        private Stack<IteratorState> IteratorStates { get; } = new();
        private Stack<StackFrame> CallStack { get; } = new();
        private Stack<DynamicValue> EvaluationStack { get; } = new();
        private Dictionary<Chunk, DynamicValue[]> ExternContexts { get; } = new();

        public int CallStackLimit { get; set; } = 30;

        public bool SwallowClrExceptions { get; set; }
        public Table GlobalTable { get; }

        public bool Running { get; private set; }

        public EVM(Executable executable, Table globalTable)
        {
            GlobalTable = globalTable ?? new Table();
            
            Executable = executable;
            RuntimeConstPool = new RuntimeConstPool(Executable.ConstPool);

            for (var i = 0; i < executable.Globals.Count; i++)
            {
                var name = executable.Globals[i];
                
                GlobalTable.Set(
                    new DynamicValue(name),
                    DynamicValue.Zero
                );
            }

            for (var i = 0; i < executable.Chunks.Count; i++)
            {
                var c = executable.Chunks[i];
                
                if (!c.Name.StartsWith('!'))
                {
                    GlobalTable.Set(
                        new DynamicValue(c.Name),
                        new DynamicValue(c)
                    );
                }
            }
        }
        
        public string DumpEvaluationStack()
        {
            var sb = new StringBuilder();

            for (var i = 0; i < EvaluationStack.Count; i++)
            {
                var v = EvaluationStack.ElementAt(i);
                sb.AppendLine($"{i}: {v.Type} {v.CopyToString().String}");
            }

            return sb.ToString();
        }

        public void SetGlobal(string key, DynamicValue value)
        {
            GlobalTable.Set(new(key), value);
        }

        public void SetGlobal(double key, DynamicValue value)
        {
            GlobalTable.Set(new(key), value);
        }

        public DynamicValue InvokeCallback(Chunk chunk, params DynamicValue[] args)
        {
            if (Running)
                throw new InvalidOperationException("EVM is busy.");

            if (chunk == null)
                throw new InvalidOperationException($"Chunk was null.");

            for (var i = 0; i < args.Length; i++)
            {
                EvaluationStack.Push(args[i]);
            }

            InvokeChunk(chunk, args.Length);

            Running = true;

            Resume();
            var ret = EvaluationStack.Pop();
            Halt();
            
            return ret;
        }

        public Chunk FindExposedChunk(string funcName)
        {
            Chunk c = null;
            for (var i = 0; i < Executable.Chunks.Count; i++)
            {
                var ch = Executable.Chunks[i];

                if (ch.Name.StartsWith("!"))
                    continue;
                
                if (ch.Name == funcName)
                {
                    c = ch;
                    break;
                }
            }

            return c;
        }
        
        public void Pause()
        {
            Running = false;
        }

        public void Resume()
        {
            while (Running)
            {
                Step();
            }
        }

        public void Halt()
        {
            Pause();
            
            ExternContexts.Clear();
            EvaluationStack.Clear();
            CallStack.Clear();
            _currentStackFrame = null;
        }

        public void Run()
        {
            if (Running)
                Halt();
            
            Running = true;
            InvokeChunk(Executable.RootChunk, 0);
            
            Resume();
        }

        public void Step()
        {
            var frame = _currentStackFrame;
            var chunk = _currentStackFrame.Chunk;
            var evstack = EvaluationStack;

            var opCode = frame.FetchOpCode();

            DynamicValue a;
            DynamicValue b;

            int ia;
            int ib;

            byte btmp;
            int itmp;

            switch (opCode)
            {
                case OpCode.NOP:
                {
                    break;
                }

                case OpCode.HLT:
                {
                    Halt();
                    break;
                }

                case OpCode.CNE:
                {
                    b = evstack.Pop();
                    a = evstack.Pop();

                    evstack.Push(new(!a.Equals(b)));
                    break;
                }

                case OpCode.CEQ:
                {
                    b = evstack.Pop();
                    a = evstack.Pop();

                    evstack.Push(new(a.Equals(b)));
                    break;
                }

                case OpCode.CGE:
                {
                    b = evstack.Pop();
                    a = evstack.Pop();

                    var comparer = ValueComparers.RetrieveComparer(a, b);
                    var compResult = comparer(a, b);

                    evstack.Push(new(compResult >= 0));
                    break;
                }

                case OpCode.CGT:
                {
                    b = evstack.Pop();
                    a = evstack.Pop();

                    var comparer = ValueComparers.RetrieveComparer(a, b);
                    var compResult = comparer(a, b);

                    evstack.Push(new(compResult > 0));
                    break;
                }

                case OpCode.CLE:
                {
                    b = evstack.Pop();
                    a = evstack.Pop();

                    var comparer = ValueComparers.RetrieveComparer(a, b);
                    var compResult = comparer(a, b);

                    evstack.Push(new(compResult <= 0));
                    break;
                }

                case OpCode.CLT:
                {
                    b = evstack.Pop();
                    a = evstack.Pop();

                    var comparer = ValueComparers.RetrieveComparer(a, b);
                    var compResult = comparer(a, b);

                    evstack.Push(new(compResult < 0));
                    break;
                }

                case OpCode.DUP:
                {
                    evstack.Push(
                        new(evstack.Peek(), false)
                    );

                    break;
                }

                case OpCode.POP:
                {
                    evstack.Pop();
                    break;
                }

                case OpCode.UNM:
                {
                    a = evstack.Pop();
                    evstack.Push(new(-a.Number));
                    break;
                }

                case OpCode.TOSTR:
                {
                    a = evstack.Pop();
                    evstack.Push(a.CopyToString());
                    break;
                }

                case OpCode.TONUM:
                {
                    a = evstack.Pop();
                    evstack.Push(a.CopyToNumber());
                    break;
                }

                case OpCode.ADD:
                {
                    b = evstack.Pop();
                    a = evstack.Pop();

                    if (a.Type == DynamicValueType.String)
                    {
                        if (b.Type != DynamicValueType.String)
                            throw new UnexpectedTypeException(b.Type, DynamicValueType.String);

                        evstack.Push(new(a.String + b.String));
                        return;
                    }
                    else if (a.Type == DynamicValueType.Number)
                    {
                        if (b.Type != DynamicValueType.Number)
                            throw new UnexpectedTypeException(b.Type, DynamicValueType.Number);

                        evstack.Push(new(a.Number + b.Number));
                        return;
                    }

                    throw new UnexpectedTypeException(a.Type);
                }

                case OpCode.SUB:
                {
                    b = evstack.Pop();
                    a = evstack.Pop();

                    evstack.Push(new(a.Number - b.Number));
                    break;
                }

                case OpCode.MUL:
                {
                    b = evstack.Pop();
                    a = evstack.Pop();

                    evstack.Push(new(a.Number * b.Number));
                    break;
                }

                case OpCode.DIV:
                {
                    b = evstack.Pop();
                    a = evstack.Pop();

                    if (b.Number == 0)
                        throw new VirtualMachineException("Attempt to divide by zero.");

                    evstack.Push(new(a.Number / b.Number));
                    break;
                }

                case OpCode.MOD:
                {
                    b = evstack.Pop();
                    a = evstack.Pop();

                    if (b.Number == 0)
                        throw new VirtualMachineException("Attempt to divide by zero.");

                    evstack.Push(
                        new(a.Number - b.Number * Math.Floor(a.Number / b.Number))
                    );
                    break;
                }

                case OpCode.AND:
                {
                    ib = evstack.Pop().AsInteger();
                    ia = evstack.Pop().AsInteger();

                    evstack.Push(new(ia & ib));
                    break;
                }

                case OpCode.OR:
                {
                    ib = evstack.Pop().AsInteger();
                    ia = evstack.Pop().AsInteger();

                    evstack.Push(new(ia | ib));
                    break;
                }

                case OpCode.XOR:
                {
                    ib = evstack.Pop().AsInteger();
                    ia = evstack.Pop().AsInteger();

                    evstack.Push(new(ia ^ ib));
                    break;
                }

                case OpCode.NOT:
                {
                    ia = evstack.Pop().AsInteger();

                    evstack.Push(new(~ia));
                    break;
                }

                case OpCode.LNOT:
                {
                    a = evstack.Pop();
                    evstack.Push(new(!a.IsTruth));
                    break;
                }

                case OpCode.LAND:
                {
                    b = evstack.Pop();
                    a = evstack.Pop();
                    evstack.Push(new(a.IsTruth && b.IsTruth));
                    break;
                }

                case OpCode.LOR:
                {
                    b = evstack.Pop();
                    a = evstack.Pop();
                    evstack.Push(new(a.IsTruth || b.IsTruth));
                    break;
                }

                case OpCode.SHL:
                {
                    ib = evstack.Pop().AsInteger();
                    ia = evstack.Pop().AsInteger();

                    evstack.Push(new(ia << ib));
                    break;
                }

                case OpCode.SHR:
                {
                    ib = evstack.Pop().AsInteger();
                    ia = evstack.Pop().AsInteger();

                    evstack.Push(new(ia >> ib));
                    break;
                }

                case OpCode.LDC:
                {
                    itmp = frame.FetchInt32();
                    evstack.Push(
                        new(RuntimeConstPool.FetchConst(itmp), true)
                    );
                    break;
                }

                case OpCode.LDL:
                {
                    itmp = frame.FetchInt32();
                    a = frame.Locals[itmp];
                    evstack.Push(a);
                    break;
                }

                case OpCode.STL:
                {
                    itmp = frame.FetchInt32();
                    a = evstack.Pop();
                    frame.Locals[itmp] = a;
                    break;
                }

                case OpCode.LDG:
                {
                    itmp = frame.FetchInt32();
                    a = RuntimeConstPool.FetchConst(itmp);

                    if (!GlobalTable.IsSet(a))
                    {
                        throw new GlobalNotFoundException(a);
                    }

                    evstack.Push(GlobalTable.Get(a));
                    break;
                }

                case OpCode.STG:
                {
                    itmp = frame.FetchInt32();
                    a = RuntimeConstPool.FetchConst(itmp);
                    b = evstack.Pop();
                    GlobalTable.Set(a, b);
                    break;
                }

                case OpCode.CALL:
                {
                    itmp = frame.FetchByte();
                    a = evstack.Pop();

                    switch (a.Type)
                    {
                        case DynamicValueType.Function:
                            InvokeChunk(a.Function, itmp);
                            break;

                        case DynamicValueType.ClrFunction:
                            InvokeClrFunction(a.ClrFunction, itmp);
                            break;

                        default:
                            throw new NonInvokableValueException(a);
                    }

                    break;
                }

                case OpCode.STA:
                {
                    btmp = frame.FetchByte();
                    a = evstack.Pop();
                    frame.FormalArguments[btmp] = a;
                    break;
                }

                case OpCode.LDA:
                {
                    btmp = frame.FetchByte();
                    evstack.Push(frame.FormalArguments[btmp]);
                    break;
                }

                case OpCode.STE:
                {
                    btmp = frame.FetchByte();
                    var value = evstack.Pop();
                    var key = evstack.Pop();
                    var indexable = evstack.Pop();

                    if (indexable.Type != DynamicValueType.Table)
                    {
                        throw new UnindexableTypeException(indexable.Type);
                    }

                    indexable.Table.Set(key, value);
                    if (btmp != 0)
                    {
                        evstack.Push(new(value, false));
                    }
                    break;
                }

                case OpCode.INDEX:
                {
                    b = evstack.Pop();
                    a = evstack.Pop();

                    evstack.Push(
                        a.Index(b)
                    );
                    break;
                }

                case OpCode.NEWTB:
                {
                    evstack.Push(new(new Table()));
                    break;
                }

                case OpCode.RETN:
                {
                    CallStack.Pop();
                    
                    if (!CallStack.TryPeek(out _currentStackFrame))
                    {
                        Pause();
                    }
                    break;
                }

                case OpCode.XARGS:
                {
                    var argTable = new Table(frame.ExtraArguments);
                    evstack.Push(new(argTable));
                    break;
                }

                case OpCode.FJMP:
                {
                    itmp = frame.FetchInt32();
                    a = evstack.Pop();

                    if (!a.IsTruth)
                    {
                        var addr = chunk.Labels[itmp];
                        frame.Jump(addr);
                    }
                    break;
                }

                case OpCode.TJMP:
                {
                    itmp = frame.FetchInt32();
                    a = evstack.Pop();

                    if (a.IsTruth)
                    {
                        var addr = chunk.Labels[itmp];
                        frame.Jump(addr);
                    }
                    break;
                }

                case OpCode.JUMP:
                {
                    itmp = frame.FetchInt32();
                    itmp = chunk.Labels[itmp];
                    frame.Jump(itmp);

                    break;
                }

                case OpCode.LEN:
                {
                    a = evstack.Pop();
                    evstack.Push(new(a.Length));
                    break;
                }

                case OpCode.XIST:
                {
                    b = evstack.Pop();
                    a = evstack.Pop();

                    evstack.Push(new(b.Contains(a)));
                    break;
                }

                case OpCode.LDF:
                {
                    itmp = frame.FetchInt32();

                    var chunkClone = Executable.Chunks[itmp].ShallowClone();

                    if (chunkClone.Externs.Count > 0)
                    {
                        var externs = new DynamicValue[chunkClone.Externs.Count];
                        ExternContexts.Add(chunkClone, externs);

                        var eidx = 0;
                        for (var i = 0; i < _currentStackFrame.FormalArguments.Length; i++)
                        {
                            externs[eidx++] = _currentStackFrame.FormalArguments[i];
                        }

                        for (var i = 0; i < _currentStackFrame.Locals.Length; i++)
                        {
                            externs[eidx++] = _currentStackFrame.Locals[i];
                        }

                        if (ExternContexts.TryGetValue(_currentStackFrame.Chunk, out var externContext))
                        {
                            for (var i = 0; i < externContext.Length; i++)
                            {
                                externs[eidx++] = externContext[i];
                            }
                        }
                    }

                    evstack.Push(new(chunkClone));
                    break;
                }

                case OpCode.LDX:
                {
                    itmp = frame.FetchInt32();
                    evstack.Push(ExternContexts[frame.Chunk][itmp]);
                    break;
                }

                case OpCode.STX:
                {
                    itmp = frame.FetchInt32();
                    a = evstack.Pop();

                    ExternContexts[frame.Chunk][itmp] = a;
                    break;
                }

                case OpCode.GNAME:
                {
                    itmp = frame.FetchInt32();
                    a = RuntimeConstPool.FetchConst(itmp);

                    if (!GlobalTable.IsSet(a))
                    {
                        a = DynamicValue.Zero;
                    }

                    evstack.Push(a);
                    break;
                }

                case OpCode.LNAME:
                {
                    itmp = frame.FetchInt32();
                    evstack.Push(new(frame.Chunk.Locals[itmp]));
                    break;
                }

                case OpCode.XNAME:
                {
                    itmp = frame.FetchInt32();
                    evstack.Push(new(frame.Chunk.Externs[itmp].Name));
                    break;
                }

                case OpCode.PNAME:
                {
                    btmp = frame.FetchByte();
                    evstack.Push(new(frame.Chunk.Parameters[btmp]));
                    break;
                }

                case OpCode.RGL:
                {
                    itmp = frame.FetchInt32();

                    GlobalTable.Unset(RuntimeConstPool.FetchConst(itmp));
                    break;
                }

                case OpCode.RTE:
                {
                    b = evstack.Pop();
                    a = evstack.Pop();

                    a.Table.Unset(b);
                    break;
                }

                case OpCode.EACH:
                {
                    a = evstack.Pop();

                    if (a.Type != DynamicValueType.Table)
                        a = new(a.AsTable());

                    var iterState = new IteratorState(a.Table);
                    IteratorStates.Push(iterState);
                    break;
                }

                case OpCode.ITER:
                {
                    itmp = frame.FetchByte();
                    var iterState = IteratorStates.Peek();
                    var result = iterState.MoveNext();

                    if (result)
                    {
                        if (itmp == 0)
                        {
                            evstack.Push(iterState.CurrentPair.Value);
                        }
                        else if (itmp == 1)
                        {
                            evstack.Push(iterState.CurrentPair.Value);
                            evstack.Push(iterState.CurrentPair.Key);
                        }
                    }

                    evstack.Push(new(result));
                    break;
                }

                case OpCode.ENDE:
                {
                    IteratorStates.Pop();
                    break;
                }

                default:
                {
                    throw new VirtualMachineException("Invalid op-code.");
                }
            }
        }

        private void InvokeClrFunction(ClrFunction clrFunction, int argc)
        {
            var args = new DynamicValue[argc];
            for (var i = 0; i < argc; i++)
            {
                args[argc - i - 1] = EvaluationStack.Pop();
            }

            var newFrame = new StackFrame(clrFunction);

            CallStack.Push(newFrame);
            _currentStackFrame = newFrame;
            {
                try
                {
                    EvaluationStack.Push(
                        clrFunction.Invoke(this, args)
                    );
                }
                catch
                {
                    EvaluationStack.Push(DynamicValue.Zero);

                    if (!SwallowClrExceptions)
                        throw;
                }
            }
            CallStack.Pop();
            _currentStackFrame = CallStack.Peek();
        }

        private void InvokeChunk(Chunk chunk, int argc)
        {
            if (CallStack.Count + 1 > CallStackLimit)
            {
                foreach (var f in CallStack)
                {
                    Console.WriteLine($"{f.Chunk.Name}:{f.IP:X8}");
                }
                Environment.Exit(1);    
            }
            
            var newFrame = new StackFrame(chunk, argc);

            var extraArgs = newFrame.ExtraArguments;
            var paramCount = chunk.Parameters.Count;

            if (argc < paramCount)
            {
                for (var i = argc; i < paramCount; i++)
                {
                    EvaluationStack.Push(new(0));
                }
            }

            if (extraArgs != null)
            {
                for (var i = 0; i < extraArgs.Length; i++)
                {
                    extraArgs[extraArgs.Length - i - 1] = EvaluationStack.Pop();
                }
            }

            CallStack.Push(newFrame);
            _currentStackFrame = newFrame;
        }
    }
}