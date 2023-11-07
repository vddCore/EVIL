using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using EVIL.CommonTypes.TypeSystem;

namespace Ceres.Runtime.Modules
{
    public class ConvertModule : RuntimeModule
    {
        public override string FullyQualifiedName => "cnv";

        [RuntimeModuleFunction("u64", ReturnType = DynamicValueType.Number)]
        private static DynamicValue U64(Fiber fiber, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var number);

            var ret = (ulong)number;
            return ret;
        }
        
        [RuntimeModuleFunction("u32", ReturnType = DynamicValueType.Number)]
        private static DynamicValue U32(Fiber fiber, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var number);

            var ret = (uint)number;
            return ret;
        }
        
        [RuntimeModuleFunction("u16", ReturnType = DynamicValueType.Number)]
        private static DynamicValue U16(Fiber fiber, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var number);

            var ret = (ushort)number;
            return ret;
        }
        
        [RuntimeModuleFunction("u8", ReturnType = DynamicValueType.Number)]
        private static DynamicValue U8(Fiber fiber, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var number);

            var ret = (byte)number;
            return ret;
        }
        
        [RuntimeModuleFunction("i8", ReturnType = DynamicValueType.Number)]
        private static DynamicValue I8(Fiber fiber, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var number);

            var ret = (sbyte)number;
            return ret;
        }
        
        [RuntimeModuleFunction("i16", ReturnType = DynamicValueType.Number)]
        private static DynamicValue I16(Fiber fiber, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var number);

            var ret = (short)number;
            return ret;
        }
        
        [RuntimeModuleFunction("i32", ReturnType = DynamicValueType.Number)]
        private static DynamicValue I32(Fiber fiber, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var number);

            var ret = (int)number;
            return ret;
        }
        
        [RuntimeModuleFunction("i64", ReturnType = DynamicValueType.Number)]
        private static DynamicValue I64(Fiber fiber, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var number);

            var ret = (long)number;
            return ret;
        }
    }
}