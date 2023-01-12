using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.Intermediate;
using StackFrame = EVIL.ExecutionEngine.Diagnostics.StackFrame;

namespace EVIL.ExecutionEngine
{
    public class EVM
    {
        private StackFrame _currentStackFrame;

        private RuntimeConstPool RuntimeConstPool { get; }
        private Stack<StackFrame> CallStack { get; } = new();
        private Executable Executable { get; }

        public Table GlobalTable { get; } = new();
        public Stack<DynamicValue> EvaluationStack { get; } = new();

        public bool Running { get; private set; }

        public EVM(Executable executable)
        {
            Executable = executable;
            RuntimeConstPool = new RuntimeConstPool(Executable.ConstPool);

            foreach (var name in executable.Globals)
            {
                GlobalTable.Set(
                    new DynamicValue(name),
                    DynamicValue.Zero
                );
            }

            foreach (var c in executable.Chunks)
            {
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

        public void Run()
        {
            Running = true;

            InvokeChunk(Executable.MainChunk, 0);

            while (Running)
            {
                Step();
            }
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

                    var comparer = ValueComparers.RetrieveComparer(a, b);
                    var compResult = comparer(a, b);

                    evstack.Push(new(compResult != 0));
                    break;
                }

                case OpCode.CEQ:
                {
                    b = evstack.Pop();
                    a = evstack.Pop();

                    var comparer = ValueComparers.RetrieveComparer(a, b);
                    var compResult = comparer(a, b);

                    evstack.Push(new(compResult == 0));
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
                    Debug.Assert(evstack.Count >= 0);

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
                    itmp = frame.FetchInt32();
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
                    itmp = frame.FetchInt32();
                    a = evstack.Pop();
                    frame.FormalArguments[itmp] = a;
                    break;
                }

                case OpCode.LDA:
                {
                    itmp = frame.FetchInt32();
                    evstack.Push(frame.FormalArguments[itmp]);
                    break;
                }

                case OpCode.STE:
                {
                    itmp = frame.FetchInt32();
                    var value = evstack.Pop();
                    var key = evstack.Pop();
                    var indexable = evstack.Pop();

                    if (indexable.Type != DynamicValueType.Table)
                    {
                        throw new UnindexableTypeException(indexable.Type);
                    }

                    indexable.Table.Set(key, value);
                    if (itmp != 0)
                    {
                        evstack.Push(new(value, false));
                    }
                    break;
                }

                case OpCode.INDEX:
                {
                    var key = evstack.Pop();
                    var indexable = evstack.Pop();

                    evstack.Push(
                        indexable.Index(key)
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
                    _currentStackFrame = CallStack.Peek();
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

                    evstack.Push(new(a.Contains(b)));
                    break;
                }
                default:
                {
                    throw new VirtualMachineException("Invalid op-code.");
                }
            }
        }

        public void Halt()
        {
            Running = false;
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
                EvaluationStack.Push(
                    clrFunction.Invoke(this, args)
                );
            }
            CallStack.Pop();
            _currentStackFrame = CallStack.Peek();
        }

        private void InvokeChunk(Chunk chunk, int argc)
        {
            var newFrame = new StackFrame(chunk, argc);

            var extraArgs = newFrame.ExtraArguments;
            if (argc < chunk.ParameterCount)
            {
                for (var i = argc; i < chunk.ParameterCount; i++)
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