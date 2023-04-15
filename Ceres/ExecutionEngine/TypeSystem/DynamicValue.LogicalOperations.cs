namespace Ceres.ExecutionEngine.TypeSystem
{
    public static partial class DynamicValueOperations
    {
        public static DynamicValue LogicallyNegate(this DynamicValue a)
            => new(!a.IsTruth);

        public static DynamicValue LogicalAnd(this DynamicValue a, DynamicValue b)
            => new(a.IsTruth && b.IsTruth);

        public static DynamicValue LogicalOr(this DynamicValue a, DynamicValue b)
            => new(a.IsTruth || b.IsTruth);
    }
}