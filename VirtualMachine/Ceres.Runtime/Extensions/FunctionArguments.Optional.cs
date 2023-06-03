using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace Ceres.Runtime.Extensions
{
    public static partial class FunctionArguments
    {
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
        
        public static DynamicValue[] OptionalStringAt(this DynamicValue[] args, int index, string defaultValue, out string value)
        {
            value = defaultValue;
            
            if (index < args.Length)
            {
                if (args[index] == Nil)
                    return args;
                
                args.ExpectStringAt(index, out value);
            }

            return args;
        }
        
        public static DynamicValue[] OptionalCharAt(this DynamicValue[] args, int index, char defaultValue, out char value)
        {
            value = defaultValue;
            
            if (index < args.Length)
            {
                if (args[index] == Nil)
                    return args;
                
                args.ExpectCharAt(index, out value);
            }

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
        
        public static DynamicValue[] OptionalTableAt(this DynamicValue[] args, int index, Table defaultValue, out Table value)
        {
            value = defaultValue;
            
            if (index < args.Length)
            {
                if (args[index] == Nil)
                    return args;
                
                args.ExpectTableAt(index, out value);
            }

            return args;
        }
        
        public static DynamicValue[] OptionalFiberAt(this DynamicValue[] args, int index, Fiber defaultValue, out Fiber value)
        {
            value = defaultValue;
            
            if (index < args.Length)
            {
                if (args[index] == Nil)
                    return args;
                
                args.ExpectFiberAt(index, out value);
            }

            return args;
        }
        
        public static DynamicValue[] OptionalChunkAt(this DynamicValue[] args, int index, Chunk defaultValue, out Chunk value)
        {
            value = defaultValue;
            
            if (index < args.Length)
            {
                if (args[index] == Nil)
                    return args;
                
                args.ExpectChunkAt(index, out value);
            }

            return args;
        }
        
        public static DynamicValue[] OptionalNativeFunctionAt(this DynamicValue[] args, int index, NativeFunction defaultValue, out NativeFunction value)
        {
            value = defaultValue;
            
            if (index < args.Length)
            {
                if (args[index] == Nil)
                    return args;
                
                args.ExpectNativeFunctionAt(index, out value);
            }

            return args;
        }
    }
}