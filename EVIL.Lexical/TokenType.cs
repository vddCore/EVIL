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
        AsString,
        Length,
        Increment,
        Decrement,
        Floor,
        NameOf,
        ShiftRight,
        ShiftLeft,
        
        Dot,
        QuestionMark,

        Assign,
        AssignAdd,
        AssignSubtract,
        AssignMultiply,
        AssignDivide,
        AssignModulo,
        AssignBitwiseAnd,
        AssignBitwiseOr,
        AssignBitwiseXor,
        AssignShiftRight,
        AssignShiftLeft,

        Equal,
        NotEqual,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,

        LogicalNot,
        LogicalAnd,
        LogicalOr,

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

        Number,
        HexInteger,
        String,

        True,
        False,

        Fn,
        Undef,
        For,
        Do,
        While,
        Each,
        If,
        Elif,
        Else,
        Skip,
        Break,
        Ret,
        Exit,
        Var,
        In,

        EOF
    }
}