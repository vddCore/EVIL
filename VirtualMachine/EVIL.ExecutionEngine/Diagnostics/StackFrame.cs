using System.Diagnostics;
using System.Runtime.CompilerServices;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.Intermediate;

namespace EVIL.ExecutionEngine.Diagnostics
{
    public class StackFrame
    {
        private EVM _evm;

        public Chunk Chunk { get; }
        
        public DynamicValue[] Locals { get; }
        public DynamicValue[] Parameters { get; }

        public int IP { get; set; }
        public DynamicValue ReturnValue { get; private set; }

        public StackFrame(EVM evm, Chunk chunk)
        {
            _evm = evm;

            Chunk = chunk;
            
            Locals = new DynamicValue[chunk.LocalCount];
            Parameters = new DynamicValue[chunk.ParameterCount];
            ReturnValue = DynamicValue.Zero;
            
            for (var i = 0; i < Locals.Length; i++)
            {
                Locals[i] = DynamicValue.Zero;
            }
            
            for (var i = 0; i < Parameters.Length; i++)
            {
                Parameters[i] = DynamicValue.Zero;
            }
        }

        public OpCode FetchOpCode()
        {
            return (OpCode)FetchByte();
        }

        public double FetchNumber()
        {
            var data = FetchBytes(8);

            unsafe
            {
                fixed (byte* dbl = &data[0])
                    return *(double*)dbl;
            }
        }

        public int FetchInt32()
        {
            var data = FetchBytes(4);

            unsafe
            {
                fixed (byte* num = &data[0])
                    return *(int*)num;
            }
        }

        public uint FetchUInt32()
        {
            var data = FetchBytes(4);

            unsafe
            {
                fixed (byte* num = &data[0])
                    return *(uint*)num;
            }
        }

        public byte[] FetchBytes(int len)
        {
            var data = new byte[len];

            for (var i = 0; i < len; i++)
                data[i] = FetchByte();

            return data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte FetchByte()
        {
            Debug.Assert(IP < Chunk.Instructions.Count);
            return Chunk.Instructions[IP++];
        }
    }
}