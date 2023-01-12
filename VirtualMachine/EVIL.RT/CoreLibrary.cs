using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Interop;
using EVIL.RT;

namespace EVIL.Interpreter.Runtime.Library
{
    public class CoreLibrary
    {
        [ClrFunction("type")]
        public static DynamicValue Type(EVM evm, params DynamicValue[] args)
        {
            args.ExpectExactly(1);
            return new(args[0].Type.ToString().ToLower());
        }

        [ClrFunction("strace_s")]
        public static DynamicValue StraceString(EVM evm, params DynamicValue[] args)
        {
            args.ExpectNone();
            return new DynamicValue(evm.DumpCallStack());
        }

        [ClrFunction("setglobal")]
        public static DynamicValue SetGlobal(EVM evm, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynamicValueType.String);

            evm.GlobalTable.Set(args[0], args[1]);
            return args[1];
        }

        [ClrFunction("uint8")]
        public static DynamicValue Uint8Conv(EVM evm, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((byte)args[0].Number);
        }
        
        [ClrFunction("sint8")]
        public static DynamicValue Sint8Conv(EVM evm, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((sbyte)args[0].Number);
        }
        
        [ClrFunction("uint16")]
        public static DynamicValue Uint16Conv(EVM evm, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((ushort)args[0].Number);
        }
        
        [ClrFunction("sint16")]
        public static DynamicValue Sint16Conv(EVM evm, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((short)args[0].Number);
        }
        
        [ClrFunction("uint32")]
        public static DynamicValue Uint32Conv(EVM evm, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((uint)args[0].Number);
        }
        
        [ClrFunction("sint32")]
        public static DynamicValue Sint32Conv(EVM evm, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((int)args[0].Number);
        }
        
        [ClrFunction("uint64")]
        public static DynamicValue Uint64Conv(EVM evm, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((ulong)args[0].Number);
        }
        
        [ClrFunction("sint64")]
        public static DynamicValue Sint64Conv(EVM evm, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((long)args[0].Number);
        }
    }
}