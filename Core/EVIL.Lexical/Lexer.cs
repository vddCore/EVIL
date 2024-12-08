namespace EVIL.Lexical;

using System;
using System.Globalization;
using System.Text;

public class Lexer
{
    private const string HexAlphabet = "abcdefABCDEF0123456789";

    private string? _sourceCode;

    public LexerState State { get; internal set; } = new();

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
        var (line, col) = (State.Line, State.Column);

        switch (State.Character)
        {
            case '.' when Peek() == '.' && Peek(2) == '.':
                Advance();
                Advance();
                State.CurrentToken = Token.Ellipsis with { Line = line, Column = col };
                break;
            case '-' when Peek() == '>':
                Advance();
                State.CurrentToken = Token.RightArrow with { Line = line, Column = col };
                break;
            case '-' when Peek() == '-':
                Advance();
                State.CurrentToken = Token.Decrement with { Line = line, Column = col };
                break;
            case '-' when Peek() == '=':
                Advance();
                State.CurrentToken = Token.AssignSubtract with { Line = line, Column = col };
                break;
            case '-':
                State.CurrentToken = Token.Minus with { Line = line, Column = col };
                break;
            case '.' when char.IsDigit(Peek()):
                State.CurrentToken = GetDecimalNumber() with { Line = line, Column = col };
                return;
            case '.':
                State.CurrentToken = Token.Dot with { Line = line, Column = col };
                break;
            case '+' when Peek() == '+':
                Advance();
                State.CurrentToken = Token.Increment with { Line = line, Column = col };
                break;
            case '+' when Peek() == '=':
                Advance();
                State.CurrentToken = Token.AssignAdd with { Line = line, Column = col };
                break;
            case '+':
                State.CurrentToken = Token.Plus with { Line = line, Column = col };
                break;
            case '*' when Peek() == '=':
                Advance();
                State.CurrentToken = Token.AssignMultiply with { Line = line, Column = col };
                break;
            case '*':
                State.CurrentToken = Token.Multiply with { Line = line, Column = col };
                break;
            case '/' when Peek() == '/':
                SkipLineComment();

                Advance();
                NextToken();

                return;
            case '/' when Peek() == '*':
                SkipBlockComment();
                    
                Advance();
                NextToken();
                    
                return;
            case '/' when Peek() == '=':
                Advance();
                State.CurrentToken = Token.AssignDivide with { Line = line, Column = col };
                break;
            case '/':
                State.CurrentToken = Token.Divide with { Line = line, Column = col };
                break;
            case '?' when Peek() == '?' && Peek(2) == '=':
                Advance();
                Advance();
                State.CurrentToken = Token.AssignCoalesce with { Line = line, Column = col };
                break;
            case '?' when Peek() == '?':
                Advance();
                State.CurrentToken = Token.DoubleQuestionMark with { Line = line, Column = col };
                break;
            case '?':
                State.CurrentToken = Token.QuestionMark with { Line = line, Column = col };
                break;
            case '%' when Peek() == '=':
                Advance();
                State.CurrentToken = Token.AssignModulo with { Line = line, Column = col };
                break;
            case '%':
                State.CurrentToken = Token.Modulo with { Line = line, Column = col };
                break;
            case '=' when Peek() == '>':
                Advance();
                State.CurrentToken = Token.Associate with { Line = line, Column = col };
                break;
            case '=' when Peek() == '=':
                Advance();
                State.CurrentToken = Token.Equal with { Line = line, Column = col };
                break;
            case '=':
                State.CurrentToken = Token.Assign with { Line = line, Column = col };
                break;
            case '<' when Peek() == '=' && Peek(2) == '=' && Peek(3) == '>':
                Advance();
                Advance();
                Advance();
                State.CurrentToken = Token.DeepEqual with { Line = line, Column = col };
                break;
            case '<' when Peek() == '!' && Peek(2) == '=' && Peek(3) == '>':
                Advance();
                Advance();
                Advance();
                State.CurrentToken = Token.DeepNotEqual with { Line = line, Column = col };
                break;
            case '<' when Peek() == '=':
                Advance();
                State.CurrentToken = Token.LessThanOrEqual with { Line = line, Column = col };
                break;
            case '<' when Peek() == '<':
            {
                Advance();
                if (Peek() == '=')
                {
                    Advance();
                    State.CurrentToken = Token.AssignShiftLeft with { Line = line, Column = col };
                }
                else
                {
                    State.CurrentToken = Token.ShiftLeft with { Line = line, Column = col };
                }

                break;
            }
            case '<':
                State.CurrentToken = Token.LessThan with { Line = line, Column = col };
                break;
            case '>' when Peek() == '=':
                Advance();
                State.CurrentToken = Token.GreaterThanOrEqual with { Line = line, Column = col };
                break;
            case '>' when Peek() == '>':
            {
                Advance();
                if (Peek() == '=')
                {
                    Advance();
                    State.CurrentToken = Token.AssignShiftRight with { Line = line, Column = col };
                }
                else
                {
                    State.CurrentToken = Token.ShiftRight with { Line = line, Column = col };
                }

                break;
            }
            case '>':
                State.CurrentToken = Token.GreaterThan with { Line = line, Column = col };
                break;
            case '&' when Peek() == '&':
                Advance();
                State.CurrentToken = Token.LogicalAnd with { Line = line, Column = col };
                break;
            case '&' when Peek() == '=':
                Advance();
                State.CurrentToken = Token.AssignBitwiseAnd with { Line = line, Column = col };
                break;
            case '&':
                State.CurrentToken = Token.BitwiseAnd with { Line = line, Column = col };
                break;
            case '|' when Peek() == '|':
                Advance();
                State.CurrentToken = Token.LogicalOr with { Line = line, Column = col };
                break;
            case '|' when Peek() == '=':
                Advance();
                State.CurrentToken = Token.AssignBitwiseOr with { Line = line, Column = col };
                break;
            case '|':
                State.CurrentToken = Token.BitwiseOr with { Line = line, Column = col };
                break;
            case '^' when Peek() == '=':
                Advance();
                State.CurrentToken = Token.AssignBitwiseXor with { Line = line, Column = col };
                break;
            case '^':
                State.CurrentToken = Token.BitwiseXor with { Line = line, Column = col };
                break;
            case '~':
                State.CurrentToken = Token.BitwiseNot with { Line = line, Column = col };
                break;
            case ',':
                State.CurrentToken = Token.Comma with { Line = line, Column = col };
                break;
            case ':' when Peek() == ':':
                Advance();
                State.CurrentToken = Token.DoubleColon with { Line = line, Column = col };
                break;
            case ':':
                State.CurrentToken = Token.Colon with { Line = line, Column = col };
                break;
            case ';':
                State.CurrentToken = Token.Semicolon with { Line = line, Column = col };
                break;
            case '(':
                State.CurrentToken = Token.LParenthesis with { Line = line, Column = col };
                break;
            case ')':
                State.CurrentToken = Token.RParenthesis with { Line = line, Column = col };
                break;
            case '!' when Peek() == 'i' && Peek(2) == 'n' && char.IsWhiteSpace(Peek(3)):
                Advance();
                Advance();
                State.CurrentToken = Token.NotIn with { Line = line, Column = col };
                break;
            case '!' when Peek() == 'i' && Peek(2) == 's' && char.IsWhiteSpace(Peek(3)):
                Advance();
                Advance();
                State.CurrentToken = Token.IsNot with { Line = line, Column = col };
                break;
            case '!' when Peek() == '=':
                Advance();
                State.CurrentToken = Token.NotEqual with { Line = line, Column = col };
                break;
            case '!':
                State.CurrentToken = Token.LogicalNot with { Line = line, Column = col };
                break;
            case '[':
                State.CurrentToken = Token.LBracket with { Line = line, Column = col };
                break;
            case ']':
                State.CurrentToken = Token.RBracket with { Line = line, Column = col };
                break;
            case '{':
                State.CurrentToken = Token.LBrace with { Line = line, Column = col };
                break;
            case '}':
                State.CurrentToken = Token.RBrace with { Line = line, Column = col };
                break;
            case (char)0:
                State.CurrentToken = Token.EOF with { Line = line, Column = col };
                break;
            case '"':
            case '\'':
                State.CurrentToken = GetString() with { Line = line, Column = col };
                break;
            case '#' when Peek() == '[':
                Advance();
                State.CurrentToken = Token.AttributeList with { Line = line, Column = col };
                break;
            case '#':
                State.CurrentToken = Token.Length with { Line = line, Column = col };
                break;
            case '@':
                State.CurrentToken = Token.AsString with { Line = line, Column = col };
                break;
            case '$':
                State.CurrentToken = Token.AsNumber with { Line = line, Column = col };
                break;

            default:
            {
                if (char.IsLetter(State.Character) || State.Character == '_')
                {
                    State.CurrentToken = GetIdentifierOrKeyword() with { Line = line, Column = col };
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

                            State.CurrentToken = GetHexNumber() with { Line = line, Column = col };
                            return;
                        }
                    }

                    State.CurrentToken = GetDecimalNumber() with { Line = line, Column = col };
                    return;
                }
            }

                throw new LexerException($"Unexpected token '{State.Character}'", col, line);
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

        if (char.IsDigit(State.Character) || State.Character == '.')
        {
            var foundDecimalSeparator = false;
            var foundExponent = false;
            var foundExponentSign = false;
                
            while (char.IsDigit(State.Character) || State.Character == '.' 
                                                 || State.Character == 'e' 
                                                 || State.Character == 'E' 
                                                 || State.Character == '-' 
                                                 || State.Character == '+')
            {
                if (State.Character == '.')
                {
                    if (foundDecimalSeparator)
                    {
                        throw new LexerException(
                            $"Character `{State.Character}' was unexpected at this time.",
                            State.Line,
                            State.Column
                        );
                    }
                        
                    foundDecimalSeparator = true;
                }

                if (State.Character == 'e' || State.Character == 'E')
                {
                    if (foundExponent)
                    {
                        throw new LexerException(
                            $"Character `{State.Character}' was unexpected at this time.",
                            State.Line,
                            State.Column
                        );
                    }

                    foundExponent = true;
                }
                    
                if (State.Character == '+' || State.Character == '-')
                {
                    if (!foundExponent)
                        break;
                    
                    if (foundExponentSign)
                    {
                        throw new LexerException(
                            $"Character `{State.Character}' was unexpected at this time.",
                            State.Line,
                            State.Column
                        );
                    }

                    foundExponent = true;
                }
                    
                number += State.Character;
                Advance();
            }
        }
        else
        {
            throw new LexerException(
                $"Character `{State.Character}' was unexpected at this time.",
                State.Line,
                State.Column
            );
        }

        try
        {
            return Token.CreateNumber(number);
        }
        catch (FormatException)
        {
            throw new LexerException($"Malformed numeric constant `{number}'.", State.Line, State.Column);
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
            "loc" => Token.Loc,
            "for" => Token.For,
            "while" => Token.While,
            "do" => Token.Do,
            "rw" => Token.Rw,
            "val" => Token.Val,
            "in" => Token.In,
            "if" => Token.If,
            "is" => Token.Is,
            "elif" => Token.Elif,
            "else" => Token.Else,
            "break" => Token.Break,
            "skip" => Token.Skip,
            "by" => Token.By,
            "ret" => Token.Ret,
            "true" => Token.True,
            "false" => Token.False,
            "nil" => Token.Nil,
            "typeof" => Token.TypeOf,
            "try" => Token.Try,
            "catch" => Token.Catch,
            "throw" => Token.Throw,
            "yield" => Token.Yield,
            "each" => Token.Each,
            "array" => Token.Array,
            "self" => Token.Self,
            "error" => Token.Error,
            "retry" => Token.Retry,
            "with" => Token.With,
            "Infinity" => Token.Infinity,
            "NaN" => Token.NaN,
            "Nil" => Token.NilTypeCode,
            "Number" => Token.NumberTypeCode,
            "String" => Token.StringTypeCode,
            "Boolean" => Token.BooleanTypeCode,
            "Table" => Token.TableTypeCode,
            "Array" => Token.ArrayTypeCode,
            "Fiber" => Token.FiberTypeCode,
            "Error" => Token.ErrorTypeCode,
            "Function" => Token.ChunkTypeCode,
            "Type" => Token.TypeCodeTypeCode,
            "NativeFunction" => Token.NativeFunctionTypeCode,
            "NativeObject" => Token.NativeObjectTypeCode,
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
        var (line, col) = (State.Line, State.Column);

        var str = string.Empty;
        var delimiter = State.Character;

        if (delimiter != '"' && delimiter != '\'')
        {
            throw new LexerException(
                "Strings can only be delimited by '' or \"\".",
                line, col
            );
        }

        Advance();

        while (State.Character != delimiter)
        {
            if (State.Character == (char)0)
                throw new LexerException("Unterminated string.", line, col);

            if (State.Character == '\\')
            {
                Advance();

                switch (State.Character)
                {
                    case 'a':
                        str += '\a';
                        break;
                    case 'b':
                        str += '\b';
                        break;
                    case 'f':
                        str += '\f';
                        break;
                    case 'n':
                        str += '\n';
                        break;
                    case 'r':
                        str += '\r';
                        break;
                    case 't':
                        str += '\t';
                        break;
                    case 'x':
                    case 'u':
                        Advance();
                        str += GetUnicodeSequence();
                        continue;
                    case 'v':
                        str += '\v';
                        break;
                    case '"':
                        str += '"';
                        break;
                    case '\'':
                        str += '\'';
                        break;
                    case '\\':
                        str += '\\';
                        break;
                    case '$':
                        str += '$';
                        break;
                    default:
                        throw new LexerException("Unrecognized escape sequence.", State.Line, State.Column);
                }

                Advance();
                continue;
            }

            str += State.Character;
            Advance();
        }

        if (delimiter == '\'')
        {
            return Token.CreatePlainString(str);
        }
        else // '"'
        {
            return Token.CreateInterpolatedString(str);
        }
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
                    State.Line, State.Column);
            }
        }

        return (char)int.Parse(sb.ToString(), NumberStyles.HexNumber);
    }

    private void SkipLineComment()
    {
        while (State.Character != '\n' && State.Character != (char)0)
            Advance();
    }

    private void SkipBlockComment()
    {
        while (true)
        {
            if (Peek() == '*' && Peek(2) == '/')
            {
                Advance();
                Advance();
                    
                break;
            }

            if (State.Character == 0)
                break;
                
            Advance();
        }
    }

    private void SkipWhitespace()
    {
        while (char.IsWhiteSpace(State.Character) || State.Character == 0xFEFF)
            Advance();
    }

    private char Peek(int howFar = 1)
    {
        if (State.Pointer + howFar >= _sourceCode!.Length)
            return (char)0;

        return _sourceCode[State.Pointer + howFar];
    }

    private string PeekString(int length = 1)
    {
        var sb = new StringBuilder();

        for (var i = 1; i <= length; i++)
        {
            if (State.Pointer + i >= _sourceCode!.Length)
                break;

            sb.Append(_sourceCode[State.Pointer + i]);
        }

        return sb.ToString();
    }

    private void Advance(int howFar = 1)
    {
        for (var i = 0; i < howFar; i++)
        {
            if (State.Character == '\n')
            {
                State.Column = 1;
                State.Line++;
            }
            else
            {
                State.Column++;
            }

            if (State.Pointer + 1 < _sourceCode!.Length)
            {
                State.Pointer++;
                State.Character = _sourceCode[State.Pointer];
            }
            else
            {
                State.Character = '\0';
            }
        }
    }
}