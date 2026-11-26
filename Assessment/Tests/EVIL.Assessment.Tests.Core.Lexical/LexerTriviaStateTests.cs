namespace EVIL.Assessment.Tests.Core.Lexical;

using Shouldly;
using static EVIL.Core.Lexical.Token;

public class LexerTriviaStateTests : LexerTestBase
{
    [Test]
    public void WhitespaceAndComments_AreSkipped()
    {
        _lexer.LoadSource(" \t// leading comment\nalpha/* block\n comment */beta// trailing comment");

        Expect(CreateIdentifier("alpha"), CreateIdentifier("beta"), EOF);
    }

    [Test]
    public void ByteOrderMarkAndWhitespace_AreSkipped()
    {
        _lexer.LoadSource("\uFEFF \t alpha");

        ExpectLocated(CreateIdentifier("alpha"), 1, 5);
        Expect(EOF);
    }

    [Test]
    public void TokenCoordinates_TrackLinesAndIndentation()
    {
        _lexer.LoadSource("first\n  second\nthird");

        ExpectLocated(CreateIdentifier("first"), 1, 1);
        ExpectLocated(CreateIdentifier("second"), 2, 3);
        ExpectLocated(CreateIdentifier("third"), 3, 1);
        Expect(EOF);
    }

    [Test]
    public void PreviousToken_Tracks_TheLastConsumedToken()
    {
        _lexer.LoadSource("first second");

        _lexer.State.PreviousToken.ShouldBe(Empty);
        _lexer.State.CurrentToken.ShouldBe(CreateIdentifier("first"));

        _lexer.NextToken();

        _lexer.State.PreviousToken.ShouldBe(CreateIdentifier("first"));
        _lexer.State.CurrentToken.ShouldBe(CreateIdentifier("second"));

        _lexer.NextToken();

        _lexer.State.PreviousToken.ShouldBe(CreateIdentifier("second"));
        _lexer.State.CurrentToken.ShouldBe(EOF);
    }

    [Test]
    public void PeekToken_ReturnsLookahead_WithoutChangingState()
    {
        _lexer.LoadSource("first second third");
        var stateBeforePeek = _lexer.State with { };

        _lexer.PeekToken(1).ShouldBe(CreateIdentifier("second"));
        _lexer.State.ShouldBe(stateBeforePeek);

        _lexer.PeekToken(2).ShouldBe(CreateIdentifier("third"));
        _lexer.State.ShouldBe(stateBeforePeek);

        Expect(CreateIdentifier("first"), CreateIdentifier("second"), CreateIdentifier("third"), EOF);
    }
}