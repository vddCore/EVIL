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
                    return new("nil");
                case DynamicValueType.Number:
                    return new(a.Number.ToString(CultureInfo.InvariantCulture));
                case DynamicValueType.String:
                    return new(a.String!);
                case DynamicValueType.Boolean:
                    return new(a.Boolean.ToString().ToLowerInvariant());
                case DynamicValueType.Table:
                    return new DynamicValue($"Table[{a.Table!.Length}]");
                case DynamicValueType.Fiber:
                {
                    unsafe
                    {
                        var fib = a.Fiber!;
                        var reference = __makeref(fib);

                        return new DynamicValue($"Fiber[{**(IntPtr**)&reference}]");
                    }
                }
                case DynamicValueType.Chunk:
                    return new DynamicValue(a.Chunk!.Name!);
                case DynamicValueType.NativeFunction:
                    return new DynamicValue($"NativeFunction[{a.NativeFunction!.Method.Name}]");

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
                        return new(double.Parse(a.String!));
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
                    return new(a.Boolean ? 1 : 0);

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