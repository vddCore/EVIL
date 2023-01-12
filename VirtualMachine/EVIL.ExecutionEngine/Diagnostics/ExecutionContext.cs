using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Interop;
using EVIL.Intermediate.CodeGeneration;

namespace EVIL.ExecutionEngine.Diagnostics
{
    public class ExecutionContext
    {
        public EVM VirtualMachine { get; }
        public Chunk Chunk { get; }

        public Stack<IteratorState> IteratorStates { get; } = new();
        public Stack<StackFrame> CallStack { get; } = new();
        public Stack<DynamicValue> EvaluationStack { get; } = new();
        public Dictionary<Chunk, DynamicValue[]> ExternContexts { get; } = new();

        public int CallStackLimit { get; set; } = 256;
        public bool SwallowClrExceptions { get; set; }

        public bool Running { get; private set; }

        public ExecutionContext(EVM virtualMachine, Chunk chunk, params DynamicValue[] args)
        {
            VirtualMachine = virtualMachine;
            Chunk = chunk;
            
            for (var i = 0; i < args.Length; i++)
                EvaluationStack.Push(args[i]);
            
            InvokeChunk(chunk, (byte)args.Length);
        }

        public void Reset()
        {
            IteratorStates.Clear();
            EvaluationStack.Clear();
            ExternContexts.Clear();
            CallStack.Clear();
        }

        public string DumpCallStack()
        {
            var sb = new StringBuilder();

            for (var i = 0; i < CallStack.Count; i++)
            {
                sb.AppendLine($"   at {CallStack.ElementAt(i).ToString()}");
            }

            return sb.ToString();
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
        
        public void Pause()
        {
            Running = false;
        }
        
        public void Resume()
        {
            Running = true;
        }
        
        public void Step()
        {
            if (!CallStack.TryPeek(out var frame))
            {
                Pause();
                return;
            }

            if (!frame.CanExecute)
            {
                Pause();
                return;
            }

            var chunk = frame.Chunk;
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
                    Reset();
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

                    evstack.Push(new(Compare(a, b) >= 0));
                    break;
                }

                case OpCode.CGT:
                {
                    b = evstack.Pop();
                    a = evstack.Pop();

                    evstack.Push(new(Compare(a, b) > 0));
                    break;
                }

                case OpCode.CLE:
                {
                    b = evstack.Pop();
                    a = evstack.Pop();

                    evstack.Push(new(Compare(a, b) <= 0));
                    break;
                }

                case OpCode.CLT:
                {
                    b = evstack.Pop();
                    a = evstack.Pop();

                    evstack.Push(new(Compare(a, b) < 0));
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
                        break;
                    }
                    else if (a.Type == DynamicValueType.Number)
                    {
                        if (b.Type != DynamicValueType.Number)
                            throw new UnexpectedTypeException(b.Type, DynamicValueType.Number);

                        evstack.Push(new(a.Number + b.Number));
                        break;
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

                case OpCode.LDCS:
                {
                    itmp = frame.FetchInt32();
                    evstack.Push(new(chunk.Constants.GetStringConstant(itmp)));
                    break;
                }

                case OpCode.LDCN:
                {
                    itmp = frame.FetchInt32();
                    evstack.Push(new(chunk.Constants.GetNumberConstant(itmp)));
                    break;
                }

                case OpCode.LDL:
                {
                    btmp = frame.FetchByte();
                    a = frame.Locals[btmp];
                    evstack.Push(new(a, false));
                    break;
                }

                case OpCode.STL:
                {
                    btmp = frame.FetchByte();
                    a = evstack.Pop();
                    frame.Locals[btmp] = a;
                    break;
                }

                case OpCode.LDG:
                {
                    itmp = frame.FetchInt32();
                    a = new DynamicValue(chunk.Constants.GetStringConstant(itmp));

                    if (!VirtualMachine.GlobalTable.IsSet(a))
                    {
                        throw new GlobalNotFoundException(a);
                    }

                    evstack.Push(VirtualMachine.GlobalTable.Get(a));
                    break;
                }

                case OpCode.STG:
                {
                    itmp = frame.FetchInt32();
                    a = new DynamicValue(chunk.Constants.GetStringConstant(itmp));
                    b = evstack.Pop();
                    VirtualMachine.GlobalTable.Set(a, b);
                    break;
                }

                case OpCode.CALL:
                {
                    btmp = frame.FetchByte();
                    a = evstack.Pop();

                    switch (a.Type)
                    {
                        case DynamicValueType.Function:
                        {
                            InvokeChunk(a.Function, btmp);
                            break;
                        }

                        case DynamicValueType.ClrFunction:
                            InvokeClrFunction(a.ClrFunction, btmp);
                            break;

                        default:
                            throw new NonInvokableValueException(a);
                    }

                    break;
                }

                case OpCode.TCALL:
                {
                    frame.Jump(0);
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
                    evstack.Push(new(frame.FormalArguments[btmp], false));
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
                        new(a.Index(b), false)
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

                    if (!CallStack.TryPeek(out _))
                        Pause();

                    break;
                }

                case OpCode.XARGS:
                {
                    var tbl = Table.Empty;

                    if (frame.ExtraArguments != null)
                        tbl = new(frame.ExtraArguments);

                    evstack.Push(new(tbl));
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

                    var chunkClone = frame.Chunk.SubChunks[itmp].ShallowClone();

                    if (chunkClone.Externs.Count > 0)
                    {
                        var externs = new DynamicValue[chunkClone.Externs.Count];
                        ExternContexts.Add(chunkClone, externs);

                        for (var i = 0; i < chunkClone.Externs.Count; i++)
                        {
                            var e = chunkClone.Externs[i];
                            if (e.Type == ExternInfo.ExternType.Local)
                            {
                                externs[i] = new(frame.Locals[e.SymbolId], false);
                            }
                            else if (e.Type == ExternInfo.ExternType.Parameter)
                            {
                                externs[i] = new(frame.FormalArguments[e.SymbolId], false);
                            }
                            else if (e.Type == ExternInfo.ExternType.Extern)
                            {
                                if (ExternContexts.TryGetValue(frame.Chunk, out var ec))
                                    externs[i] = new(ec[e.SymbolId], false);
                            }
                        }
                    }

                    evstack.Push(new(chunkClone));
                    break;
                }

                case OpCode.LDX:
                {
                    itmp = frame.FetchInt32();
                    evstack.Push(new(ExternContexts[frame.Chunk][itmp], false));
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
                    a = new DynamicValue(chunk.Constants.GetStringConstant(itmp));

                    if (!VirtualMachine.GlobalTable.IsSet(a))
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

                    VirtualMachine.GlobalTable.Unset(new DynamicValue(chunk.Constants.GetStringConstant(itmp)));
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

        private void InvokeChunk(Chunk chunk, byte argc)
        {
            if (CallStack.Count >= CallStackLimit)
            {
                throw new EvmCallStackOverflowException();
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
        }

        private void InvokeClrFunction(ClrFunction clrFunction, int argc)
        {
            if (CallStack.Count >= CallStackLimit)
            {
                throw new EvmCallStackOverflowException();
            }

            var args = new DynamicValue[argc];
            for (var i = 0; i < argc; i++)
            {
                args[argc - i - 1] = EvaluationStack.Pop();
            }

            var newFrame = new StackFrame(clrFunction);

            CallStack.Push(newFrame);
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
        }
        
        private int Compare(DynamicValue a, DynamicValue b)
        {
            if (a.Type == DynamicValueType.String)
            {
                return string.Compare(a.String, b.String, StringComparison.Ordinal);
            }
            else if (a.Type == DynamicValueType.Number)
            {
                return Math.Sign(a.Number - b.Number);
            }

            throw new TypeComparisonException(a, b);
        }
    }
}