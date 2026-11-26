namespace EVIL.Assessment.Tests.Core.Lexical;

using EVIL.Core.Lexical;
using static EVIL.Core.Lexical.Token;

public class TokenVocabularyTests : LexerTestBase
{
    [Test]
    public void ReservedWordsAndWordOperators_AreRecognizedCorrectly()
    {
        _lexer.LoadSource(
            "fn loc for while do rw val in if is elif else break skip by ret " +
            "true false nil typeof try catch throw yield each array self error retry with"
        );

        Expect(
            Fn, Loc, For, While, Do, Rw, Val, In, If, Is, Elif, Else, Break,
            Skip, By, Ret, True, False, Nil, TypeOf, Try, Catch, Throw, Yield,
            Each, Array, Self, Error, Retry, With, EOF
        );
    }

    [Test]
    public void TypeCodesAndAliases_AreRecognizedCorrectly()
    {
        _lexer.LoadSource(
            "Nil Number String Boolean Table Array Fiber Function Fn Error Type " +
            "NativeFunction NativeObject NaN Infinity"
        );

        Expect(
            NilTypeCode, NumberTypeCode, StringTypeCode, BooleanTypeCode,
            TableTypeCode, ArrayTypeCode, FiberTypeCode, ChunkTypeCode,
            ChunkTypeCode, ErrorTypeCode, TypeCodeTypeCode,
            NativeFunctionTypeCode, NativeObjectTypeCode, NaN, Infinity, EOF
        );
    }

    [Test]
    public void IdentifiersAndKeywordLookalikes_ArePlainIdentifiers()
    {
        _lexer.LoadSource(
            "fnx xfn val1 _val in_ _in trueValue nilValue Function_ nan infinity"
        );

        Expect(
            CreateIdentifier("fnx"), CreateIdentifier("xfn"),
            CreateIdentifier("val1"), CreateIdentifier("_val"),
            CreateIdentifier("in_"), CreateIdentifier("_in"),
            CreateIdentifier("trueValue"), CreateIdentifier("nilValue"),
            CreateIdentifier("Function_"), CreateIdentifier("nan"),
            CreateIdentifier("infinity"), EOF
        );
    }

    [Test]
    public void OperatorsAndDelimiters_AreRecognizedCorrectly()
    {
        _lexer.LoadSource(
            "= += -= *= /= %= &= |= ^= >>= <<= ??= " +
            "+ - / * % & ~ | ^ << >> " +
            "... .. => -> <- -- ++ && || ! <==> <!=> == != > < >= <= " +
            "# $ @ ? ?? ?. ?[ ?: ?( ...? :: ; , . { } [ ] ( ) #[ " +
            "array self error in !in is !is typeof"
        );

        Expect(
            Assign, AssignAdd, AssignSubtract, AssignMultiply, AssignDivide,
            AssignModulo, AssignBitwiseAnd, AssignBitwiseOr, AssignBitwiseXor,
            AssignShiftRight, AssignShiftLeft, AssignCoalesce,
            Plus, Minus, Slash, Asterisk, Modulo, BitwiseAnd, BitwiseNot,
            BitwiseOr, BitwiseXor, ShiftLeft, ShiftRight,
            Ellipsis, Range, Associate, RightArrow, LeftArrow, Decrement, Increment,
            LogicalAnd, LogicalOr, LogicalNot, DeepEqual, DeepNotEqual,
            Equal, NotEqual, GreaterThan, LessThan, GreaterThanOrEqual,
            LessThanOrEqual,
            Length, AsNumber, AsString, QuestionMark, DoubleQuestionMark,
            Elvis, ElvisArray, SafeColon, ElvisInvocation, SafeSpread,
            DoubleColon, Semicolon, Comma, Dot, LBrace, RBrace, LBracket,
            RBracket, LParenthesis, RParenthesis, AttributeList,
            Array, Self, Error, In, NotIn, Is, IsNot, TypeOf, EOF
        );
    }

    [Test]
    public void OperatorPrefixes_UseTheLongestAvailableToken()
    {
        _lexer.LoadSource("1..4 . .. ... ...? .5 ? ?? ??= ?. ?[ ?: ?(");

        Expect(
            CreateNumber("1"), Range, CreateNumber("4"), Dot, Range, Ellipsis,
            SafeSpread, CreateNumber(".5"), QuestionMark, DoubleQuestionMark,
            AssignCoalesce, Elvis, ElvisArray, SafeColon, ElvisInvocation, EOF
        );
    }

    [Test]
    public void NotInAndIsNot_DoNotCaptureLongerIdentifiers()
    {
        _lexer.LoadSource("!in !is !in(x) !is(x) !in/*comment*/y !inside !island");

        Expect(
            NotIn, IsNot, NotIn, LParenthesis, CreateIdentifier("x"), RParenthesis,
            IsNot, LParenthesis, CreateIdentifier("x"), RParenthesis,
            NotIn, CreateIdentifier("y"), LogicalNot, CreateIdentifier("inside"),
            LogicalNot, CreateIdentifier("island"), EOF
        );
    }
}