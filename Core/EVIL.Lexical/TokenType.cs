﻿namespace EVIL.Lexical
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

        DeepEqual,
        DeepNotEqual,
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

        AttributeList,
        Identifier,

        Number,
        HexInteger,
        String,

        True,
        False,

        Fn,
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
        Var,
        In,
        Nil,
        TypeOf,
        Yield,
        YieldTargetOpen,
        YieldTargetClose,
        
        EOF
    }
}