using System.Runtime.CompilerServices;
using System.Text;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Interop;
using EVIL.Intermediate.CodeGeneration;

namespace EVIL.ExecutionEngine.Diagnostics
{
    public class StackFrame
    {
        private byte[] _internalBuffer = new byte[16];
        
        public int IP { get; private set; }
        public bool CanExecute => IP < Chunk.Instructions.Count;

        public ClrFunction ClrFunction { get; }

        public ExecutionContext ExecutionContext { get; }
        public Chunk Chunk { get; }
        
        public DynamicValue[] Locals { get; }
        public DynamicValue[] FormalArguments { get; }
        public DynamicValue[] ExtraArguments { get; }

        public StackFrame(ExecutionContext ctx, Chunk chunk, int argCount)
        {
            ExecutionContext = ctx;
            Chunk = chunk;
            
            Locals = new DynamicValue[chunk.Locals.Count];
            FormalArguments = new DynamicValue[chunk.Parameters.Count];
            
            var extraArgCount = argCount - FormalArguments.Length;
            if (extraArgCount > 0)
            {
                ExtraArguments = new DynamicValue[extraArgCount];
            }
            
            for (var i = 0; i < Locals.Length; i++)
            {
                Locals[i] = DynamicValue.Zero;
            }
            
            for (var i = 0; i < FormalArguments.Length; i++)
            {
                FormalArguments[i] = DynamicValue.Zero;
            }
        }

        public StackFrame(ClrFunction clrFunction)
        {
            ClrFunction = clrFunction;
        }

        public void Jump(int addr)
        {
            if (addr >= Chunk.Instructions.Count)
                throw new AddressOutOfBoundsException(ExecutionContext, this, addr);

            IP = addr;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (Chunk != null)
            {
                sb.Append(Chunk.Name);
                sb.Append("(");
                sb.Append(string.Join(',', Chunk.Parameters));
                sb.Append($") @ {IP:X8}");
                
                var (line, _) = Chunk.GetCodeCoordinatesForInstructionPointer(IP);
                
                if (line > 0)
                {
                    sb.Append($": line {line}");
                }
            }
            else if (ClrFunction != null)
            {
                var attribs = ClrFunction.Method.GetCustomAttributes(typeof(ClrFunctionAttribute), false);

                if (attribs.Length > 0 
                    && attribs[0] is ClrFunctionAttribute clrFuncAttrib
                    && !string.IsNullOrEmpty(clrFuncAttrib.RuntimeAlias))
                {
                    sb.Append($"CLR!{clrFuncAttrib.RuntimeAlias}");
                }
                else
                {
                    sb.Append($"CLR!{ClrFunction.Method.Name}");
                }
            }

            return sb.ToString();
        }
        
        internal OpCode FetchOpCode()
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
            for (var i = 0; i < len; i++)
                _internalBuffer[i] = FetchByte();

            return _internalBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal byte FetchByte()
        {
            return Chunk.Instructions[IP++];
        }
    }
}