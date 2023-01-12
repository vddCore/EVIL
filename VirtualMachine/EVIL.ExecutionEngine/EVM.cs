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

        public void Run()
        {
            Running = true;
            CallStack.Push(new StackFrame(this, Executable.MainChunk, 0));

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

            int itmp;

            switch (opCode)
            {
                case OpCode.NOP:
                {
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
                    itmp = frame.FetchInt32();
                    a = frame.Locals[itmp];
                    EvaluationStack.Push(a);
                    break;
                }

                case OpCode.STLOCAL:
                {
                    itmp = frame.FetchInt32();
                    a = EvaluationStack.Pop();
                    frame.Locals[itmp] = a;
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
                    var argc = CurrentStackFrame.FetchInt32();
                    a = EvaluationStack.Pop();

                    if (a.Type != DynamicValueType.Function)
                        throw new NonInvokableValueException(a);

                    CallStack.Push(new StackFrame(this, a.Function, argc));
                    var extraArgs = CurrentStackFrame.ExtraArguments;

                    if (extraArgs != null)
                    {
                        for (var i = 0; i < extraArgs.Length; i++)
                        {
                            extraArgs[extraArgs.Length - i - 1] = EvaluationStack.Pop();
                        }
                    }
                    break;
                }

                case OpCode.STARG:
                {
                    itmp = CurrentStackFrame.FetchInt32();
                    a = EvaluationStack.Pop();
                    CurrentStackFrame.FormalArguments[itmp] = a;
                    break;
                }

                case OpCode.LDARG:
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
    }
}