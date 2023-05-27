using System;
using System.Globalization;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace Ceres.ExecutionEngine.TypeSystem
{
    public static partial class DynamicValueOperations
    {
        public static DynamicValue ConvertToString(this DynamicValue a)
        {
            switch (a.Type)
            {
                case DynamicValueType.Nil:
                    return "nil";
                case DynamicValueType.Number:
                    return a.Number.ToString(CultureInfo.InvariantCulture);
                case DynamicValueType.String:
                    return a.String!;
                case DynamicValueType.Boolean:
                    return a.Boolean.ToString().ToLowerInvariant();
                case DynamicValueType.Table:
                    return $"Table[{a.Table!.Length}]";
                case DynamicValueType.Fiber:
                {
                    unsafe
                    {
                        var fib = a.Fiber!;
                        var reference = __makeref(fib);

                        return $"Fiber[{**(IntPtr**)&reference}]";
                    }
                }
                case DynamicValueType.Chunk:
                    return a.Chunk!.Name!;
                case DynamicValueType.NativeFunction:
                    return $"NativeFunction[{a.NativeFunction!.Method.Name}]";
                case DynamicValueType.NativeObject:
                    return $"NativeObject[{a.NativeObject}]";
                default:
                {
                    throw new UnsupportedDynamicValueOperationException(
                        $"Attempt to convert a {a.Type} to String."
                    );
                }
            }
        }

        public static DynamicValue ConvertToNumber(this DynamicValue a)
        {
            switch (a.Type)
            {
                case DynamicValueType.String:
                {
                    try
                    {
                        return double.Parse(a.String!);
                    }
                    catch (FormatException fe)
                    {
                        throw new MalformedNumberException(
                            $"Attempt to convert '{a.String}' to a number.",
                            fe
                        );
                    }
                }

                case DynamicValueType.Boolean:
                    return a.Boolean ? 1 : 0;

                default:
                {
                    throw new UnsupportedDynamicValueOperationException(
                        $"Attempt to convert a {a.Type} to Number."
                    );
                }
            }
        }
    }
}