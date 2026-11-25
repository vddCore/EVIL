namespace EVIL.CoreTests;

using static EVIL.Lexical.Token;

using EVIL.Lexical;
using NUnit.Framework;
using Shouldly;

public class LexerTests
{
    protected Lexer _lexer = null!;

    protected Token CurrentToken => _lexer.State.CurrentToken;

    [SetUp]
    public virtual void Setup()
    {
        _lexer = new Lexer();
    }

    [TearDown]
    public virtual void Teardown()
    {
        _lexer = null!;
    }

    protected void InitializeWithSource(string source)
    {
        _lexer.LoadSource(source);
    }

    protected void Expect(params Token[] tokens)
    {
        foreach (var token in tokens)
        {
            CurrentToken.ShouldBe(token);
            _lexer.NextToken();
        }
    }

    protected void ExpectExact(params (TokenType ExpectedTokenType, string ExpectedValue)[] constraints)
    {
        foreach (var constraint in constraints)
        {
            CurrentToken.Type.ShouldBe(constraint.ExpectedTokenType);
            CurrentToken.Value.ShouldBe(constraint.ExpectedValue);

            _lexer.NextToken();
        }
    }

    [Test]
    public void Keywords()
    {
        _lexer.LoadSource(
            "typeof " +
            "while break yield false catch retry throw " +
            "skip each elif else true " +
            "val ret for nil try " +
            "if rw do in fn is !is"
        );

        Expect(TypeOf);
        Expect(While, Break, Yield, False, Catch, Retry, Throw);
        Expect(Skip, Each, Elif, Else, True);
        Expect(Val, Ret, For, Nil, Try);
        Expect(If, Rw, Do, In, Fn, Token.Is, IsNot);
        Expect(EOF);
    }

    [Test]
    public void Operators()
    {
        _lexer.LoadSource(
            "= += -= *= /= %= &= |= ^= >>= <<= " +
            "+ - / * % & ~ | ^ << >> " +
            "... => -> -- ++ && || ! " +
            "<==> <!=> == != > < >= <= " +
            "# $ @ ? : ; , . { } [ ] ( ) #[ " +
            "array self with"
        );

        Expect(
            Assign, AssignAdd, AssignSubtract, AssignMultiply,
            AssignDivide, AssignModulo, AssignBitwiseAnd, AssignBitwiseOr,
            AssignBitwiseXor, AssignShiftRight, AssignShiftLeft
        );

        Expect(
            Plus, Minus, Slash, Asterisk, Modulo, BitwiseAnd, BitwiseNot,
            BitwiseOr, BitwiseXor, ShiftLeft, ShiftRight
        );

        Expect(
            Ellipsis, Associate, RightArrow, Decrement, Increment,
            LogicalAnd, LogicalOr, LogicalNot
        );

        Expect(
            DeepEqual, DeepNotEqual, Equal, NotEqual, GreaterThan,
            LessThan, GreaterThanOrEqual, LessThanOrEqual
        );

        Expect(
            Length, AsNumber, AsString, QuestionMark, Colon, Semicolon,
            Comma, Dot, LBrace, RBrace, LBracket, RBracket, LParenthesis,
            RParenthesis, AttributeList
        );

        Expect(Array, Self, With);

        Expect(EOF);
    }

    [Test]
    public void Aliases()
    {
        _lexer.LoadSource("NaN Infinity");
        Expect(NaN, Infinity, EOF);
    }

    [Test]
    public void Identifiers()
    {
        _lexer.LoadSource("identifier_1 identifier_2 identifier_3");

        ExpectExact(
            (TokenType.Identifier, "identifier_1"),
            (TokenType.Identifier, "identifier_2"),
            (TokenType.Identifier, "identifier_3")
        );

        Expect(EOF);
    }

    [Test]
    public void DecimalNumbers()
    {
        _lexer.LoadSource("- 2 (-2) -32E03 21.37 44.11 .42 27.0 31.1 .1 12");

        Expect(Minus);
        ExpectExact((TokenType.Number, "2"));
        Expect(LParenthesis);
        Expect(Minus);
        ExpectExact((TokenType.Number, "2"));
        Expect(RParenthesis);
        Expect(Minus);
        
        ExpectExact(
            (TokenType.Number, "32E03"),
            (TokenType.Number, "21.37"),
            (TokenType.Number, "44.11"),
            (TokenType.Number, ".42"),
            (TokenType.Number, "27.0"),
            (TokenType.Number, "31.1"),
            (TokenType.Number, ".1"),
            (TokenType.Number, "12")
        );

        Expect(EOF);
    }

    [Test]
    public void HexIntegers()
    {
        _lexer.LoadSource(
            "0x1 0x12 0x123 0x1234 0x12345 0x123456 0x1234567 " +
            "0x12345678 0xDEADBEEF13371 0x1D34d 0x2CafE 0x3b33f " +
            "-0x1234"
        );

        ExpectExact(
            (TokenType.HexInteger, "1"),
            (TokenType.HexInteger, "12"),
            (TokenType.HexInteger, "123"),
            (TokenType.HexInteger, "1234"),
            (TokenType.HexInteger, "12345"),
            (TokenType.HexInteger, "123456"),
            (TokenType.HexInteger, "1234567"),
            (TokenType.HexInteger, "12345678"),
            (TokenType.HexInteger, "DEADBEEF13371"),
            (TokenType.HexInteger, "1D34d"),
            (TokenType.HexInteger, "2CafE"),
            (TokenType.HexInteger, "3b33f")
        );

        Expect(Minus);
        ExpectExact((TokenType.HexInteger, "1234"));

        Expect(EOF);
    }

    [Test]
    public void Strings()
    {
        _lexer.LoadSource(
            "'single-quoted string' " +
            "'single-quoted string with \\' escape' " +
            "\"double quoted string\" " +
            "\"double quoted string with \\\" escape\" " +
            "'this is a \\uABCD \\x1234'"
        );

        ExpectExact(
            (TokenType.PlainString, "single-quoted string"),
            (TokenType.PlainString, "single-quoted string with ' escape"),
            (TokenType.InterpolatedString, "double quoted string"),
            (TokenType.InterpolatedString, "double quoted string with \" escape"),
            (TokenType.PlainString, "this is a \uABCD \x1234")
        );

        Expect(EOF);
    }

    [Test]
    public void StringDollarEscape()
    {
        _lexer.LoadSource(
            "\"double \\$var\" 'single \\$var'"
        );

        ExpectExact(
            (TokenType.InterpolatedString, "double \\$var"),
            (TokenType.PlainString, "single $var")
        );

        Expect(EOF);
    }

    [Test]
    public void NotInAndIsNotOrthogonality()
    {
        _lexer.LoadSource("!in !is !in(x) !is(x) !in/*comment*/y !inside !island");

        Expect(NotIn, IsNot);
        Expect(NotIn, LParenthesis);
        ExpectExact((TokenType.Identifier, "x"));
        Expect(RParenthesis);
        Expect(IsNot, LParenthesis);
        ExpectExact((TokenType.Identifier, "x"));
        Expect(RParenthesis);
        Expect(NotIn);
        ExpectExact((TokenType.Identifier, "y"));
        Expect(LogicalNot);
        ExpectExact((TokenType.Identifier, "inside"));
        Expect(LogicalNot);
        ExpectExact((TokenType.Identifier, "island"));
        Expect(EOF);
    }

    [Test]
    public void TypeCodes()
    {
        _lexer.LoadSource(
            "Nil Number Boolean String " +
            "Table Function Type " +
            "NativeFunction NativeObject Fn"
        );

        Expect(NilTypeCode, NumberTypeCode, BooleanTypeCode, StringTypeCode);
        Expect(TableTypeCode, ChunkTypeCode, TypeCodeTypeCode);
        Expect(NativeFunctionTypeCode, NativeObjectTypeCode, ChunkTypeCode);
        Expect(EOF);
    }

    [Test]
    public void SafeInvocationTokens()
    {
        _lexer.LoadSource("obj?:method() func?(arg)");

        ExpectExact((TokenType.Identifier, "obj"));
        Expect(SafeColon);
        ExpectExact((TokenType.Identifier, "method"));
        Expect(LParenthesis, RParenthesis);
        ExpectExact((TokenType.Identifier, "func"));
        Expect(ElvisInvocation);
        ExpectExact((TokenType.Identifier, "arg"));
        Expect(RParenthesis);
        Expect(EOF);
    }

    [Test]
    public void SafeSpreadTokens()
    {
        _lexer.LoadSource("{ ...?opts, ...strict }");

        Expect(LBrace);
        Expect(SafeSpread);
        ExpectExact((TokenType.Identifier, "opts"));
        Expect(Comma);
        Expect(Ellipsis);
        ExpectExact((TokenType.Identifier, "strict"));
        Expect(RBrace);
        Expect(EOF);
    }

    [Test]
    public void RangeTokenDoesNotChangeDotOrSpreadTokens()
    {
        _lexer.LoadSource("1..4 . .. ... ...? .5");

        ExpectExact(
            (TokenType.Number, "1"),
            (TokenType.Range, ".."),
            (TokenType.Number, "4"),
            (TokenType.Dot, "."),
            (TokenType.Range, ".."),
            (TokenType.Ellipsis, "..."),
            (TokenType.SafeSpread, "...?"),
            (TokenType.Number, ".5")
        );

        Expect(EOF);
    }

    [Test]
    public void InterpolatedStringExpressions()
    {
        _lexer.LoadSource(
            "\"result: ${1 + 2}\" " +
            "\"nested: ${flag ? \"yes\" : \"no\"}\" " +
            "\"table: ${{ a: 1 }.a}\""
        );

        ExpectExact(
            (TokenType.InterpolatedString, "result: ${1 + 2}"),
            (TokenType.InterpolatedString, "nested: ${flag ? \"yes\" : \"no\"}"),
            (TokenType.InterpolatedString, "table: ${{ a: 1 }.a}")
        );

        Expect(EOF);
    }
}