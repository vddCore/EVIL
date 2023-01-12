namespace EVIL.Lexical
{
    public enum TokenType
    {
        Empty = -1,
        
        Minus,
        Plus,
        Multiply,
        Divide,
        Modulo,
        BitwiseAnd,
        BitwiseOr,
        BitwiseXor,
        BitwiseNot,
        AsNumber,
        AsString,
        Length,
        Increment,
        Decrement,
        NameOf,
        ShiftRight,
        ShiftLeft,
        
        Associate,
        
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
        ExtraArguments,

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
        Loc,
        In,
        Null,

        EOF
    }
}