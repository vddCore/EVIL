using System;

namespace EVIL.Lexical
{
    public class Token
    {
        public TokenType Type { get; }
        public object Value { get; }

        public Token(TokenType type, object value)
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
                TokenType.And => "&&",
                TokenType.Assign => "=",
                TokenType.BitwiseAnd => "&",
                TokenType.BitwiseOr => "|",
                TokenType.BitwiseXor => "^",
                TokenType.BitwiseNot => "~",
                TokenType.Break => "break",
                TokenType.Colon => ":",
                TokenType.Semicolon => ";",
                TokenType.Comma => ",",
                TokenType.CompareEqual => "==",
                TokenType.CompareGreaterOrEqualTo => ">=",
                TokenType.CompareGreaterThan => ">",
                TokenType.CompareLessOrEqualTo => "<=",
                TokenType.CompareLessThan => "<",
                TokenType.CompareNotEqual => "!=",
                TokenType.AssignAdd => "+=",
                TokenType.AssignSubtract => "-=",
                TokenType.AssignMultiply => "*=",
                TokenType.AssignDivide => "/=",
                TokenType.AssignModulo => "%=",
                TokenType.AssignBitwiseAnd => "&=",
                TokenType.AssignBitwiseOr => "|=",
                TokenType.AssignBitwiseXor => "^=",
                TokenType.Decrement => "--",
                TokenType.Divide => "/",
                TokenType.Each => "each",
                TokenType.Elif => "elif",
                TokenType.Else => "else",
                TokenType.ExistsIn => "??",
                TokenType.Exit => "exit",
                TokenType.False => "false",
                TokenType.Floor => "$",
                TokenType.Fn => "fn",
                TokenType.For => "for",
                TokenType.If => "if",
                TokenType.Increment => "++",
                TokenType.KeyInitializer => "<-",
                TokenType.LBrace => "{",
                TokenType.LBracket => "[",
                TokenType.Length => "#",
                TokenType.LParenthesis => "(",
                TokenType.MemberAccess => "->",
                TokenType.Minus => "-",
                TokenType.Modulo => "%",
                TokenType.Multiply => "*",
                TokenType.NameOf => "?",
                TokenType.Negation => "!",
                TokenType.Or => "||",
                TokenType.Plus => "+",
                TokenType.RBrace => "}",
                TokenType.RBracket => "]",
                TokenType.Ret => "ret",
                TokenType.RParenthesis => ")",
                TokenType.ShiftLeft => "<<",
                TokenType.ShiftRight => ">>",
                TokenType.Skip => "skip",
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