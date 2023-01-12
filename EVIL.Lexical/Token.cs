using System;

namespace EVIL.Lexical
{
    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }

        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public Token Copy()
            => new(Type, Value);

        public static string StringRepresentation(TokenType type)
        {
            return type switch
            {
                TokenType.Assign => "=",
                TokenType.AssignAdd => "+=",
                TokenType.AssignBitwiseAnd => "&=",
                TokenType.AssignBitwiseOr => "|=",
                TokenType.AssignBitwiseXor => "^=",
                TokenType.AssignDivide => "/=",
                TokenType.AssignModulo => "%=",
                TokenType.AssignMultiply => "*=",
                TokenType.AssignSubtract => "-=",
                TokenType.BitwiseAnd => "&",
                TokenType.BitwiseNot => "~",
                TokenType.BitwiseOr => "|",
                TokenType.BitwiseXor => "^",
                TokenType.Break => "break",
                TokenType.Colon => ":",
                TokenType.Comma => ",",
                TokenType.Decrement => "--",
                TokenType.Divide => "/",
                TokenType.Do => "do",
                TokenType.Dot => ".",
                TokenType.Each => "each",
                TokenType.Elif => "elif",
                TokenType.Else => "else",
                TokenType.Exit => "exit",
                TokenType.Equal => "==",
                TokenType.False => "false",
                TokenType.Floor => "$",
                TokenType.Fn => "fn",
                TokenType.For => "for",
                TokenType.GreaterThanOrEqual => ">=",
                TokenType.GreaterThan => ">",
                TokenType.If => "if",
                TokenType.In => "in",
                TokenType.Increment => "++",
                TokenType.LBrace => "{",
                TokenType.LBracket => "[",
                TokenType.Length => "#",
                TokenType.LessThanOrEqual => "<=",
                TokenType.LessThan => "<",
                TokenType.LogicalAnd => "&&",
                TokenType.LogicalNot => "!",
                TokenType.LogicalOr => "||",
                TokenType.LParenthesis => "(",
                TokenType.Minus => "-",
                TokenType.Modulo => "%",
                TokenType.Multiply => "*",
                TokenType.NameOf => "nameof",
                TokenType.NotEqual => "!=",
                TokenType.Plus => "+",
                TokenType.QuestionMark => "?",
                TokenType.RBrace => "}",
                TokenType.RBracket => "]",
                TokenType.Ret => "ret",
                TokenType.RParenthesis => ")",
                TokenType.Semicolon => ";",
                TokenType.Skip => "skip",
                TokenType.ShiftLeft => "<<",
                TokenType.ShiftRight => ">>",
                TokenType.ToString => "@",
                TokenType.True => "true",
                TokenType.Undef => "undef",
                TokenType.Var => "var",
                TokenType.While => "while",
                _ => Convert.ToString(type)
            };
        }

        public override string ToString()
        {
            return $"{Type}: {Value}";
        }
    }
}