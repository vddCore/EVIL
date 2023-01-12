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
                    State.CurrentToken = Token.Decrement;
                    break;
                case '-' when Peek() == '=':
                    Advance();
                    State.CurrentToken = Token.AssignSubtract;
                    break;
                case '-':
                    State.CurrentToken = Token.Minus;
                    break;
                case '.' when char.IsDigit(Peek()):
                    State.CurrentToken = GetDecimalNumber();
                    break;
                case '.' when Peek() == '.' && Peek(2) == '.':
                    Advance();
                    Advance();
                    State.CurrentToken = Token.ExtraArguments;
                    break;
                case '.':
                    State.CurrentToken = Token.Dot;
                    break;
                case '+' when Peek() == '+':
                    Advance();
                    State.CurrentToken = Token.Increment;
                    break;
                case '+' when Peek() == '=':
                    Advance();
                    State.CurrentToken = Token.AssignAdd;
                    break;
                case '+':
                    State.CurrentToken = Token.Plus;
                    break;
                case '*' when Peek() == '=':
                    Advance();
                    State.CurrentToken = Token.AssignMultiply;
                    break;
                case '*':
                    State.CurrentToken = Token.Multiply;
                    break;
                case '/' when Peek() == '/':
                    SkipComment();

                    Advance();
                    NextToken();

                    return;
                case '/' when Peek() == '=':
                    Advance();
                    State.CurrentToken = Token.AssignDivide;
                    break;
                case '/':
                    State.CurrentToken = Token.Divide;
                    break;
                case '?' when Peek() == '?':
                    Advance();
                    State.CurrentToken = Token.NameOf;
                    break;
                case '?':
                    State.CurrentToken = Token.QuestionMark;
                    break;
                case '%' when Peek() == '=':
                    Advance();
                    State.CurrentToken = Token.AssignModulo;
                    break;
                case '%':
                    State.CurrentToken = Token.Modulo;
                    break;
                case '=' when Peek() == '>':
                    Advance();
                    State.CurrentToken = Token.Associate;
                    break;
                case '=' when Peek() == '=':
                    Advance();
                    State.CurrentToken = Token.Equal;
                    break;
                case '=':
                    State.CurrentToken = Token.Assign;
                    break;
                case '<' when Peek() == '=':
                    Advance();
                    State.CurrentToken = Token.LessThanOrEqual;
                    break;
                case '<' when Peek() == '<':
                {
                    Advance();
                    if (Peek() == '=')
                    {
                        Advance();
                        State.CurrentToken = Token.AssignShiftLeft;
                    }
                    else
                    {
                        State.CurrentToken = Token.ShiftLeft;
                    }
                    break;
                }
                case '<':
                    State.CurrentToken = Token.LessThan;
                    break;
                case '>' when Peek() == '=':
                    Advance();
                    State.CurrentToken = Token.GreaterThanOrEqual;
                    break;
                case '>' when Peek() == '>':
                {
                    Advance();
                    if (Peek() == '=')
                    {
                        Advance();
                        State.CurrentToken = Token.AssignShiftRight;
                    }
                    else
                    {
                        State.CurrentToken = Token.ShiftRight;
                    }
                    break;
                }
                case '>':
                    State.CurrentToken = Token.GreaterThan;
                    break;
                case '&' when Peek() == '&':
                    Advance();
                    State.CurrentToken = Token.LogicalAnd;
                    break;
                case '&' when Peek() == '=':
                    Advance();
                    State.CurrentToken = Token.AssignBitwiseAnd;
                    break;
                case '&':
                    State.CurrentToken = Token.BitwiseAnd;
                    break;
                case '|' when Peek() == '|':
                    Advance();
                    State.CurrentToken = Token.LogicalOr;
                    break;
                case '|' when Peek() == '=':
                    Advance();
                    State.CurrentToken = Token.AssignBitwiseOr;
                    break;
                case '|':
                    State.CurrentToken = Token.BitwiseOr;
                    break;
                case '^' when Peek() == '=':
                    Advance();
                    State.CurrentToken = Token.AssignBitwiseXor;
                    break;
                case '^':
                    State.CurrentToken = Token.BitwiseXor;
                    break;
                case '~':
                    State.CurrentToken = Token.BitwiseNot;
                    break;
                case ',':
                    State.CurrentToken = Token.Comma;
                    break;
                case ':':
                    State.CurrentToken = Token.Colon;
                    break;
                case ';':
                    State.CurrentToken = Token.Semicolon;
                    break;
                case '(':
                    State.CurrentToken = Token.LParenthesis;
                    break;
                case ')':
                    State.CurrentToken = Token.RParenthesis;
                    break;
                case '!' when Peek() == '=':
                    Advance();
                    State.CurrentToken = Token.NotEqual;
                    break;
                case '!':
                    State.CurrentToken = Token.LogicalNot;
                    break;
                case '[':
                    State.CurrentToken = Token.LBracket;
                    break;
                case ']':
                    State.CurrentToken = Token.RBracket;
                    break;
                case '{':
                    State.CurrentToken = Token.LBrace;
                    break;
                case '}':
                    State.CurrentToken = Token.RBrace;
                    break;
                case (char)0:
                    State.CurrentToken = Token.EOF;
                    break;
                case '"':
                    State.CurrentToken = GetString();
                    break;
                case '#':
                    State.CurrentToken = Token.Length;
                    break;
                case '@':
                    State.CurrentToken = Token.AsString;
                    break;
                case '$':
                    State.CurrentToken = Token.AsNumber;
                    break;

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

            Advance();
        }

        public Token PeekToken(int howFar)
        {
            var prevState = CopyState();
            var token = Token.Empty;

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

            return Token.CreateHexInteger(number);
        }

        private Token GetDecimalNumber()
        {
            var number = string.Empty;

            while (char.IsDigit(State.Character) || State.Character == '.')
            {
                number += State.Character;
                Advance();
            }

            try
            {
                return Token.CreateNumber(number);
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
                "fn" => Token.Fn,
                "for" => Token.For,
                "each" => Token.Each,
                "while" => Token.While,
                "do" => Token.Do,
                "loc" => Token.Loc,
                "in" => Token.In,
                "if" => Token.If,
                "elif" => Token.Elif,
                "else" => Token.Else,
                "break" => Token.Break,
                "skip" => Token.Skip,
                "ret" => Token.Ret,
                "true" => Token.True,
                "false" => Token.False,
                "exit" => Token.Exit,
                "null" => Token.Null,
                _ => Token.CreateIdentifier(str)
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

            return Token.CreateString(str);
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

        private char Peek(int howFar = 1)
        {
            if (State.Pointer + howFar >= _sourceCode.Length)
                return (char)0;

            return _sourceCode[State.Pointer + howFar];
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