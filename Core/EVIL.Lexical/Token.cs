using System;

namespace EVIL.Lexical
{
    public struct Token : IEquatable<Token>
    {
        public TokenType Type { get; }
        public TokenClass Class { get; }
        public string Value { get; }

        public int Line { get; init; }
        public int Column { get; init; }

        public Token(TokenType type, TokenClass @class, string value)
        {
            Type = type;
            Class = @class;
            Value = value;
        }

        public static Token CreateHexInteger(string value)
            => new(TokenType.HexInteger, TokenClass.Literal, value);

        public static Token CreateString(string value)
            => new(TokenType.String, TokenClass.Literal, value);

        public static Token CreateNumber(string value)
            => new(TokenType.Number, TokenClass.Literal, value);

        public static Token CreateIdentifier(string value)
            => new(TokenType.Identifier, TokenClass.Identifier, value);

        public override string ToString()
            => $"[{Type}: {Value}]";
        
        public bool Equals(Token other) 
            => Type == other.Type && Class == other.Class && Value == other.Value;

        public override bool Equals(object? obj) 
            => obj is Token other && Equals(other);

        public override int GetHashCode() 
            => HashCode.Combine((int)Type, (int)Class, Value);

        public static bool operator ==(Token left, Token right) 
            => left.Equals(right);

        public static bool operator !=(Token left, Token right) 
            => !left.Equals(right);

        public static readonly Token Assign = new(TokenType.Assign, TokenClass.Operator, "=");
        public static readonly Token AssignAdd = new(TokenType.AssignAdd, TokenClass.Operator, "+=");
        public static readonly Token AssignSubtract = new(TokenType.AssignSubtract, TokenClass.Operator, "-=");
        public static readonly Token AssignMultiply = new(TokenType.AssignMultiply, TokenClass.Operator, "*=");
        public static readonly Token AssignDivide = new(TokenType.AssignDivide, TokenClass.Operator, "/=");
        public static readonly Token AssignModulo = new(TokenType.AssignModulo, TokenClass.Operator, "%=");
        public static readonly Token AssignBitwiseAnd = new(TokenType.AssignBitwiseAnd, TokenClass.Operator, "&=");
        public static readonly Token AssignBitwiseOr = new(TokenType.AssignBitwiseOr, TokenClass.Operator, "|=");
        public static readonly Token AssignBitwiseXor = new(TokenType.AssignBitwiseXor, TokenClass.Operator, "^=");
        public static readonly Token AssignShiftRight = new(TokenType.AssignShiftRight, TokenClass.Operator, ">>=");
        public static readonly Token AssignShiftLeft = new(TokenType.AssignShiftLeft, TokenClass.Operator, "<<=");
        public static readonly Token Plus = new(TokenType.Plus, TokenClass.Operator, "+");
        public static readonly Token Minus = new(TokenType.Minus, TokenClass.Operator, "-");
        public static readonly Token Divide = new(TokenType.Divide, TokenClass.Operator, "/");
        public static readonly Token Multiply = new(TokenType.Multiply, TokenClass.Operator, "*");
        public static readonly Token Modulo = new(TokenType.Modulo, TokenClass.Operator, "%");
        public static readonly Token BitwiseAnd = new(TokenType.BitwiseAnd, TokenClass.Operator, "&");
        public static readonly Token BitwiseNot = new(TokenType.BitwiseNot, TokenClass.Operator, "~");
        public static readonly Token BitwiseOr = new(TokenType.BitwiseOr, TokenClass.Operator, "|");
        public static readonly Token BitwiseXor = new(TokenType.BitwiseXor, TokenClass.Operator, "^");
        public static readonly Token ShiftLeft = new(TokenType.ShiftLeft, TokenClass.Operator, "<<");
        public static readonly Token ShiftRight = new(TokenType.ShiftRight, TokenClass.Operator, ">>");
        public static readonly Token Ellipsis = new(TokenType.Ellipsis, TokenClass.Operator, "...");
        public static readonly Token Associate = new(TokenType.Associate, TokenClass.Operator, "=>");
        public static readonly Token RightArrow = new(TokenType.RightArrow, TokenClass.Operator, "->");
        public static readonly Token Decrement = new(TokenType.Decrement, TokenClass.Operator, "--");
        public static readonly Token Increment = new(TokenType.Increment, TokenClass.Operator, "++");
        public static readonly Token LogicalAnd = new(TokenType.LogicalAnd, TokenClass.Operator, "&&");
        public static readonly Token LogicalOr = new(TokenType.LogicalOr, TokenClass.Operator, "||");
        public static readonly Token LogicalNot = new(TokenType.LogicalNot, TokenClass.Operator, "!");
        public static readonly Token DeepEqual = new(TokenType.DeepEqual, TokenClass.Operator, "<==>");
        public static readonly Token DeepNotEqual = new(TokenType.DeepNotEqual, TokenClass.Operator, "<!=>");
        public static readonly Token Equal = new(TokenType.Equal, TokenClass.Operator, "==");
        public static readonly Token NotEqual = new(TokenType.NotEqual, TokenClass.Operator, "!=");
        public static readonly Token GreaterThan = new(TokenType.GreaterThan, TokenClass.Operator, ">");
        public static readonly Token LessThan = new(TokenType.LessThan, TokenClass.Operator, "<");
        public static readonly Token GreaterThanOrEqual = new(TokenType.GreaterThanOrEqual, TokenClass.Operator, ">=");
        public static readonly Token LessThanOrEqual = new(TokenType.LessThanOrEqual, TokenClass.Operator, "<=");
        public static readonly Token Length = new(TokenType.Length, TokenClass.Operator, "#");
        public static readonly Token AsNumber = new(TokenType.AsNumber, TokenClass.Operator, "$");
        public static readonly Token AsString = new(TokenType.AsString, TokenClass.Operator, "@");
        public static readonly Token QuestionMark = new(TokenType.QuestionMark, TokenClass.Operator, "?");
        public static readonly Token Colon = new(TokenType.Colon, TokenClass.Operator, ":");
        public static readonly Token Semicolon = new(TokenType.Semicolon, TokenClass.Operator, ";");
        public static readonly Token Comma = new(TokenType.Comma, TokenClass.Operator, ",");
        public static readonly Token Dot = new(TokenType.Dot, TokenClass.Operator, ".");
        public static readonly Token LBrace = new(TokenType.LBrace, TokenClass.Operator, "{");
        public static readonly Token RBrace = new(TokenType.RBrace, TokenClass.Operator, "}");
        public static readonly Token LBracket = new(TokenType.LBracket, TokenClass.Operator, "[");
        public static readonly Token RBracket = new(TokenType.RBracket, TokenClass.Operator, "]");
        public static readonly Token LParenthesis = new(TokenType.LParenthesis, TokenClass.Operator, "(");
        public static readonly Token RParenthesis = new(TokenType.RParenthesis, TokenClass.Operator, ")");
        public static readonly Token AttributeList = new(TokenType.AttributeList, TokenClass.Operator, "#[");

        public static readonly Token Val = new(TokenType.Val, TokenClass.Keyword, "val");
        public static readonly Token If = new(TokenType.If, TokenClass.Keyword, "if");
        public static readonly Token Elif = new(TokenType.Elif, TokenClass.Keyword, "elif");
        public static readonly Token Else = new(TokenType.Else, TokenClass.Keyword, "else");
        public static readonly Token Fn = new(TokenType.Fn, TokenClass.Keyword, "fn");
        public static readonly Token Ret = new(TokenType.Ret, TokenClass.Keyword, "ret");
        public static readonly Token For = new(TokenType.For, TokenClass.Keyword, "for");
        public static readonly Token Do = new(TokenType.Do, TokenClass.Keyword, "do");
        public static readonly Token While = new(TokenType.While, TokenClass.Keyword, "while");
        public static readonly Token Break = new(TokenType.Break, TokenClass.Keyword, "break");
        public static readonly Token Skip = new(TokenType.Skip, TokenClass.Keyword, "skip");
        public static readonly Token In = new(TokenType.In, TokenClass.Keyword, "in");
        public static readonly Token Is = new(TokenType.Is, TokenClass.Keyword, "is");
        public static readonly Token False = new(TokenType.False, TokenClass.Keyword, "false");
        public static readonly Token True = new(TokenType.True, TokenClass.Keyword, "true");
        public static readonly Token Nil = new(TokenType.Nil, TokenClass.Keyword, "nil");
        public static readonly Token TypeOf = new(TokenType.TypeOf, TokenClass.Keyword, "typeof");
        public static readonly Token Yield = new(TokenType.Yield, TokenClass.Keyword, "yield");
        public static readonly Token Rw = new(TokenType.Rw, TokenClass.Keyword, "rw");
        public static readonly Token Each = new(TokenType.Each, TokenClass.Keyword, "each");
        
        public static readonly Token NaN = new(TokenType.NaN, TokenClass.Alias, "NaN");
        public static readonly Token Infinity = new(TokenType.Infinity, TokenClass.Alias, "Infinity");

        public static readonly Token NilTypeCode = new(TokenType.NilTypeCode, TokenClass.TypeCode, "Nil");
        public static readonly Token NumberTypeCode = new(TokenType.NumberTypeCode, TokenClass.TypeCode, "Number");
        public static readonly Token StringTypeCode = new(TokenType.StringTypeCode, TokenClass.TypeCode, "String");
        public static readonly Token BooleanTypeCode = new(TokenType.BooleanTypeCode, TokenClass.TypeCode, "Boolean");
        public static readonly Token TableTypeCode = new(TokenType.TableTypeCode, TokenClass.TypeCode, "Table");
        public static readonly Token FiberTypeCode = new(TokenType.FiberTypeCode, TokenClass.TypeCode, "Fiber");
        public static readonly Token ChunkTypeCode = new(TokenType.ChunkTypeCode, TokenClass.TypeCode, "Function");
        public static readonly Token TypeCodeTypeCode = new(TokenType.TypeCodeTypeCode, TokenClass.TypeCode, "Type");
        public static readonly Token NativeFunctionTypeCode = new(TokenType.NativeFunctionTypeCode, TokenClass.TypeCode, "NativeFunction");
        public static readonly Token NativeObjectTypeCode = new(TokenType.NativeObjectTypeCode, TokenClass.TypeCode, "NativeObject");

        public static readonly Token Identifier = new(TokenType.Identifier, TokenClass.Identifier, string.Empty);
        public static readonly Token Number = new(TokenType.Number, TokenClass.Literal, string.Empty);
        public static readonly Token HexInteger = new(TokenType.HexInteger, TokenClass.Literal, string.Empty);
        public static readonly Token String = new(TokenType.String, TokenClass.Literal, string.Empty);

        public static readonly Token Include = new(TokenType.Include, TokenClass.Meta, "#include");
        public static readonly Token EOF = new(TokenType.EOF, TokenClass.Meta, "<EOF>");
        public static readonly Token Empty = new(TokenType.Empty, TokenClass.Meta, string.Empty);
    }
}