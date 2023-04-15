using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace Ceres.ExecutionEngine.TypeSystem
{
    public static partial class DynamicValueOperations
    {
        public static DynamicValue Add(this DynamicValue a, DynamicValue b)
        {
            if (a.Type == DynamicValueType.Number && b.Type == DynamicValueType.Number)
            {
                return new(a.Number + b.Number);
            }

            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to add a {a.Type} to a {b.Type}."
            );
        }

        public static DynamicValue Subtract(this DynamicValue a, DynamicValue b)
        {
            if (a.Type == DynamicValueType.Number && b.Type == DynamicValueType.Number)
            {
                return new(a.Number - b.Number);
            }

            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to subtract a {b.Type} from a {a.Type}."
            );
        }

        public static DynamicValue Multiply(this DynamicValue a, DynamicValue b)
        {
            if (a.Type == DynamicValueType.Number && b.Type == DynamicValueType.Number)
            {
                return new(a.Number * b.Number);
            }

            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to multiply a {a.Type} by a {b.Type}."
            );
        }

        public static DynamicValue DivideBy(this DynamicValue a, DynamicValue b)
        {
            if (a.Type == DynamicValueType.Number || b.Type == DynamicValueType.Number)
            {
                if (b.Number == 0)
                {
                    throw new DivisionByZeroException();
                }

                return new(a.Number / b.Number);
            }

            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to divide a {a.Type} by a {b.Type}."
            );
        }

        public static DynamicValue ArithmeticallyNegate(this DynamicValue a)
        {
            if (a.Type == DynamicValueType.Number)
            {
                return new(-a.Number);
            }

            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to arithmetically negate a {a.Type}."
            );
        }
    }
}