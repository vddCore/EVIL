using System;
using EVIL.CommonTypes.TypeSystem;
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

            if (a.Type == DynamicValueType.String && b.Type == DynamicValueType.String)
            {
                return new(a.String + b.String);
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
        
        public static DynamicValue Modulo(this DynamicValue a, DynamicValue b)
        {
            if (a.Type == DynamicValueType.Number || b.Type == DynamicValueType.Number)
            {
                if (b.Number == 0)
                {
                    throw new DivisionByZeroException();
                }

                return new(a.Number - b.Number * Math.Floor(a.Number / b.Number));
            }

            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to perform a modulo operation using a {a.Type} and a {b.Type}."
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

        public static DynamicValue Increment(this DynamicValue a)
        {
            if (a.Type == DynamicValueType.Number)
            {
                return new(a.Number + 1);
            }
            
            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to increment a {a.Type}."
            );
        }

        public static DynamicValue Decrement(this DynamicValue a)
        {
            if (a.Type == DynamicValueType.Number)
            {
                return new(a.Number - 1);
            }
            
            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to decrement a {a.Type}."
            );
        }
    }
}