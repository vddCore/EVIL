using System;
using System.Globalization;

using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace Ceres.ExecutionEngine.TypeSystem
{
    public static partial class DynamicValueOperations
    {
        public static DynamicValue Contains(this DynamicValue a, DynamicValue b)
        {
            if (a.Type == DynamicValueType.String)
            {
                if (b.Type == DynamicValueType.String)
                {
                    return new(a.String!.Contains(b.String!, StringComparison.InvariantCulture));
                }
            }

            if (a.Type == DynamicValueType.Table)
            {
                if (b.Type == DynamicValueType.String || b.Type  == DynamicValueType.Number)
                {
                    return new(a.Table!.Contains(b));
                }
            }

            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to check if a {a.Type} contains a {b.Type}."
            );
        }
        
        public static DynamicValue GetLength(this DynamicValue a)
        {
            if (a.Type == DynamicValueType.String)
            {
                return new(a.String!.Length);
            }

            if (a.Type == DynamicValueType.Table)
            {
                return new(a.Table!.Length);
            }

            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to get length of a {a.Type}."
            );
        }

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
                    throw new UnsupportedDynamicValueOperationException(
                        $"Attempt to convert a {a.Type} to string."
                    );
            }
        }

        public static DynamicValue ShiftLeft(this DynamicValue a, DynamicValue b)
        {
            if (b.Type != DynamicValueType.Number)
            {
                throw new UnsupportedDynamicValueOperationException(
                    $"Attempt to left-shift a {a.Type} by a {b.Type}."
                );
            }

            if (a.Type == DynamicValueType.Number)
            {
                return new((long)a.Number << (int)b.Number);
            }

            if (a.Type == DynamicValueType.String)
            {
                var amount = (int)b.Number;
                var str = a.String!;

                if (amount >= str.Length)
                {
                    return DynamicValue.EmptyString;
                }

                return new(str.Substring(amount));
            }

            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to left-shift a {a.Type}."
            );
        }

        public static DynamicValue ShiftRight(this DynamicValue a, DynamicValue b)
        {
            if (b.Type != DynamicValueType.Number)
            {
                throw new UnsupportedDynamicValueOperationException(
                    $"Attempt to right-shift a {a.Type} by a {b.Type}."
                );
            }

            if (a.Type == DynamicValueType.Number)
            {
                return new((long)a.Number >> (int)b.Number);
            }

            if (a.Type == DynamicValueType.String)
            {
                var str = a.String!;
                var amount = str.Length - (int)b.Number;

                if (amount <= 0)
                {
                    return DynamicValue.EmptyString;
                }

                return new(str.Substring(0, amount));
            }

            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to right-shift a {a.Type}."
            );
        }
    }
}