using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.Intermediate;

namespace EVIL.ExecutionEngine
{
    public class EVM
    {
        private Stack<DynamicValue> EvaluationStack { get; } = new();
        
        public int IP { get; set; }
        public byte[] Program { get; private set; }
        public bool Running { get; private set; }

        public EVM()
        {
        }
        
        public void LoadProgram(byte[] program)
        {
            IP = 0;
            Program = new byte[program.Length];

            Buffer.BlockCopy(program, 0, Program, 0, program.Length);
        }

        public void Run()
        {
            Running = true;

            while (Running)
                Step();
        }

        public void Step()
        {
            var opCode = (OpCode)FetchByte();

            DynamicValue a;
            DynamicValue b;

            switch (opCode)
            {
                case OpCode.NOP:
                    break;

                case OpCode.POP:
                    if (EvaluationStack.Count <= 0)
                        throw new VirtualMachineException("Evaluation stack underflow.");
                    
                    EvaluationStack.Pop();
                    break;

                case OpCode.HLT:
                    Halt();
                    break;

                case OpCode.ADD:
                    b = EvaluationStack.Pop();
                    a = EvaluationStack.Pop();
            
                    EvaluationStack.Push(new DynamicValue(a.Number + b.Number));
                    break;

                case OpCode.SUB:
                    b = EvaluationStack.Pop();
                    a = EvaluationStack.Pop();

                    EvaluationStack.Push(new DynamicValue(a.Number - b.Number));
                    break;
                    
                case OpCode.MUL:
                    b = EvaluationStack.Pop();
                    a = EvaluationStack.Pop();

                    EvaluationStack.Push(new DynamicValue(a.Number * b.Number));
                    break;

                case OpCode.DIV:
                    b = EvaluationStack.Pop();
                    a = EvaluationStack.Pop();

                    if (b.Number == 0)
                        throw new VirtualMachineException("Attempt to divide by zero.");

                    EvaluationStack.Push(new DynamicValue(a.Number / b.Number));
                    break;
                
                case OpCode.MOD:
                    b = EvaluationStack.Pop();
                    a = EvaluationStack.Pop();

                    if (b.Number == 0)
                        throw new VirtualMachineException("Attempt to divide by zero.");

                    EvaluationStack.Push(new DynamicValue(a.Number - b.Number * Math.Floor(a.Number / b.Number)));
                    break;

                case OpCode.LDNUM:
                    var number = FetchNumber();
                    EvaluationStack.Push(new(number));
                    break;
                
                default:
                    throw new VirtualMachineException("Invalid op-code.");
            }
        }

        public void Halt()
        {
            Running = false;
            IP = 0;
        }
        
        private double FetchNumber()
        {
            var data = FetchBytes(8);
            
            unsafe
            {
                fixed (double* dbl = &data[0])
                    return *dbl;
            }
        }

        private int FetchInt32()
        {
            var data = FetchBytes(4);

            unsafe
            {
                fixed (int* num = &data[0])
                    return *num;
            }
        }

        private uint FetchUInt32()
        {
            var data = FetchBytes(4);

            unsafe
            {
                fixed (uint* num = &data[0])
                    return *num;
            }
        }

        private byte[] FetchBytes(int len)
        {
            var data = new byte[len];

            for (var i = 0; i < len; i++)
                data[i] = FetchByte();

            return data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte FetchByte()
            => Program[IP++];
    }
}