using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace Ceres.ExecutionEngine.TypeSystem
{
    public static partial class DynamicValueOperations
    {
        
        public static DynamicValue IsEqualTo(this DynamicValue a, DynamicValue b)
        {
            if (a.Type != b.Type)
                return False;

            switch (a.Type)
            {
                case DynamicValueType.Nil:
                    return new(b.Type == DynamicValueType.Nil);

                case DynamicValueType.Number:
                    return new(a.Number.Equals(b.Number));

                case DynamicValueType.String:
                    return new(a.String == b.String);

                case DynamicValueType.Boolean:
                    return new DynamicValue(a.Boolean == b.Boolean);

                case DynamicValueType.Table:
                    return new DynamicValue(a.Table == b.Table);

                case DynamicValueType.Fiber:
                    return new DynamicValue(a.Fiber == b.Fiber);

                case DynamicValueType.Chunk:
                    return new DynamicValue(a.Chunk == b.Chunk);

                case DynamicValueType.NativeFunction:
                    return new DynamicValue(a.NativeFunction == b.NativeFunction);
            }

            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to compare (==) a {a.Type} with a {b.Type}."
            );
        }

        public static DynamicValue IsNotEqualTo(this DynamicValue a, DynamicValue b)
        {
            if (a.Type != b.Type)
                return False;

            switch (a.Type)
            {
                case DynamicValueType.Nil:
                    return new(b.Type != DynamicValueType.Nil);

                case DynamicValueType.Number:
                    return new(!a.Number.Equals(b.Number));

                case DynamicValueType.String:
                    return new(a.String != b.String);

                case DynamicValueType.Boolean:
                    return new DynamicValue(a.Boolean != b.Boolean);

                case DynamicValueType.Table:
                    return new DynamicValue(a.Table != b.Table);

                case DynamicValueType.Fiber:
                    return new DynamicValue(a.Fiber != b.Fiber);

                case DynamicValueType.Chunk:
                    return new DynamicValue(a.Chunk != b.Chunk);

                case DynamicValueType.NativeFunction:
                    return new DynamicValue(a.NativeFunction != b.NativeFunction);
            }

            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to compare (!=) a {a.Type} with a {b.Type}."
            );
        }

        public static DynamicValue IsGreaterThan(this DynamicValue a, DynamicValue b)
        {
            if (a.Type == DynamicValueType.Number && b.Type == DynamicValueType.Number)
            {
                return new(a.Number > b.Number);
            }

            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to compare (>) a {a.Type} with a {b.Type}."
            );
        }

        public static DynamicValue IsLessThan(this DynamicValue a, DynamicValue b)
        {
            if (a.Type == DynamicValueType.Number && b.Type == DynamicValueType.Number)
            {
                return new(a.Number < b.Number);
            }

            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to compare (<) a {a.Type} with a {b.Type}."
            );
        }

        public static DynamicValue IsGreaterThanOrEqualTo(this DynamicValue a, DynamicValue b)
        {
            if (a.Type == DynamicValueType.Number && b.Type == DynamicValueType.Number)
            {
                return new(a.Number >= b.Number);
            }

            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to compare (>=) a {a.Type} with a {b.Type}."
            );
        }

        public static DynamicValue IsLessThanOrEqualTo(this DynamicValue a, DynamicValue b)
        {
            if (a.Type == DynamicValueType.Number && b.Type == DynamicValueType.Number)
            {
                return new(a.Number <= b.Number);
            }

            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to compare (<=) a {a.Type} with a {b.Type}."
            );
        }

    }
}