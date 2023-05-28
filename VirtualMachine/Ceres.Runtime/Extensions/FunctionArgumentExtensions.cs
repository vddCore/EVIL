using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace Ceres.Runtime.Extensions
{
    public static class FunctionArgumentExtensions
    {
        public static DynamicValue[] ExpectExactly(this DynamicValue[] args, int count)
        {
            if (count != args.Length)
                throw new EvilRuntimeException($"Expected {(count == 0 ? "no" : count)} arguments, found {args.Length}.");

            return args;
        }

        public static DynamicValue[] ExpectAtLeast(this DynamicValue[] args, int count)
        {
            if (args.Length < count)
                throw new EvilRuntimeException($"Expected at least {count} arguments, found {args.Length}.");

            return args;
        }

        public static DynamicValue[] ExpectAtMost(this DynamicValue[] args, int count)
        {
            if (args.Length > count)
                throw new EvilRuntimeException($"Expected at most {count} arguments, found {args.Length}.");

            return args;
        }

        public static DynamicValue[] ExpectNone(this DynamicValue[] args)
            => args.ExpectExactly(0);

        public static DynamicValue[] ExpectTypeAt(this DynamicValue[] args, int index, DynamicValueType type)
        {
            args.ExpectAtLeast(index + 1);

            var argType = args[index].Type;
            
            if (argType != type)
                throw new EvilRuntimeException($"Expected a {type} at argument index {index}, found {argType}.");

            return args;
        }

        public static DynamicValue[] ExpectNilAt(this DynamicValue[] args, int index)
            => args.ExpectTypeAt(index, DynamicValueType.Nil);

        public static DynamicValue[] ExpectNumberAt(this DynamicValue[] args, int index, out double value)
        {
            args.ExpectTypeAt(index, DynamicValueType.Number);
            value = args[index].Number;

            return args;
        }

        public static DynamicValue[] ExpectStringAt(this DynamicValue[] args, int index, out string value)
        {
            args.ExpectTypeAt(index, DynamicValueType.String);
            value = args[index].String!;
            
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

        public static DynamicValue[] ExpectNativeFunctionAt(this DynamicValue[] args, int index, out NativeFunction value)
        {
            args.ExpectTypeAt(index, DynamicValueType.NativeFunction);
            value = args[index].NativeFunction!;
            
            return args;
        }
    }
}