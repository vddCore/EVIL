namespace EVIL.Lexical;

using System;
using System.Globalization;
using System.Text;
using EVIL.CommonTypes.Diagnostics;

public class Lexer
{
    private const string HexAlphabet = "abcdefABCDEF0123456789";

    private string? _sourceCode;

    public LexerState State { get; private set; } = new();
    public IDiagnosticSink? DiagnosticSink { get; set; }
    public string? FilePath { get; set; }

    public Lexer(IDiagnosticSink? diagnosticSink = null, string? filePath = null)
    {
        DiagnosticSink = diagnosticSink;
        FilePath = filePath;
    }

    private void ReportLexerError(DiagnosticDescriptor descriptor, SourceLocation location, string fallbackMessage, params object[] args)
    {
        if (DiagnosticSink != null)
        {
            DiagnosticSink.Report(descriptor.Create(location, args));
        }
        else
        {
            var msg = args.Length > 0 ? string.Format(fallbackMessage, args) : fallbackMessage;
            throw new LexerException(msg, location);
        }
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
        while (true)
        {
            SkipWhitespace();

            State.PreviousToken = State.CurrentToken;
            var (line, col) = (State.Line, State.Column);

            switch (State.Character)
            {
                case '.' when Peek() == '.' && Peek(2) == '.':
                    Advance(2);
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
                    State.CurrentToken = Token.Asterisk with { Line = line, Column = col };
                    break;
                case '/' when Peek() == '/':
                    SkipLineComment();

                    Advance();
                    continue;

                case '/' when Peek() == '*':
                    SkipBlockComment();

                    Advance();
                    continue;

                case '/' when Peek() == '=':
                    Advance();
                    State.CurrentToken = Token.AssignDivide with { Line = line, Column = col };
                    break;
                case '/':
                    State.CurrentToken = Token.Slash with { Line = line, Column = col };
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
                case '?' when Peek() == '.':
                    Advance();
                    State.CurrentToken = Token.Elvis with { Line = line, Column = col };
                    break;
                case '?' when Peek() == '[':
                    Advance();
                    State.CurrentToken = Token.ElvisArray with { Line = line, Column = col };
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
                    Advance(3);
                    State.CurrentToken = Token.DeepEqual with { Line = line, Column = col };
                    break;
                case '<' when Peek() == '!' && Peek(2) == '=' && Peek(3) == '>':
                    Advance(3);
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
                case '!' when Peek() == 'i' && Peek(2) == 'n' && !IsLegalIdentifierCharacter(Peek(3)):
                    Advance(2);
                    State.CurrentToken = Token.NotIn with { Line = line, Column = col };
                    break;
                case '!' when Peek() == 'i' && Peek(2) == 's' && !IsLegalIdentifierCharacter(Peek(3)):
                    Advance(2);
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
                                Advance(2);

                                State.CurrentToken = GetHexNumber() with { Line = line, Column = col };
                                return;
                            }
                        }

                        State.CurrentToken = GetDecimalNumber() with { Line = line, Column = col };
                        return;
                    }
                }

                ReportLexerError(
                    CompilerDiagnostics.UnexpectedCharacter,
                    new SourceLocation(FilePath, line, col),
                    "Unexpected token '{0}'",
                    State.Character
                );
                Advance();
                break;
            }

            Advance();
            break;
        }
    }

    public Token PeekToken(int howFar)
    {
        var prevState = State with { };
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

        bool foundDecimal = false;
        bool foundExponent = false;
        
        if (!char.IsDigit(State.Character) && State.Character != '.')
        {
            ReportLexerError(
                CompilerDiagnostics.UnexpectedCharacter,
                new SourceLocation(FilePath, State.Line, State.Column),
                "Character `{0}` was unexpected at this time.",
                State.Character
            );
            return Token.CreateNumber("0");
        }

        while (true)
        {
            if (char.IsDigit(State.Character))
            {
                number += State.Character;
                Advance();
            }
            else if (State.Character == '.')
            {
                if (foundDecimal)
                {
                    ReportLexerError(
                        CompilerDiagnostics.MalformedNumericConstant,
                        new SourceLocation(FilePath, State.Line, State.Column),
                        "Unexpected second decimal point."
                    );
                    Advance();
                    continue;
                }

                foundDecimal = true;
                number += State.Character;
                Advance();
            }
            else if (State.Character is 'e' or 'E')
            {
                if (foundExponent)
                {
                    ReportLexerError(
                        CompilerDiagnostics.MalformedNumericConstant,
                        new SourceLocation(FilePath, State.Line, State.Column),
                        "Unexpected second exponent."
                    );
                    Advance();
                    continue;
                }

                foundExponent = true;
                number += State.Character;
                Advance();

                if (State.Character is '+' or '-')
                {
                    number += State.Character;
                    Advance();
                }
                else if (!char.IsDigit(State.Character))
                {
                    ReportLexerError(
                        CompilerDiagnostics.MalformedNumericConstant,
                        new SourceLocation(FilePath, State.Line, State.Column),
                        "Malformed exponent."
                    );
                }
            }
            else
            {
                break;
            }
        }

        try
        {
            return Token.CreateNumber(number);
        }
        catch (FormatException)
        {
            ReportLexerError(
                CompilerDiagnostics.MalformedNumericConstant,
                new SourceLocation(FilePath, State.Line, State.Column),
                "Malformed numeric constant `{0}`.",
                number
            );
            return Token.CreateNumber("0");
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
            "Function" or "Fn" => Token.ChunkTypeCode,
            "Type" => Token.TypeCodeTypeCode,
            "NativeFunction" => Token.NativeFunctionTypeCode,
            "NativeObject" => Token.NativeObjectTypeCode,
            _ => Token.CreateIdentifier(str)
        };
    }

    private static bool IsLegalIdentifierCharacter(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_';
    }

    private static bool IsLegalHexNumberCharacter(char c)
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
            ReportLexerError(
                CompilerDiagnostics.UnexpectedCharacter,
                new SourceLocation(FilePath, line, col),
                "Strings can only be delimited by '' or \"\"."
            );
            return Token.CreatePlainString(string.Empty);
        }

        Advance();

        while (State.Character != delimiter)
        {
            switch (State.Character)
            {
                case (char)0:
                {
                    ReportLexerError(
                        CompilerDiagnostics.UnterminatedString,
                        new SourceLocation(FilePath, line, col),
                        "Unterminated string."
                    );
                    return delimiter == '\'' ? Token.CreatePlainString(str) : Token.CreateInterpolatedString(str);
                }

                case '\\':
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
                            if (delimiter == '"')
                                str += "\\$";
                            else
                                str += '$';
                            break;
                        default:
                            ReportLexerError(
                                CompilerDiagnostics.UnexpectedCharacter,
                                new SourceLocation(FilePath, State.Line, State.Column),
                                "Unrecognized escape sequence."
                            );
                            str += State.Character;
                            break;
                    }

                    Advance();
                    continue;
                }

                case '$' when delimiter == '"' && Peek() == '{':
                {
                    str += "${";
                    Advance(2);

                    var braceDepth = 1;
                    while (braceDepth > 0 && State.Character != (char)0)
                    {
                        if (State.Character == '\\')
                        {
                            str += State.Character;
                            Advance();
                            if (State.Character != (char)0)
                            {
                                str += State.Character;
                                Advance();
                            }
                            continue;
                        }

                        if (State.Character == '/' && Peek() == '/')
                        {
                            str += "//";
                            Advance(2);
                            while (State.Character != '\n' && State.Character != (char)0)
                            {
                                str += State.Character;
                                Advance();
                            }
                            continue;
                        }

                        if (State.Character == '/' && Peek() == '*')
                        {
                            str += "/*";
                            Advance(2);
                            while (State.Character != (char)0)
                            {
                                if (State.Character == '*' && Peek() == '/')
                                {
                                    str += "*/";
                                    Advance(2);
                                    break;
                                }

                                str += State.Character;
                                Advance();
                            }
                            continue;
                        }

                        if (State.Character == '"' || State.Character == '\'')
                        {
                            var quote = State.Character;
                            str += quote;
                            Advance();

                            while (State.Character != quote && State.Character != (char)0)
                            {
                                if (State.Character == '\\')
                                {
                                    str += State.Character;
                                    Advance();
                                    if (State.Character != (char)0)
                                    {
                                        str += State.Character;
                                        Advance();
                                    }
                                    continue;
                                }

                                str += State.Character;
                                Advance();
                            }

                            if (State.Character == quote)
                            {
                                str += quote;
                                Advance();
                            }
                            continue;
                        }

                        if (State.Character == '{')
                        {
                            braceDepth++;
                            str += '{';
                            Advance();
                            continue;
                        }

                        if (State.Character == '}')
                        {
                            braceDepth--;
                            str += '}';
                            Advance();
                            continue;
                        }

                        str += State.Character;
                        Advance();
                    }

                    if (braceDepth > 0)
                    {
                        ReportLexerError(
                            CompilerDiagnostics.UnterminatedInterpolatedExpression,
                            new SourceLocation(FilePath, line, col),
                            "Unterminated expression in interpolated string."
                        );
                    }

                    continue;
                }

                default:
                {
                    str += State.Character;
                    Advance();
                    break;
                }
            }
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
                ReportLexerError(
                    CompilerDiagnostics.UnexpectedCharacter,
                    new SourceLocation(FilePath, State.Line, State.Column),
                    "Invalid universal character code."
                );
                return '?';
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
                Advance(2);
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