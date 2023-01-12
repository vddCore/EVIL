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
        Floor,
        NameOf,
        ExistsIn,
        Negation,
        ShiftRight,
        ShiftLeft,
        
        MemberAccess,
        KeyInitializer,

        Meta,
        
        Assign,
        AssignAdd,
        AssignSubtract,
        AssignMultiply,
        AssignDivide,
        AssignModulo,
        AssignBitwiseAnd,
        AssignBitwiseOr,
        AssignBitwiseXor,

        CompareNotEqual,
        CompareEqual,
        CompareLessThan,
        CompareLessOrEqualTo,
        CompareGreaterThan,
        CompareGreaterOrEqualTo,

        And,
        Or,

        Comma,
        Colon,
        Semicolon,
        LParenthesis,
        RParenthesis,
        LBrace,
        RBrace,
        LBracket,
        RBracket,

        Var,
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