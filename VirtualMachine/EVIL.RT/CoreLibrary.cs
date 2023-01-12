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
    }
}