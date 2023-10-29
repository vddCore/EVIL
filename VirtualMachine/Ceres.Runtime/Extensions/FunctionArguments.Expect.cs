using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;

namespace Ceres.Runtime.Extensions
{
    public static partial class FunctionArguments
    {
        public static DynamicValue[] ExpectNilAt(this DynamicValue[] args, int index)
            => args.ExpectTypeAt(index, DynamicValueType.Nil);
        
        public static DynamicValue[] ExpectNumberAt(this DynamicValue[] args, int index, out double value)
        {
            args.ExpectTypeAt(index, DynamicValueType.Number);
            value = args[index].Number;

            return args;
        }

        public static DynamicValue[] ExpectIntegerAt(this DynamicValue[] args, int index, out long value)
        {
            args.ExpectTypeAt(index, DynamicValueType.Number);
            var num = args[index].Number;
            if (num % 1 != 0)
                throw new EvilRuntimeException($"Expected an integer at argument index {index}.");

            value = (long)args[index].Number;
            return args;
        }

        public static DynamicValue[] ExpectStringAt(this DynamicValue[] args, int index, out string value)
        {
            args.ExpectTypeAt(index, DynamicValueType.String);
            value = args[index].String!;

            return args;
        }

        public static DynamicValue[] ExpectCharAt(this DynamicValue[] args, int index, out char value)
        {
            args.ExpectTypeAt(index, DynamicValueType.String);

            var str = args[index].String!;
            if (str.Length != 1)
                throw new EvilRuntimeException($"Expected a single character at argument index {index}.");

            value = str[0];
            return args;
        }

        public static DynamicValue[] ExpectBooleanAt(this DynamicValue[] args, int index, out bool value)
        {
            args.ExpectTypeAt(index, DynamicValueType.Boolean);
            value = args[index].Boolean;

            return args;
        }

        public static DynamicValue[] ExpectTableAt(this DynamicValue[] args, int index, out Table value)
        {
            args.ExpectTypeAt(index, DynamicValueType.Table);
            value = args[index].Table!;

            return args;
        }

        public static DynamicValue[] ExpectArrayAt(this DynamicValue[] args, int index, out Array value)
        {
            args.ExpectTypeAt(index, DynamicValueType.Array);
            value = args[index].Array!;

            return args;
        }
        
        public static DynamicValue[] ExpectFiberAt(this DynamicValue[] args, int index, out Fiber value)
        {
            args.ExpectTypeAt(index, DynamicValueType.Fiber);
            value = args[index].Fiber!;

            return args;
        }

        public static DynamicValue[] ExpectChunkAt(this DynamicValue[] args, int index, out Chunk value)
        {
            args.ExpectTypeAt(index, DynamicValueType.Chunk);
            value = args[index].Chunk!;

            return args;
        }

        public static DynamicValue[] ExpectTypeCodeAt(this DynamicValue[] args, int index, out DynamicValueType value)
        {
            args.ExpectTypeAt(index, DynamicValueType.TypeCode);
            value = args[index].TypeCode;

            return args;
        }

        public static DynamicValue[] ExpectNativeFunctionAt(this DynamicValue[] args, int index,
            out NativeFunction value)
        {
            args.ExpectTypeAt(index, DynamicValueType.NativeFunction);
            value = args[index].NativeFunction!;

            return args;
        }

        public static DynamicValue[] ExpectNativeObjectAt(this DynamicValue[] args, int index, out object? value)
        {
            args.ExpectTypeAt(index, DynamicValueType.NativeObject);
            value = args[index].NativeObject;

            return args;
        }

        public static DynamicValue[] ExpectNativeObjectAt<T>(this DynamicValue[] args, int index, out T value)
        {
            args.ExpectTypeAt(index, DynamicValueType.NativeObject);
            
            if (args[index].NativeObject is not T tValue)
            {
                throw new EvilRuntimeException(
                    $"A NativeObject value is not the expected CLR type '{typeof(T).FullName}'."
                );
            }

            value = tValue;
            return args;
        }
    }
}