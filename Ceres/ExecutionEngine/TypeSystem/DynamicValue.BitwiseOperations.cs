using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace Ceres.ExecutionEngine.TypeSystem
{
    public static partial class DynamicValueOperations
    {
        public static DynamicValue BitwiseNegate(this DynamicValue a)
        {
            if (a.Type == DynamicValueType.Number)
            {
                return new(~(long)a.Number);
            }
            
            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to bitwise-not a {a.Type}."
            );
        }
        
        public static DynamicValue BitwiseAnd(this DynamicValue a, DynamicValue b)
        {
            if (a.Type == DynamicValueType.Number && b.Type == DynamicValueType.Number)
            {
                return new((long)a.Number & (long)b.Number);
            }
            
            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to bitwise-and a {a.Type} and a {b.Type}."
            );
        }

        public static DynamicValue BitwiseOr(this DynamicValue a, DynamicValue b)
        {
            if (a.Type == DynamicValueType.Number && b.Type == DynamicValueType.Number)
            {
                return new((long)a.Number | (long)b.Number);
            }
            
            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to bitwise-or a {a.Type} and a {b.Type}."
            );
        }
        
        public static DynamicValue BitwiseXor(this DynamicValue a, DynamicValue b)
        {
            if (a.Type == DynamicValueType.Number && b.Type == DynamicValueType.Number)
            {
                return new((long)a.Number ^ (long)b.Number);
            }
            
            throw new UnsupportedDynamicValueOperationException(
                $"Attempt to bitwise-or a {a.Type} and a {b.Type}."
            );
        }
    }
}