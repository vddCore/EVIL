using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Diagnostics;
using EVIL.ExecutionEngine.Interop;

namespace EVIL.Runtime.Library
{
    public partial class CoreModule
    {
        [ClrFunction("strace_s")]
        public static DynamicValue StraceString(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectNone();
            return new DynamicValue(ctx.DumpCallStack());
        }

        [ClrFunction("uint8")]
        public static DynamicValue Uint8Conv(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((byte)args[0].Number);
        }
        
        [ClrFunction("sint8")]
        public static DynamicValue Sint8Conv(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((sbyte)args[0].Number);
        }
        
        [ClrFunction("uint16")]
        public static DynamicValue Uint16Conv(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((ushort)args[0].Number);
        }
        
        [ClrFunction("sint16")]
        public static DynamicValue Sint16Conv(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((short)args[0].Number);
        }
        
        [ClrFunction("uint32")]
        public static DynamicValue Uint32Conv(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((uint)args[0].Number);
        }
        
        [ClrFunction("sint32")]
        public static DynamicValue Sint32Conv(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((int)args[0].Number);
        }
        
        [ClrFunction("uint64")]
        public static DynamicValue Uint64Conv(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((ulong)args[0].Number);
        }
        
        [ClrFunction("sint64")]
        public static DynamicValue Sint64Conv(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((long)args[0].Number);
        }
    }
}