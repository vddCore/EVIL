namespace EVIL.Ceres.ExecutionEngine.TypeSystem;

public static partial class DynamicValueOperations
{
    public static DynamicValue LogicallyNegate(this DynamicValue a)
        => new(!DynamicValue.IsTruth(a));

    public static DynamicValue LogicalAnd(this DynamicValue a, DynamicValue b)
        => new(DynamicValue.IsTruth(a) && DynamicValue.IsTruth(b));

    public static DynamicValue LogicalOr(this DynamicValue a, DynamicValue b)
        => new(DynamicValue.IsTruth(a) || DynamicValue.IsTruth(b));
}