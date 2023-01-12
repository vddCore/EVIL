using System;
using System.Globalization;
using System.Text;

namespace EVIL.Lexical
{
    public class Lexer
    {
        private const string HexAlphabet = "abcdefABCDEF0123456789";

        private string _sourceCode;

        public LexerState State { get; set; } = new();

        public LexerState CopyState()
        {
            return new()
            {
                Character = State.Character,
                Column = State.Column,
                CurrentToken = State.CurrentToken,
                Line = State.Line,
                Pointer = State.Pointer,
                PreviousToken = State.PreviousToken
            };
        }

        public void LoadSource(string source)
        {
            _sourceCode = source;
            State.Reset();

            if (_sourceCode.Length != 0)
                State.Character = _sourceCode[0];

            NextToken();
        }

        public void NextToken()
        {
            SkipWhitespace();

            State.PreviousToken = State.CurrentToken;

            switch (State.Character)
            {
                case '-' when Peek() == '-':
                    Advance();
                    State.CurrentToken = new Token(TokenType.Decrement, "--");
                    break;
                case '-' when Peek() == '=':
                    Advance();
                    State.CurrentToken = new Token(TokenType.AssignSubtract, "-=");
                    break;
                case '-':
                    State.CurrentToken = new Token(TokenType.Minus, "-");
                    break;
                case '.' when char.IsDigit(Peek()):
                    State.CurrentToken = GetDecimalNumber();
                    break;
                case '.':
                    State.CurrentToken = new Token(TokenType.Dot, ".");
                    break;
                case '+' when Peek() == '+':
                    Advance();
                    State.CurrentToken = new Token(TokenType.Increment, "++");
                    break;
                case '+' when Peek() == '=':
                    Advance();
                    State.CurrentToken = new Token(TokenType.AssignAdd, "+=");
                    break;
                case '+':
                    State.CurrentToken = new Token(TokenType.Plus, "-");
                    break;
                case '*' when Peek() == '=':
                    Advance();
                    State.CurrentToken = new Token(TokenType.AssignMultiply, "*=");
                    break;
                case '*':
                    State.CurrentToken = new Token(TokenType.Multiply, "*");
                    break;
                case '/' when Peek() == '/':
                    SkipComment();

                    Advance();
                    NextToken();

                    return;
                case '/' when Peek() == '=':
                    Advance();
                    State.CurrentToken = new Token(TokenType.AssignDivide, "/=");
                    break;
                case '/':
                    State.CurrentToken = new Token(TokenType.Divide, "/");
                    break;
                case '?':
                    State.CurrentToken = new Token(TokenType.NameOf, "?");
                    break;
                case '%' when Peek() == '=':
                    Advance();
                    State.CurrentToken = new Token(TokenType.AssignModulo, "%=");
                    break;
                case '%':
                    State.CurrentToken = new Token(TokenType.Modulo, "%");
                    break;
                case '=' when Peek() == '=':
                    Advance();
                    State.CurrentToken = new Token(TokenType.Equal, "==");
                    break;
                case '=':
                    State.CurrentToken = new Token(TokenType.Assign, "=");
                    break;
                case '<' when Peek() == '=':
                    Advance();
                    State.CurrentToken = new Token(TokenType.LessThanOrEqual, "<=");
                    break;
                case '<' when Peek() == '<':
                    Advance();
                    State.CurrentToken = new Token(TokenType.ShiftLeft, "<<");
                    break;
                case '<':
                    State.CurrentToken = new Token(TokenType.LessThan, "<");
                    break;
                case '>' when Peek() == '=':
                    Advance();
                    State.CurrentToken = new Token(TokenType.GreaterThanOrEqual, ">=");
                    break;
                case '>' when Peek() == '>':
                    Advance();
                    State.CurrentToken = new Token(TokenType.ShiftRight, ">>");
                    break;
                case '>':
                    State.CurrentToken = new Token(TokenType.GreaterThan, ">");
                    break;
                case '&' when Peek() == '&':
                    Advance();
                    State.CurrentToken = new Token(TokenType.LogicalAnd, "&&");
                    break;
                case '&' when Peek() == '=':
                    Advance();
                    State.CurrentToken = new Token(TokenType.AssignBitwiseAnd, "&=");
                    break;
                case '&':
                    State.CurrentToken = new Token(TokenType.BitwiseAnd, "&");
                    break;
                case '|' when Peek() == '|':
                    Advance();
                    State.CurrentToken = new Token(TokenType.LogicalOr, "||");
                    break;
                case '|' when Peek() == '=':
                    Advance();
                    State.CurrentToken = new Token(TokenType.AssignBitwiseOr, "|=");
                    break;
                case '|':
                    State.CurrentToken = new Token(TokenType.BitwiseOr, "|");
                    break;
                case '^' when Peek() == '=':
                    Advance();
                    State.CurrentToken = new Token(TokenType.AssignBitwiseXor, "^=");
                    break;
                case '^':
                    State.CurrentToken = new Token(TokenType.BitwiseXor, "^");
                    break;
                case '~':
                    State.CurrentToken = new Token(TokenType.BitwiseNot, "~");
                    break;
                case ',':
                    State.CurrentToken = new Token(TokenType.Comma, ",");
                    break;
                case ':':
                    State.CurrentToken = new Token(TokenType.Colon, ":");
                    break;
                case ';':
                    State.CurrentToken = new Token(TokenType.Semicolon, ";");
                    break;
                case '(':
                    State.CurrentToken = new Token(TokenType.LParenthesis, "(");
                    break;
                case ')':
                    State.CurrentToken = new Token(TokenType.RParenthesis, ")");
                    break;
                case '!' when Peek() == '=':
                    Advance();
                    State.CurrentToken = new Token(TokenType.NotEqual, "!=");
                    break;
                case '!':
                    State.CurrentToken = new Token(TokenType.LogicalNot, "!");
                    break;
                case '[':
                    State.CurrentToken = new Token(TokenType.LBracket, "[");
                    break;
                case ']':
                    State.CurrentToken = new Token(TokenType.RBracket, "]");
                    break;
                case '{':
                    State.CurrentToken = new Token(TokenType.LBrace, "{");
                    break;
                case '}':
                    State.CurrentToken = new Token(TokenType.RBrace, "}");
                    break;
                default:
                {
                    switch (State.Character)
                    {
                        case '"':
                            State.CurrentToken = GetString();
                            Advance();

                            return;
                        case (char)0:
                            State.CurrentToken = new Token(TokenType.EOF, "<EOF>");
                            Advance();

                            return;
                        case '#':
                            State.CurrentToken = new Token(TokenType.Length, "#");
                            Advance();

                            return;
                        case '@':
                            State.CurrentToken = new Token(TokenType.ToString, "@");
                            Advance();

                            return;
                        case '$':
                            State.CurrentToken = new Token(TokenType.Floor, "$");
                            Advance();

                            return;
                        default:
                        {
                            if (char.IsLetter(State.Character) || State.Character == '_')
                            {
                                State.CurrentToken = GetIdentifierOrKeyword();
                                return;
                            }
                            else if (char.IsDigit(State.Character))
                            {
                                if (State.Character == '0')
                                {
                                    if (Peek() == 'x')
                                    {
                                        Advance();
                                        Advance();

                                        State.CurrentToken = GetHexNumber();
                                        return;
                                    }
                                }
                                State.CurrentToken = GetDecimalNumber();
                                return;
                            }
                        }
                            
                        throw new LexerException($"Unexpected token '{State.Character}'", State.Column, State.Line);
                    }
                }
            }

            Advance();
        }

        public Token PeekToken(int howFar)
        {
            var prevState = CopyState();
            Token token = null;

            for (var i = 0; i < howFar; i++)
            {
                NextToken();
                token = State.CurrentToken;
            }

            State = prevState;
            return token;
        }

        private Token GetHexNumber()
        {
            var number = string.Empty;

            while (IsLegalHexNumberCharacter(State.Character))
            {
                number += State.Character;
                Advance();
            }

            return new Token(TokenType.HexInteger, number);
        }

        private Token GetDecimalNumber()
        {
            var number = string.Empty;
            var isDecimal = false;

            while (char.IsDigit(State.Character) || State.Character == '.')
            {
                if (State.Character == '.')
                {
                    isDecimal = true;
                }

                number += State.Character;
                Advance();
            }

            try
            {
                if (isDecimal)
                {
                    return new Token(TokenType.Decimal, number);
                }
                else
                {
                    return new Token(TokenType.Integer, number);
                }
            }
            catch (FormatException)
            {
                throw new LexerException("Invalid number format.", State.Column, State.Line);
            }
        }

        private Token GetIdentifierOrKeyword()
        {
            var str = string.Empty;

            while (IsLegalIdentifierCharacter(State.Character))
            {
                str += State.Character;
                Advance();
            }

            return str switch
            {
                "fn" => new Token(TokenType.Fn, "fn"),
                "for" => new Token(TokenType.For, "for"),
                "each" => new Token(TokenType.Each, "each"),
                "undef" => new Token(TokenType.Undef, "undef"),
                "while" => new Token(TokenType.While, "while"),
                "do" => new Token(TokenType.Do, "do"),
                "var" => new Token(TokenType.Var, "var"),
                "in" => new Token(TokenType.In, "in"),
                "if" => new Token(TokenType.If, "if"),
                "elif" => new Token(TokenType.Elif, "elif"),
                "else" => new Token(TokenType.Else, "else"),
                "break" => new Token(TokenType.Break, "break"),
                "skip" => new Token(TokenType.Skip, "skip"),
                "ret" => new Token(TokenType.Ret, "ret"),
                "true" => new Token(TokenType.True, "true"),
                "false" => new Token(TokenType.False, "false"),
                "exit" => new Token(TokenType.Exit, "exit"),
                _ => new Token(TokenType.Identifier, str)
            };
        }

        private bool IsLegalIdentifierCharacter(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }

        private bool IsLegalHexNumberCharacter(char c)
        {
            return HexAlphabet.Contains(c);
        }

        private Token GetString()
        {
            var str = string.Empty;
            Advance();

            while (State.Character != '"')
            {
                if (State.Character == (char)0)
                    throw new LexerException("Unterminated string.", State.Column, State.Line);

                if (State.Character == '\\')
                {
                    Advance();

                    switch (State.Character)
                    {
                        case '"':
                            str += '"';
                            break;

                        case '\\':
                            str += '\\';
                            break;

                        case 'n':
                            str += '\n';
                            break;

                        case 'u':
                            Advance();
                            str += GetUnicodeSequence();
                            continue;

                        case 'r':
                            str += '\r';
                            break;

                        default:
                            throw new LexerException("Unrecognized escape sequence.", State.Column, State.Line);
                    }

                    Advance();
                    continue;
                }

                str += State.Character;
                Advance();
            }

            return new Token(TokenType.String, str);
        }

        private char GetUnicodeSequence()
        {
            var sb = new StringBuilder();

            for (var i = 0; i < 4; i++)
            {
                if (HexAlphabet.Contains(State.Character))
                {
                    sb.Append(State.Character);
                    Advance();
                }
                else
                {
                    throw new LexerException(
                        "Invalid universal character code.",
                        State.Column,
                        State.Line
                    );
                }
            }

            return (char)int.Parse(sb.ToString(), NumberStyles.HexNumber);
        }

        private void SkipComment()
        {
            while (State.Character != '\n' && State.Character != (char)0)
                Advance();
        }

        private void SkipWhitespace()
        {
            while (char.IsWhiteSpace(State.Character) || State.Character == 0xFEFF)
                Advance();
        }

        private char Peek()
        {
            if (State.Pointer + 1 >= _sourceCode.Length)
                return (char)0;

            return _sourceCode[State.Pointer + 1];
        }

        private void Advance()
        {
            State.Pointer++;
            State.Column++;

            if (State.Pointer >= _sourceCode.Length)
            {
                State.Character = (char)0;
                return;
            }

            State.Character = _sourceCode[State.Pointer];

            if (State.Character == '\n')
            {
                State.Column = 1;
                State.Line++;
            }
        }
    }
}