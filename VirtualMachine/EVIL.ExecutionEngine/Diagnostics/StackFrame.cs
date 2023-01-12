using System.Diagnostics;
using System.Runtime.CompilerServices;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.Intermediate;

namespace EVIL.ExecutionEngine.Diagnostics
{
    public class StackFrame
    {
        private EVM _evm;

        public int IP { get; private set; }
        public bool CanExecute => IP < Chunk.Instructions.Count;
        
        public Chunk Chunk { get; }
        
        public DynamicValue[] Locals { get; }
        public DynamicValue[] FormalArguments { get; }
        public DynamicValue[] ExtraArguments { get; }

        public DynamicValue ReturnValue { get; set; }

        public StackFrame(EVM evm, Chunk chunk, int argCount)
        {
            _evm = evm;
            
            Chunk = chunk;
            Locals = new DynamicValue[chunk.LocalCount];
            FormalArguments = new DynamicValue[chunk.ParameterCount];
            
            var extraArgCount = argCount - FormalArguments.Length;
            if (extraArgCount > 0)
            {
                ExtraArguments = new DynamicValue[extraArgCount];
            }
            
            ReturnValue = DynamicValue.Zero;
            
            for (var i = 0; i < Locals.Length; i++)
            {
                Locals[i] = DynamicValue.Zero;
            }
            
            for (var i = 0; i < FormalArguments.Length; i++)
            {
                FormalArguments[i] = DynamicValue.Zero;
            }
        }

        public OpCode FetchOpCode()
        {
            return (OpCode)FetchByte();
        }

        internal double FetchNumber()
        {
            var data = FetchBytes(8);

            unsafe
            {
                fixed (byte* dbl = &data[0])
                    return *(double*)dbl;
            }
        }

        internal int FetchInt32()
        {
            var data = FetchBytes(4);

            unsafe
            {
                fixed (byte* num = &data[0])
                    return *(int*)num;
            }
        }

        internal uint FetchUInt32()
        {
            var data = FetchBytes(4);

            unsafe
            {
                fixed (byte* num = &data[0])
                    return *(uint*)num;
            }
        }

        internal byte[] FetchBytes(int len)
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