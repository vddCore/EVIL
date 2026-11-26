namespace EVIL.Assessment.Tests.Core.Lexical;

public class LexerInitializationTests : LexerTestBase
{
    [Test]
    public void LoadSource_Rejects_NullSource()
    {
        Assert.Throws<ArgumentNullException>(
            () => _lexer.LoadSource(null!)
        );
    }

    [Test]
    public void NextToken_BeforeSourceIsLoaded_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => _lexer.NextToken());
    }

    [Test]
    public void PeekToken_BeforeSourceIsLoaded_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => _lexer.PeekToken(1));
    }
}