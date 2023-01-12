using System;
using System.Collections.Generic;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Diagnostics;
using EVIL.Intermediate;

namespace EVIL.ExecutionEngine
{
    public class EVM
    {
        private Stack<DynamicValue> EvaluationStack { get; } = new();
        private RuntimeConstPool RuntimeConstPool { get; }
        private Table GlobalTable { get; } = new();

        private Stack<StackFrame> CallStack { get; } = new();
        private StackFrame CurrentStackFrame => CallStack.Peek();
        
        public Executable Executable { get; }

        public bool Running { get; private set; }

        public EVM(Executable executable)
        {
            Executable = executable;
            RuntimeConstPool = new RuntimeConstPool(Executable.ConstPool);

            foreach (var c in executable.Chunks)
            {
                if(!c.Name.StartsWith('!'))
                    GlobalTable.Set(new DynamicValue(c.Name), new DynamicValue(c));
            }
        }

        public void JumpToChunk(Chunk chunk)
        {
            CallStack.Push(new StackFrame(this, chunk));
        }

        public void Return()
        {
            EvaluationStack.Push(
                CurrentStackFrame.ReturnValue
            );

            CallStack.Pop();
        }

        public void Run()
        {
            Running = true;

            CallStack.Push(new StackFrame(this, Executable.MainChunk));
            
            while (Running)
                Step();
        }

        public void Step()
        {
            var frame = CurrentStackFrame;
            var opCode = frame.FetchOpCode();

            DynamicValue a;
            DynamicValue b;
            
            uint utmp;
            int itmp;

            switch (opCode)
            {
                case OpCode.NOP:
                    break;

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
                    EvaluationStack.Push(new DynamicValue(-a.Number));
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

                    EvaluationStack.Push(
                        new DynamicValue(
                            a.Number + b.Number
                        )
                    );
                    break;
                }

                case OpCode.SUB:
                {
                    b = EvaluationStack.Pop();
                    a = EvaluationStack.Pop();

                    EvaluationStack.Push(
                        new DynamicValue(
                            a.Number - b.Number
                        )
                    );
                    break;
                }

                case OpCode.MUL:
                {
                    b = EvaluationStack.Pop();
                    a = EvaluationStack.Pop();

                    EvaluationStack.Push(
                        new DynamicValue(
                            a.Number * b.Number
                        )
                    );
                    break;
                }

                case OpCode.DIV:
                {
                    b = EvaluationStack.Pop();
                    a = EvaluationStack.Pop();

                    if (b.Number == 0)
                        throw new VirtualMachineException("Attempt to divide by zero.");

                    EvaluationStack.Push(
                        new DynamicValue(
                            a.Number / b.Number
                        )
                    );
                    break;
                }

                case OpCode.MOD:
                {
                    b = EvaluationStack.Pop();
                    a = EvaluationStack.Pop();

                    if (b.Number == 0)
                        throw new VirtualMachineException("Attempt to divide by zero.");

                    EvaluationStack.Push(
                        new DynamicValue(
                            a.Number - b.Number * Math.Floor(a.Number / b.Number)
                        )
                    );
                    break;
                }

                case OpCode.LDCONST:
                {
                    itmp = frame.FetchInt32();
                    EvaluationStack.Push(
                        new(RuntimeConstPool.FetchConst(itmp))
                    );
                    break;
                }

                case OpCode.LDLOCAL:
                {
                    utmp = frame.FetchUInt32();
                    a = frame.Locals[utmp];
                    EvaluationStack.Push(a);
                    break;
                }

                case OpCode.STLOCAL:
                {
                    utmp = frame.FetchUInt32();
                    a = EvaluationStack.Pop();
                    
                    frame.Locals[utmp] = a;
                    break;
                }

                case OpCode.LDGLOBAL:
                {
                    a = EvaluationStack.Pop();
                    EvaluationStack.Push(GlobalTable.Get(a));
                    break;
                }

                case OpCode.STGLOBAL:
                {
                    b = EvaluationStack.Pop();
                    a = EvaluationStack.Pop();
                    GlobalTable.Set(a, b);
                    break;
                }

                case OpCode.CALL:
                {
                    a = EvaluationStack.Pop();

                    if (a.Type != DynamicValueType.Function)
                        throw new NonInvokableValueException(a);
                    
                    JumpToChunk(a.Function);
                    break;
                }

                case OpCode.STARG:
                {
                    itmp = CurrentStackFrame.FetchInt32();
                    a = EvaluationStack.Pop();

                    CurrentStackFrame.Parameters[itmp] = a;
                    break;
                }

                case OpCode.LDARG:
                {
                    itmp = CurrentStackFrame.FetchInt32();
                    EvaluationStack.Push(CurrentStackFrame.Parameters[itmp]);
                    break;
                }
                
                default:
                    throw new VirtualMachineException("Invalid op-code.");
            }
        }

        public void Halt()
        {
            Running = false;
        }
    }
}