using System;
using EVIL.CommonTypes.TypeSystem;

namespace EVIL.Ceres.ExecutionEngine.TypeSystem
{
    using EVIL.Ceres.ExecutionEngine.Marshaling;

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
                if (b.Type == DynamicValueType.String || b.Type == DynamicValueType.Number)
                {
                    return new(a.Table!.Contains(b));
                }
            }

            if (a.Type == DynamicValueType.Array)
            {
                return new(a.Array!.IndexOf(b) >= 0);
            }

            if (a.Type == DynamicValueType.Error)
            {
                return new(a.Error!.Contains(b));
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

            if (a.Type == DynamicValueType.Array)
            {
                return new(a.Array!.Length);
            }

            if (a.Type == DynamicValueType.Table)
            {
                return new(a.Table!.Length);
            }

            if (a.Type == DynamicValueType.Error)
            {
                return new(a.Error!.Length);
            }

            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to get length of a {a.Type}."
            );
        }

        public static DynamicValue Index(this DynamicValue a, DynamicValue key)
        {
            switch (a.Type)
            {
                case DynamicValueType.Table:
                    return a.Table![key];
                
                case DynamicValueType.NativeObject when a.NativeObject is IIndexableObject indexable:
                    return indexable.Index(key);

                case DynamicValueType.Array when key.Type != DynamicValueType.Number:
                    throw new UnsupportedDynamicValueOperationException(
                        $"Attempt to index an {a.Type} using a {key.Type}."
                    );

                case DynamicValueType.Array:
                    return a.Array![(int)key.Number];

                case DynamicValueType.String when key.Type != DynamicValueType.Number:
                    throw new UnsupportedDynamicValueOperationException(
                        $"Attempt to index a {a.Type} using a {key.Type}."
                    );

                case DynamicValueType.String when key.Number % 1 != 0:
                    throw new UnsupportedDynamicValueOperationException(
                        $"Attempt to index a {a.Type} using a fractional number."
                    );

                case DynamicValueType.String when key.Number < 0 || key.Number >= a.String!.Length:
                    return DynamicValue.Nil;

                case DynamicValueType.String:
                    return new(a.String![(int)key.Number]);

                case DynamicValueType.Chunk when key.Type != DynamicValueType.String:
                    throw new UnsupportedDynamicValueOperationException(
                        $"Chunks may only be indexed using a String."
                    );

                case DynamicValueType.Chunk:
                    return a.Chunk![key.String!] ?? DynamicValue.Nil;

                case DynamicValueType.Error:
                    return a.Error![key];
                
                default:
                    throw new UnsupportedDynamicValueOperationException(
                        $"Attempt to index a non-indexable {a.Type} value."
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