namespace EVIL.Lexical
{
    public struct Token
    {
        public TokenType Type { get; }
        public string Value { get; }

        private Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public Token Copy()
            => new(Type, Value);

        public static bool operator ==(Token a, Token b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Token a, Token b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            return obj is Token t && t.Type == Type;
        }

        public override int GetHashCode()
        {
            return (int)Type;
        }

        public static readonly Token Empty = new(TokenType.Empty, null);

        public static readonly Token Assign = new(TokenType.Assign, "=");
        public static readonly Token AssignAdd = new(TokenType.AssignAdd, "+=");
        public static readonly Token AssignSubtract = new(TokenType.AssignSubtract, "-=");
        public static readonly Token AssignMultiply = new(TokenType.AssignMultiply, "*=");
        public static readonly Token AssignDivide = new(TokenType.AssignDivide, "/=");
        public static readonly Token AssignModulo = new(TokenType.AssignModulo, "%=");
        public static readonly Token AssignBitwiseAnd = new(TokenType.AssignBitwiseAnd, "&=");
        public static readonly Token AssignBitwiseOr = new(TokenType.AssignBitwiseOr, "|=");
        public static readonly Token AssignBitwiseXor = new(TokenType.AssignBitwiseXor, "^=");
        public static readonly Token AssignShiftRight = new(TokenType.AssignShiftRight, ">>=");
        public static readonly Token AssignShiftLeft = new(TokenType.AssignShiftLeft, "<<=");

        public static readonly Token Plus = new(TokenType.Plus, "+");
        public static readonly Token Minus = new(TokenType.Minus, "-");
        public static readonly Token Divide = new(TokenType.Divide, "/");
        public static readonly Token Multiply = new(TokenType.Multiply, "*");
        public static readonly Token Modulo = new(TokenType.Modulo, "%");

        public static readonly Token BitwiseAnd = new(TokenType.BitwiseAnd, "&");
        public static readonly Token BitwiseNot = new(TokenType.BitwiseNot, "~");
        public static readonly Token BitwiseOr = new(TokenType.BitwiseOr, "|");
        public static readonly Token BitwiseXor = new(TokenType.BitwiseXor, "^");

        public static readonly Token ShiftLeft = new(TokenType.ShiftLeft, "<<");
        public static readonly Token ShiftRight = new(TokenType.ShiftRight, ">>");

        public static readonly Token Associate = new(TokenType.Associate, "=>");

        public static readonly Token Decrement = new(TokenType.Decrement, "--");
        public static readonly Token Increment = new(TokenType.Increment, "++");

        public static readonly Token LogicalAnd = new(TokenType.LogicalAnd, "&&");
        public static readonly Token LogicalOr = new(TokenType.LogicalOr, "||");
        public static readonly Token LogicalNot = new(TokenType.LogicalNot, "!");

        public static readonly Token Equal = new(TokenType.Equal, "==");
        public static readonly Token NotEqual = new(TokenType.NotEqual, "!=");
        public static readonly Token GreaterThan = new(TokenType.GreaterThan, ">");
        public static readonly Token LessThan = new(TokenType.LessThan, "<");
        public static readonly Token GreaterThanOrEqual = new(TokenType.GreaterThanOrEqual, ">=");
        public static readonly Token LessThanOrEqual = new(TokenType.LessThanOrEqual, "<=");

        public static readonly Token Length = new(TokenType.Length, "#");
        public static readonly Token AsNumber = new(TokenType.AsNumber, "$");
        public static readonly Token AsString = new(TokenType.AsString, "@");
        public static readonly Token QuestionMark = new(TokenType.QuestionMark, "?");

        public static readonly Token Colon = new(TokenType.Colon, ":");
        public static readonly Token Semicolon = new(TokenType.Semicolon, ";");
        public static readonly Token Comma = new(TokenType.Comma, ",");
        public static readonly Token Dot = new(TokenType.Dot, ".");
        public static readonly Token LBrace = new(TokenType.LBrace, "{");
        public static readonly Token RBrace = new(TokenType.RBrace, "}");
        public static readonly Token LBracket = new(TokenType.LBracket, "[");
        public static readonly Token RBracket = new(TokenType.RBracket, "]");
        public static readonly Token LParenthesis = new(TokenType.LParenthesis, "(");
        public static readonly Token RParenthesis = new(TokenType.RParenthesis, ")");
        public static readonly Token ExtraArguments = new(TokenType.ExtraArguments, "...");

        public static readonly Token Var = new(TokenType.Var, "var");
        public static readonly Token Undef = new(TokenType.Undef, "undef");

        public static readonly Token If = new(TokenType.If, "if");
        public static readonly Token Elif = new(TokenType.Elif, "elif");
        public static readonly Token Else = new(TokenType.Else, "else");

        public static readonly Token Fn = new(TokenType.Fn, "fn");
        public static readonly Token Ret = new(TokenType.Ret, "ret");
        public static readonly Token Exit = new(TokenType.Exit, "exit");

        public static readonly Token For = new(TokenType.For, "for");
        public static readonly Token Each = new(TokenType.Each, "each");
        public static readonly Token Do = new(TokenType.Do, "do");
        public static readonly Token While = new(TokenType.While, "while");
        public static readonly Token Break = new(TokenType.Break, "break");
        public static readonly Token Skip = new(TokenType.Skip, "skip");

        public static readonly Token In = new(TokenType.In, "in");
        public static readonly Token NameOf = new(TokenType.NameOf, "??");

        public static readonly Token False = new(TokenType.False, "false");
        public static readonly Token True = new(TokenType.True, "true");

        public static readonly Token EOF = new(TokenType.EOF, "<EOF>");

        public static readonly Token Number = new(TokenType.Number, string.Empty);
        public static readonly Token HexInteger = new(TokenType.HexInteger, string.Empty);
        public static readonly Token String = new(TokenType.String, string.Empty);
        public static readonly Token Identifier = new(TokenType.Identifier, string.Empty);

        public static Token CreateHexInteger(string value)
            => new(TokenType.HexInteger, value);

        public static Token CreateString(string value)
            => new(TokenType.String, value);

        public static Token CreateNumber(string value)
            => new(TokenType.Number, value);

        public static Token CreateIdentifier(string value)
            => new(TokenType.Identifier, value);

        public override string ToString()
            => $"[{Type}: {Value}]";
    }
}