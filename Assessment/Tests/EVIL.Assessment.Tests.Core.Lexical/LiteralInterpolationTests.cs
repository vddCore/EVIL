namespace EVIL.Assessment.Tests.Core.Lexical;

using static EVIL.Core.Lexical.Token;

public class LiteralInterpolationTests : LexerTestBase
{
    [Test]
    public void DecimalNumbers()
    {
        _lexer.LoadSource(
            "- 2 (-2) -32E03 21.37 44.11 .42 27.0 31.1 .1 12 " +
            "3e4 5E+6 7e-8 9.1e+2"
        );

        Expect(
            Minus, CreateNumber("2"), LParenthesis, Minus, CreateNumber("2"),
            RParenthesis, Minus, CreateNumber("32E03"), CreateNumber("21.37"),
            CreateNumber("44.11"), CreateNumber(".42"), CreateNumber("27.0"),
            CreateNumber("31.1"), CreateNumber(".1"), CreateNumber("12"),
            CreateNumber("3e4"), CreateNumber("5E+6"), CreateNumber("7e-8"),
            CreateNumber("9.1e+2"), EOF
        );
    }

    [Test]
    public void HexIntegers()
    {
        _lexer.LoadSource(
            "0x1 0x12 0x123 0x1234 0x12345 0x123456 0x1234567 " +
            "0x12345678 0xDEADBEEF13371 0x1D34d 0x2CafE 0x3b33f " +
            "-0x1234"
        );

        Expect(
            CreateHexInteger("1"), CreateHexInteger("12"),
            CreateHexInteger("123"), CreateHexInteger("1234"),
            CreateHexInteger("12345"), CreateHexInteger("123456"),
            CreateHexInteger("1234567"), CreateHexInteger("12345678"),
            CreateHexInteger("DEADBEEF13371"), CreateHexInteger("1D34d"),
            CreateHexInteger("2CafE"), CreateHexInteger("3b33f"), Minus,
            CreateHexInteger("1234"), EOF
        );
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

        Expect(
            CreatePlainString("single-quoted string"),
            CreatePlainString("single-quoted string with ' escape"),
            CreateInterpolatedString("double quoted string"),
            CreateInterpolatedString("double quoted string with \" escape"),
            CreatePlainString("this is a \uABCD \x1234"), EOF
        );
    }

    [Test]
    public void StringDollarEscape()
    {
        _lexer.LoadSource(
            "\"double \\$var\" 'single \\$var'"
        );

        Expect(
            CreateInterpolatedString("double \\$var"),
            CreatePlainString("single $var"), EOF
        );
    }

    [Test]
    public void StringEscapeSequences()
    {
        _lexer.LoadSource("'\\a\\b\\f\\n\\r\\t\\v\\\"\\'\\\\\\$\\u0041\\x0042'");

        Expect(CreatePlainString("\a\b\f\n\r\t\v\"'\\$AB"), EOF);
    }

    [Test]
    public void InterpolatedStringVariableAndNestedBraces_PreserveSource()
    {
        _lexer.LoadSource(
            "\"value: $name\" " +
            "\"table: ${{ outer: { inner: 1 } }}\""
        );

        Expect(
            CreateInterpolatedString("value: $name"),
            CreateInterpolatedString("table: ${{ outer: { inner: 1 } }}"), EOF
        );
    }

    [Test]
    public void InterpolatedStringNestedQuotesAndComments_PreserveSource()
    {
        _lexer.LoadSource(
            "\"nested: ${flag ? \"yes\" : \"no\"}\" " +
            "\"${call(\"value\", /* block { comment } */ { item: 1 }, // line }\n 2)}\""
        );

        Expect(
            CreateInterpolatedString("nested: ${flag ? \"yes\" : \"no\"}"),
            CreateInterpolatedString("${call(\"value\", /* block { comment } */ { item: 1 }, // line }\n 2)}"),
            EOF
        );
    }

    [Test]
    public void InterpolatedStringExpressions_NestedStringConcatenation()
    {
        _lexer.LoadSource("\"${\"string\"+\"string\"}.\"");

        Expect(
            CreateInterpolatedString("${\"string\"+\"string\"}."), EOF
        );
    }
}