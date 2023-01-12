namespace EVIL.Lexical
{
    public enum TokenType
    {
        Minus,
        Plus,
        Multiply,
        Divide,
        Modulo,
        BitwiseAnd,
        BitwiseOr,
        BitwiseXor,
        BitwiseNot,
        ToString,
        Length,
        Increment,
        Decrement,
        NameOf,
        Negation,
        ShiftRight,
        ShiftLeft,

        CompoundAdd,
        CompoundSubtract,
        CompoundMultiply,
        CompoundDivide,
        CompoundModulo,
        CompoundBitwiseAnd,
        CompoundBitwiseOr,
        CompoundBitwiseXor,

        CompareNotEqual,
        CompareEqual,
        CompareLessThan,
        CompareLessOrEqualTo,
        CompareGreaterThan,
        CompareGreaterOrEqualTo,

        LocalVar,

        And,
        Or,

        Assign,
        Comma,
        Colon,
        Semicolon,
        LParenthesis,
        RParenthesis,
        LBrace,
        RBrace,
        LBracket,
        RBracket,

        Identifier,

        DecimalNumber,
        HexNumber,
        String,

        True,
        False,

        Fn,
        Undef,
        For,
        While,
        Each,
        If,
        Elif,
        Else,
        Skip,
        Break,
        Ret,
        Exit,

        EOF
    }
}