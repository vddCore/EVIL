namespace EVIL.CommonTypes.TypeSystem
{
    public enum TableOverride : byte
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo,
        ShiftLeft,
        ShiftRight,
        ArithmeticNegate,
        Increment,
        Decrement,
        LogicalNot,
        LogicalOr,
        LogicalAnd,
        BitwiseOr,
        BitwiseXor,
        BitwiseAnd,
        BitwiseNot,
        DeepEqual,
        DeepNotEqual,
        Equal,
        NotEqual,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        Length,
        ToNumber,
        ToString,
        Invoke,
        Get,
        Set,
        Exists
    }
}