namespace EVIL.Ceres.Runtime.Extensions;

using System.Diagnostics.CodeAnalysis;
using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;

public static partial class FunctionArguments
{
    public static DynamicValue[] ExpectTypeAt(this DynamicValue[] args, int index, DynamicValueType type, bool allowNil = false)
    {
        args.ExpectAtLeast(index + 1);

        var argType = args[index].Type;

        if (argType != type)
        {
            if (allowNil)
            {
                if (argType == DynamicValueType.Nil)
                {
                    return args;
                }
                else
                {
                    throw new EvilRuntimeException($"Expected a {type} or {DynamicValueType.Nil} at argument index {index}, found {argType}.");
                }
            }

            throw new EvilRuntimeException($"Expected a {type} at argument index {index}, found {argType}.");
        }

        return args;
    }

    public static DynamicValue[] ExpectOneOfTypesAt(
        this DynamicValue[] args, 
        int index,
        out DynamicValue value,
        DynamicValueType firstType,
        params DynamicValueType[] types)
    {
        args.ExpectAtLeast(index + 1);

        var argType = args[index].Type;

        if (firstType != argType)
        {
            if (System.Array.IndexOf(types, argType) < 0)
            {
                throw new EvilRuntimeException(
                    $"Expected one of the following types: [{string.Join(',', types)}] " +
                    $"at argument index {index}, found {argType}."
                );
            }
        }

        value = args[index];

        return args;
    }
        
    public static DynamicValue[] ExpectNilAt(this DynamicValue[] args, int index)
        => args.ExpectTypeAt(index, DynamicValueType.Nil);
        
    public static DynamicValue[] ExpectAnyAt(this DynamicValue[] args, int index, out DynamicValue value)
    {
        args.ExpectAtLeast(index + 1);
        value = args[index];
            
        return args;
    }
        
    public static DynamicValue[] ExpectNumberAt(
        this DynamicValue[] args,
        int index,
        out double value,
        bool allowNil = false)
    {
        args.ExpectTypeAt(index, DynamicValueType.Number, allowNil);
        value = args[index].Number;

        return args;
    }

    public static DynamicValue[] ExpectIntegerAt(
        this DynamicValue[] args,
        int index,
        out long value,
        bool allowNil = false)
    {
        args.ExpectTypeAt(index, DynamicValueType.Number, allowNil);
            
        var num = args[index].Number;
        if (num % 1 != 0)
            throw new EvilRuntimeException($"Expected an integer at argument index {index}.");

        value = (long)args[index].Number;
        return args;
    }

    public static DynamicValue[] ExpectStringAt(
        this DynamicValue[] args, 
        int index, 
        [AllowNull] out string value,
        bool allowNil = false)
    {
        args.ExpectTypeAt(index, DynamicValueType.String, allowNil);
        value = args[index].String!;

        return args;
    }

    public static DynamicValue[] ExpectCharAt(
        this DynamicValue[] args,
        int index,
        out char value,
        bool allowNil = false)
    {
        args.ExpectTypeAt(index, DynamicValueType.String, allowNil);

        var str = args[index].String!;
        if (str.Length != 1)
            throw new EvilRuntimeException($"Expected a single character at argument index {index}.");

        value = str[0];
        return args;
    }

    public static DynamicValue[] ExpectBooleanAt(
        this DynamicValue[] args,
        int index,
        out bool value,
        bool allowNil = false)
    {
        args.ExpectTypeAt(index, DynamicValueType.Boolean, allowNil);
        value = args[index].Boolean;

        return args;
    }

    public static DynamicValue[] ExpectTableAt(
        this DynamicValue[] args,
        int index,
        out Table value,
        bool allowNil = false)
    {
        args.ExpectTypeAt(index, DynamicValueType.Table, allowNil);
        value = args[index].Table!;

        return args;
    }

    public static DynamicValue[] ExpectArrayAt(
        this DynamicValue[] args,
        int index, 
        out Array value,
        bool allowNil = false)
    {
        args.ExpectTypeAt(index, DynamicValueType.Array, allowNil);
        value = args[index].Array!;

        return args;
    }
        
    public static DynamicValue[] ExpectFiberAt(
        this DynamicValue[] args,
        int index,
        out Fiber value,
        bool allowNil = false)
    {
        args.ExpectTypeAt(index, DynamicValueType.Fiber, allowNil);
        value = args[index].Fiber!;

        return args;
    }

    public static DynamicValue[] ExpectChunkAt(
        this DynamicValue[] args,
        int index,
        out Chunk value,
        bool allowNil = false)
    {
        args.ExpectTypeAt(index, DynamicValueType.Chunk, allowNil);
        value = args[index].Chunk!;

        return args;
    }

    public static DynamicValue[] ExpectTypeCodeAt(
        this DynamicValue[] args,
        int index,
        out DynamicValueType value,
        bool allowNil = false)
    {
        args.ExpectTypeAt(index, DynamicValueType.TypeCode, allowNil);
        value = args[index].TypeCode;

        return args;
    }

    public static DynamicValue[] ExpectNativeFunctionAt(
        this DynamicValue[] args, 
        int index,
        out NativeFunction value,
        bool allowNil = false)
    {
        args.ExpectTypeAt(index, DynamicValueType.NativeFunction, allowNil);
        value = args[index].NativeFunction!;

        return args;
    }

    public static DynamicValue[] ExpectNativeObjectAt(
        this DynamicValue[] args,
        int index,
        out object? value,
        bool allowNil = false)
    {
        args.ExpectTypeAt(index, DynamicValueType.NativeObject, allowNil);
        value = args[index].NativeObject;

        return args;
    }

    public static DynamicValue[] ExpectNativeObjectAt<T>(
        this DynamicValue[] args,
        int index,
        out T value,
        bool allowNil = false)
    {
        args.ExpectTypeAt(index, DynamicValueType.NativeObject, allowNil);
            
        if (args[index].NativeObject is not T tValue)
        {
            if (args[index].Type == DynamicValueType.Nil && allowNil)
            {
                value = default(T)!;
                return args;
            }

            throw new EvilRuntimeException(
                $"A NativeObject value is not the expected CLR type '{typeof(T).FullName}'."
            );
        }

        value = tValue;
        return args;
    }
}