namespace EVIL.Ceres.Runtime.Extensions;

using static EVIL.Ceres.ExecutionEngine.TypeSystem.DynamicValue;

using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;

public static partial class FunctionArguments
{
    public static DynamicValue[] OptionalNumberAt(this DynamicValue[] args, int index, double defaultValue, out double value)
    {
        value = defaultValue;
            
        if (index < args.Length)
        {
            if (args[index] == Nil)
                return args;
                
            args.ExpectNumberAt(index, out value);
        }

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

    public static DynamicValue[] OptionalArrayAt(this DynamicValue[] args, int index, Array defaultValue, out Array value)
    {
        value = defaultValue;
            
        if (index < args.Length)
        {
            if (args[index] == Nil)
                return args;

            args.ExpectArrayAt(index, out value);
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

    public static DynamicValue[] OptionalTypeCodeAt(this DynamicValue[] args, int index, DynamicValueType defaultValue, out DynamicValueType value)
    {
        value = defaultValue;

        if (index < args.Length)
        {
            if (args[index] == Nil)
                return args;

            args.ExpectTypeCodeAt(index, out value);
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
        
    public static DynamicValue[] OptionalNativeObjectAt(this DynamicValue[] args, int index, object? defaultValue, out object? value)
    {
        value = defaultValue;
            
        if (index < args.Length)
        {
            if (args[index] == Nil)
                return args;
                
            args.ExpectNativeObjectAt(index, out value);
        }

        return args;
    }

    public static DynamicValue[] OptionalNativeObjectAt<T>(this DynamicValue[] args, int index, T defaultValue, out T value)
    {
        value = defaultValue;

        if (index < args.Length)
        {
            if (args[index] == Nil)
                return args;

            args.ExpectNativeObjectAt(index, out value);
        }

        return args;
    }
}