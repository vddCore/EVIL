using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace Ceres.Runtime.Extensions
{
    public static class FunctionArgumentExtensions
    {
        public static Table ToTable(this DynamicValue[] args)
        {
            var ret = new Table();

            for (var i = 0; i < args.Length; i++)
                ret[i] = args[i];
            
            return ret;
        }
        
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

        public static DynamicValue[] ExpectIntegerAt(this DynamicValue[] args, int index, out long value)
        {
            args.ExpectTypeAt(index, DynamicValueType.Number);
            var num = args[index].Number;
            if (num % 1 != 0)
                throw new EvilRuntimeException($"Expected an integer at argument index {index}.");
            
            value = (long)args[index].Number;
            return args;
        }
        
        public static DynamicValue[] OptionalIntegerAt(this DynamicValue[] args, int index, long defaultValue, out long value)
        {
            value = defaultValue;
            
            if (index < args.Length)
            {
                if (args[index] == Nil)
                    return args;
                
                args.ExpectIntegerAt(index, out value);
            }

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
        
        public static DynamicValue[] OptionalBooleanAt(this DynamicValue[] args, int index, bool defaultValue, out bool value)
        {
            value = defaultValue;
            
            if (index < args.Length)
            {
                if (args[index] == Nil)
                    return args;
                
                args.ExpectBooleanAt(index, out value);
            }

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