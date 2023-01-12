using System;
using System.Globalization;

namespace EVIL.Lexical
{
    public class Scanner
    {
        private const string HexAlphabet = "abcdefABCDEF0123456789";

        private string _sourceCode;

        public ScannerState State { get; set; }

        public ScannerState CopyState()
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
            State = new ScannerState
            {
                Line = 1,
                Column = 1,
                Pointer = 0
            };

            if (_sourceCode.Length != 0)
                State.Character = _sourceCode[0];

            NextToken();
        }

        public void NextToken()
        {
            SkipWhitespace();

            State.PreviousToken = State.CurrentToken;

            if (State.Character == '-')
            {
                if (Peek() == '-')
                {
                    Advance();
                    State.CurrentToken = new Token(TokenType.Decrement, "--");
                }
                else if (Peek() == '=')
                {
                    Advance();
                    State.CurrentToken = new Token(TokenType.AssignSubtract, "-=");
                }
                else
                {
                    State.CurrentToken = new Token(TokenType.Minus, '-');
                }
            }
            else if (State.Character == '+')
            {
                if (Peek() == '+')
                {
                    Advance();
                    State.CurrentToken = new Token(TokenType.Increment, "++");
                }
                else if (Peek() == '=')
                {
                    Advance();
                    State.CurrentToken = new Token(TokenType.AssignAdd, "+=");
                }
                else
                {
                    State.CurrentToken = new Token(TokenType.Plus, '+');
                }
            }
            else if (State.Character == '*')
            {
                if (Peek() == '=')
                {
                    Advance();
                    State.CurrentToken = new Token(TokenType.AssignMultiply, "*=");
                }
                else
                {
                    State.CurrentToken = new Token(TokenType.Multiply, '*');
                }
            }
            else if (State.Character == '/')
            {
                if (Peek() == '/')
                {
                    SkipComment();

                    Advance();
                    NextToken();

                    return;
                }
                else if (Peek() == '=')
                {
                    Advance();
                    State.CurrentToken = new Token(TokenType.AssignDivide, "/=");
                }
                else
                {
                    State.CurrentToken = new Token(TokenType.Divide, '/');
                }
            }
            else if (State.Character == '?')
            {
                State.CurrentToken = new Token(TokenType.NameOf, '?');
            }
            else if (State.Character == '%')
            {
                if (Peek() == '=')
                {
                    Advance();
                    State.CurrentToken = new Token(TokenType.AssignModulo, "%=");
                }
                else
                {
                    State.CurrentToken = new Token(TokenType.Modulo, '%');
                }
            }
            else if (State.Character == '=')
            {
                if (Peek() == '=')
                {
                    Advance();
                    State.CurrentToken = new Token(TokenType.CompareEqual, "==");
                }
                else
                {
                    State.CurrentToken = new Token(TokenType.Assign, '=');
                }
            }
            else if (State.Character == '<')
            {
                if (Peek() == '=')
                {
                    Advance();
                    State.CurrentToken = new Token(TokenType.CompareLessOrEqualTo, "<=");
                }
                else if (Peek() == '<')
                {
                    Advance();
                    State.CurrentToken = new Token(TokenType.ShiftLeft, "<<");
                }
                else
                {
                    State.CurrentToken = new Token(TokenType.CompareLessThan, '<');
                }
            }
            else if (State.Character == '>')
            {
                if (Peek() == '=')
                {
                    Advance();
                    State.CurrentToken = new Token(TokenType.CompareGreaterOrEqualTo, ">=");
                }
                else if (Peek() == '>')
                {
                    Advance();
                    State.CurrentToken = new Token(TokenType.ShiftRight, ">>");
                }
                else
                {
                    State.CurrentToken = new Token(TokenType.CompareGreaterThan, '>');
                }
            }
            else if (State.Character == '&')
            {
                if (Peek() == '&')
                {
                    Advance();
                    State.CurrentToken = new Token(TokenType.And, "&&");
                }
                else
                {
                    if (Peek() == '=')
                    {
                        Advance();
                        State.CurrentToken = new Token(TokenType.AssignBitwiseAnd, "&=");
                    }
                    else
                    {
                        State.CurrentToken = new Token(TokenType.BitwiseAnd, "&");
                    }
                }
            }
            else if (State.Character == '|')
            {
                if (Peek() == '|')
                {
                    Advance();
                    State.CurrentToken = new Token(TokenType.Or, "||");
                }
                else
                {
                    if (Peek() == '=')
                    {
                        Advance();
                        State.CurrentToken = new Token(TokenType.AssignBitwiseOr, "|=");
                    }
                    else
                    {
                        State.CurrentToken = new Token(TokenType.BitwiseOr, "|");
                    }
                }
            }
            else if (State.Character == '^')
            {
                if (Peek() == '=')
                {
                    Advance();
                    State.CurrentToken = new Token(TokenType.AssignBitwiseXor, "^=");
                }
                else
                {
                    State.CurrentToken = new Token(TokenType.BitwiseXor, "^");
                }
            }
            else if (State.Character == '~')
            {
                State.CurrentToken = new Token(TokenType.BitwiseNot, "~");
            }
            else if (State.Character == ',')
            {
                State.CurrentToken = new Token(TokenType.Comma, ',');
            }
            else if (State.Character == ':')
            {
                State.CurrentToken = new Token(TokenType.Colon, ':');
            }
            else if (State.Character == ';')
            {
                State.CurrentToken = new Token(TokenType.Semicolon, ';');
            }
            else if (State.Character == '(')
            {
                State.CurrentToken = new Token(TokenType.LParenthesis, '(');
            }
            else if (State.Character == ')')
            {
                State.CurrentToken = new Token(TokenType.RParenthesis, ')');
            }
            else if (State.Character == '!')
            {
                if (Peek() == '=')
                {
                    Advance();
                    State.CurrentToken = new Token(TokenType.CompareNotEqual, "!=");
                }
                else
                {
                    State.CurrentToken = new Token(TokenType.Negation, '!');
                }
            }
            else if (State.Character == '[')
            {
                State.CurrentToken = new Token(TokenType.LBracket, '[');
            }
            else if (State.Character == ']')
            {
                State.CurrentToken = new Token(TokenType.RBracket, ']');
            }
            else if (State.Character == '{')
            {
                State.CurrentToken = new Token(TokenType.LBrace, '{');
            }
            else if (State.Character == '}')
            {
                State.CurrentToken = new Token(TokenType.RBrace, '}');
            }
            else if (char.IsLetter(State.Character) || State.Character == '_')
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
            else if (State.Character == '"')
            {
                State.CurrentToken = GetString();
                Advance();

                return;
            }
            else if (State.Character == (char)0)
            {
                State.CurrentToken = new Token(TokenType.EOF, "<EOF>");
            }
            else if (State.Character == '#')
            {
                State.CurrentToken = new Token(TokenType.Length, '#');
            }
            else if (State.Character == '@')
            {
                State.CurrentToken = new Token(TokenType.ToString, '@');
            }
            else
            {
                throw new ScannerException($"Unexpected token '{State.Character}'", State.Column, State.Line);
            }

            Advance();
        }

        private Token GetHexNumber()
        {
            var number = string.Empty;

            while (IsLegalHexNumberCharacter(State.Character))
            {
                number += State.Character;
                Advance();
            }

            return new Token(TokenType.HexNumber, int.Parse(number, NumberStyles.HexNumber));
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
                return new Token(TokenType.DecimalNumber, double.Parse(number));
            }
            catch (FormatException)
            {
                throw new ScannerException("Invalid fractional number format.", State.Column, State.Line);
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

            switch (str)
            {
                case "fn":
                    return new Token(TokenType.Fn, "fn");

                case "for":
                    return new Token(TokenType.For, "for");

                case "each":
                    return new Token(TokenType.Each, "each");

                case "undef":
                    return new Token(TokenType.Undef, "undef");

                case "while":
                    return new Token(TokenType.While, "while");

                case "var":
                    return new Token(TokenType.Var, "var");

                case "if":
                    return new Token(TokenType.If, "if");

                case "elif":
                    return new Token(TokenType.Elif, "elif");

                case "else":
                    return new Token(TokenType.Else, "else");

                case "break":
                    return new Token(TokenType.Break, "break");

                case "skip":
                    return new Token(TokenType.Skip, "skip");

                case "ret":
                    return new Token(TokenType.Ret, "ret");

                case "true":
                    return new Token(TokenType.True, "true");

                case "false":
                    return new Token(TokenType.False, "false");

                case "exit":
                    return new Token(TokenType.Exit, "exit");
            }

            return new Token(TokenType.Identifier, str);
        }

        private bool IsLegalIdentifierCharacter(char c)
        {
            return char.IsLetterOrDigit(c) || c == '.' || c == '_';
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
                    throw new ScannerException("Unterminated string.", State.Column, State.Line);

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
                            str += GetUnicodeCharacter();
                            continue;

                        case 'r':
                            str += '\r';
                            break;

                        default:
                            throw new ScannerException("Unrecognized escape sequence.", State.Column, State.Line);
                    }

                    Advance();
                    continue;
                }

                str += State.Character;
                Advance();
            }

            return new Token(TokenType.String, str);
        }

        private char GetUnicodeCharacter()
        {
            var code = string.Empty;

            for (var i = 0; i < 4; i++)
            {
                if (HexAlphabet.Contains(State.Character))
                {
                    code += State.Character;
                    Advance();
                }
                else
                {
                    throw new ScannerException("Invalid UTF-8 character code.", State.Column, State.Line);
                }
            }

            return (char)int.Parse(code, NumberStyles.HexNumber);
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