using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.Ceres.Runtime.Extensions;
using static EVIL.CommonTypes.TypeSystem.DynamicValueType;

namespace EVIL.Ceres.Runtime.Modules
{
    public class ConvertModule : RuntimeModule
    {
        public override string FullyQualifiedName => "cnv";

        [RuntimeModuleFunction("u64")]
        [EvilDocFunction(
            "Casts the given number to an unsigned 64-bit integer.",
            Returns = "A 64-bit unsigned integer.",
            ReturnType = Number
        )]
        [EvilDocArgument("number", "A number to be converted.", Number)]
        private static DynamicValue U64(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var number);

            var ret = (ulong)number;
            return ret;
        }
        
        [RuntimeModuleFunction("u32")]
        [EvilDocFunction(
            "Casts the given number to an unsigned 32-bit integer. Remember to mask off the relevant bits.",
            Returns = "A 32-bit unsigned integer.",
            ReturnType = Number
        )]
        [EvilDocArgument("number", "A number to be converted.", Number)]
        private static DynamicValue U32(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var number);

            var ret = (uint)number;
            return ret;
        }
        
        [RuntimeModuleFunction("u16")]
        [EvilDocFunction(
            "Casts the given number to an unsigned 16-bit integer. Remember to mask off the relevant bits.",
            Returns = "A 16-bit unsigned integer.",
            ReturnType = Number
        )]
        [EvilDocArgument("number", "A number to be converted.", Number)]
        private static DynamicValue U16(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var number);

            var ret = (ushort)number;
            return ret;
        }
        
        [RuntimeModuleFunction("u8")]
        [EvilDocFunction(
            "Casts the given number to an unsigned 8-bit integer. Remember to mask off the relevant bits.",
            Returns = "An 8-bit unsigned integer.",
            ReturnType = Number
        )]
        [EvilDocArgument("number", "A number to be converted.", Number)]
        private static DynamicValue U8(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var number);

            var ret = (byte)number;
            return ret;
        }
        
        [RuntimeModuleFunction("i8")]
        [EvilDocFunction(
            "Casts the given number to a signed 8-bit integer. Remember to mask off the relevant bits.",
            Returns = "An 8-bit signed integer.",
            ReturnType = Number
        )]
        [EvilDocArgument("number", "A number to be converted.", Number)]
        private static DynamicValue I8(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var number);

            var ret = (sbyte)number;
            return ret;
        }
        
        [RuntimeModuleFunction("i16")]
        [EvilDocFunction(
            "Casts the given number to a signed 16-bit integer. Remember to mask off the relevant bits.",
            Returns = "A 16-bit signed integer.",
            ReturnType = Number
        )]
        [EvilDocArgument("number", "A number to be converted.", Number)]
        private static DynamicValue I16(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var number);

            var ret = (short)number;
            return ret;
        }
        
        [RuntimeModuleFunction("i32")]
        [EvilDocFunction(
            "Casts the given number to a signed 32-bit integer. Remember to mask off the relevant bits.",
            Returns = "A 32-bit signed integer.",
            ReturnType = Number
        )]
        [EvilDocArgument("number", "A number to be converted.", Number)]
        private static DynamicValue I32(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var number);

            var ret = (int)number;
            return ret;
        }
        
        [RuntimeModuleFunction("i64")]
        [EvilDocFunction(
            "Casts the given number to a signed 64-bit integer.",
            Returns = "A 64-bit signed integer.",
            ReturnType = Number
        )]
        [EvilDocArgument("number", "A number to be converted.", Number)]
        private static DynamicValue I64(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var number);

            var ret = (long)number;
            return ret;
        }
    }
}