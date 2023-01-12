using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Diagnostics;
using EVIL.Intermediate;

namespace EVIL.ExecutionEngine
{
    public class EVM
    {
        private RuntimeConstPool RuntimeConstPool { get; }
        private StackFrame CurrentStackFrame => CallStack.Peek();
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
            CallStack.Push(new StackFrame(Executable.MainChunk, 0));

            while (Running)
            {
                if (!CurrentStackFrame.CanExecute)
                {
                    if (CurrentStackFrame.Chunk == Executable.MainChunk)
                    {
                        Halt();
                    }
                    else
                    {
                        CallStack.Pop();
                    }
                }
                else
                {
                    Step();
                }
            }
        }

        public void Step()
        {
            var frame = CurrentStackFrame;
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

                case OpCode.DUP:
                {
                    EvaluationStack.Push(
                        new(EvaluationStack.Peek(), false)
                    );
                    
                    break;
                }

                case OpCode.POP:
                {
                    if (EvaluationStack.Count <= 0)
                        throw new VirtualMachineException("Evaluation stack underflow.");

                    EvaluationStack.Pop();
                    break;
                }

                case OpCode.HLT:
                {
                    Halt();
                    break;
                }

                case OpCode.UNM:
                {
                    a = EvaluationStack.Pop();
                    EvaluationStack.Push(new(-a.Number));
                    break;
                }

                case OpCode.TOSTR:
                {
                    a = EvaluationStack.Pop();
                    EvaluationStack.Push(a.CopyToString());
                    break;
                }

                case OpCode.TONUM:
                {
                    a = EvaluationStack.Pop();
                    EvaluationStack.Push(a.CopyToNumber());
                    break;
                }

                case OpCode.ADD:
                {
                    b = EvaluationStack.Pop();
                    a = EvaluationStack.Pop();

                    EvaluationStack.Push(new(a.Number + b.Number));
                    break;
                }

                case OpCode.SUB:
                {
                    b = EvaluationStack.Pop();
                    a = EvaluationStack.Pop();

                    EvaluationStack.Push(new(a.Number - b.Number));
                    break;
                }

                case OpCode.MUL:
                {
                    b = EvaluationStack.Pop();
                    a = EvaluationStack.Pop();

                    EvaluationStack.Push(new(a.Number * b.Number));
                    break;
                }

                case OpCode.DIV:
                {
                    b = EvaluationStack.Pop();
                    a = EvaluationStack.Pop();

                    if (b.Number == 0)
                        throw new VirtualMachineException("Attempt to divide by zero.");

                    EvaluationStack.Push(new(a.Number / b.Number));
                    break;
                }

                case OpCode.MOD:
                {
                    b = EvaluationStack.Pop();
                    a = EvaluationStack.Pop();

                    if (b.Number == 0)
                        throw new VirtualMachineException("Attempt to divide by zero.");

                    EvaluationStack.Push(
                        new(a.Number - b.Number * Math.Floor(a.Number / b.Number))
                    );
                    break;
                }

                case OpCode.AND:
                {
                    ib = EvaluationStack.Pop().AsInteger();
                    ia = EvaluationStack.Pop().AsInteger();
                    
                    EvaluationStack.Push(new(ia & ib));
                    break;
                }

                case OpCode.OR:
                {
                    ib = EvaluationStack.Pop().AsInteger();
                    ia = EvaluationStack.Pop().AsInteger();
                    
                    EvaluationStack.Push(new(ia | ib));
                    break;
                }

                case OpCode.XOR:
                {
                    ib = EvaluationStack.Pop().AsInteger();
                    ia = EvaluationStack.Pop().AsInteger();
                    
                    EvaluationStack.Push(new(ia ^ ib));
                    break;
                }

                case OpCode.NOT:
                {
                    ia = EvaluationStack.Pop().AsInteger();
                    
                    EvaluationStack.Push(new(~ia));
                    break;
                }

                case OpCode.LNOT:
                {
                    a = EvaluationStack.Pop();
                    EvaluationStack.Push(
                        new(a.IsTruth ? 1 : 0)
                    );
                    break;
                }

                case OpCode.LAND:
                {
                    b = EvaluationStack.Pop();
                    a = EvaluationStack.Pop();
                    EvaluationStack.Push(
                        new(a.IsTruth && b.IsTruth ? 1 : 0)
                    );
                    break;
                }

                case OpCode.LOR:
                {
                    b = EvaluationStack.Pop();
                    a = EvaluationStack.Pop();
                    EvaluationStack.Push(
                        new(a.IsTruth || b.IsTruth ? 1 : 0)
                    );
                    break;
                }

                case OpCode.SHL:
                {
                    ib = EvaluationStack.Pop().AsInteger();
                    ia = EvaluationStack.Pop().AsInteger();
                    
                    EvaluationStack.Push(new(ia << ib));
                    break;
                }
                
                case OpCode.SHR:
                {
                    ib = EvaluationStack.Pop().AsInteger();
                    ia = EvaluationStack.Pop().AsInteger();
                    
                    EvaluationStack.Push(new(ia >> ib));
                    break;
                }

                case OpCode.LDC:
                {
                    itmp = frame.FetchInt32();
                    EvaluationStack.Push(
                        new(RuntimeConstPool.FetchConst(itmp), true)
                    );
                    break;
                }

                case OpCode.LDL:
                {
                    itmp = frame.FetchInt32();
                    a = frame.Locals[itmp];
                    EvaluationStack.Push(a);
                    break;
                }

                case OpCode.STL:
                {
                    itmp = frame.FetchInt32();
                    a = EvaluationStack.Pop();
                    frame.Locals[itmp] = a;
                    break;
                }

                case OpCode.LDG:
                {
                    itmp = CurrentStackFrame.FetchInt32();
                    a = RuntimeConstPool.FetchConst(itmp);

                    if (!GlobalTable.IsSet(a))
                    {
                        throw new GlobalNotFoundException(a);
                    }
                    
                    EvaluationStack.Push(GlobalTable.Get(a));
                    break;
                }

                case OpCode.STG:
                {
                    itmp = CurrentStackFrame.FetchInt32();
                    a = RuntimeConstPool.FetchConst(itmp);
                    b = EvaluationStack.Pop();
                    GlobalTable.Set(a, b);
                    break;
                }

                case OpCode.CALL:
                {
                    var argc = CurrentStackFrame.FetchInt32();
                    a = EvaluationStack.Pop();

                    switch (a.Type)
                    {
                        case DynamicValueType.Function:
                            InvokeChunk(a, argc);
                            break;
                        
                        case DynamicValueType.ClrFunction:
                            InvokeClrFunction(a, argc);
                            break;
                        
                        default: 
                            throw new NonInvokableValueException(a);
                    }

                    break;
                }

                case OpCode.STA:
                {
                    itmp = CurrentStackFrame.FetchInt32();
                    a = EvaluationStack.Pop();
                    CurrentStackFrame.FormalArguments[itmp] = a;
                    break;
                }

                case OpCode.LDA:
                {
                    itmp = CurrentStackFrame.FetchInt32();
                    EvaluationStack.Push(CurrentStackFrame.FormalArguments[itmp]);
                    break;
                }

                case OpCode.RETN:
                {
                    CallStack.Pop();
                    break;
                }

                case OpCode.XARGS:
                {
                    var argTable = new Table(CurrentStackFrame.ExtraArguments);
                    EvaluationStack.Push(new(argTable));
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

        private void InvokeClrFunction(DynamicValue clrFunction, int argc)
        {
            var clrFunc = clrFunction.ClrFunction;
            
            var args = new DynamicValue[argc];
            for (var i = 0; i < argc; i++)
            {
                args[argc - i - 1] = EvaluationStack.Pop();
            }
            
            CallStack.Push(new(clrFunc));
            {
                EvaluationStack.Push(
                    clrFunc.Invoke(this, args)
                );
            }
            CallStack.Pop();
        }

        private void InvokeChunk(DynamicValue chunk, int argc)
        {
            CallStack.Push(new(chunk.Function, argc));
            var extraArgs = CurrentStackFrame.ExtraArguments;

            if (extraArgs != null)
            {
                for (var i = 0; i < extraArgs.Length; i++)
                {
                    extraArgs[extraArgs.Length - i - 1] = EvaluationStack.Pop();
                }
            }
        }
    }
}