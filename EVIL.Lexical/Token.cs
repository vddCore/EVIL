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

        public static string StringRepresentation(TokenType type)
        {
            switch (type)
            {
                case TokenType.And: return "&&";
                case TokenType.Assign: return "=";
                case TokenType.Break: return "break";
                case TokenType.Colon: return ":";
                case TokenType.Semicolon: return ";";
                case TokenType.Comma: return ",";
                case TokenType.CompareEqual: return "==";
                case TokenType.CompareGreaterOrEqualTo: return ">=";
                case TokenType.CompareGreaterThan: return ">";
                case TokenType.CompareLessOrEqualTo: return "<=";
                case TokenType.CompareLessThan: return "<";
                case TokenType.CompareNotEqual: return "!=";
                case TokenType.AssignAdd: return "+=";
                case TokenType.AssignSubtract: return "-=";
                case TokenType.AssignMultiply: return "*=";
                case TokenType.AssignDivide: return "/=";
                case TokenType.AssignModulo: return "%=";
                case TokenType.AssignBitwiseAnd: return "&=";
                case TokenType.AssignBitwiseOr: return "|=";
                case TokenType.AssignBitwiseXor: return "^=";
                case TokenType.Decrement: return "--";
                case TokenType.Divide: return "/";
                case TokenType.Each: return "each";
                case TokenType.Elif: return "elif";
                case TokenType.Else: return "else";
                case TokenType.Exit: return "exit";
                case TokenType.False: return "false";
                case TokenType.Fn: return "fn";
                case TokenType.For: return "for";
                case TokenType.If: return "if";
                case TokenType.Increment: return "++";
                case TokenType.LBrace: return "{";
                case TokenType.LBracket: return "[";
                case TokenType.Length: return "#";
                case TokenType.Var: return "var";
                case TokenType.LParenthesis: return "(";
                case TokenType.Minus: return "-";
                case TokenType.Modulo: return "%";
                case TokenType.Multiply: return "*";
                case TokenType.NameOf: return "?";
                case TokenType.BitwiseAnd: return "&";
                case TokenType.BitwiseOr: return "|";
                case TokenType.BitwiseXor: return "^";
                case TokenType.BitwiseNot: return "~";
                case TokenType.Floor: return "$";
                case TokenType.Negation: return "!";
                case TokenType.Or: return "||";
                case TokenType.Plus: return "+";
                case TokenType.RBrace: return "}";
                case TokenType.RBracket: return "]";
                case TokenType.Ret: return "ret";
                case TokenType.RParenthesis: return ")";
                case TokenType.ShiftLeft: return "<<";
                case TokenType.ShiftRight: return ">>";
                case TokenType.Skip: return "skip";
                case TokenType.ToString: return "@";
                case TokenType.True: return "true";
                case TokenType.Undef: return "undef";
                case TokenType.While: return "while";
                case TokenType.ExistsIn: return "??";
                case TokenType.MemberAccess: return "->";
                default: return Convert.ToString(type);
            }
        }

        public override string ToString()
        {
            return $"{Type}: {Value}";
        }
    }
}