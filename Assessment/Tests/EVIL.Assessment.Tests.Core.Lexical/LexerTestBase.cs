namespace EVIL.Assessment.Tests.Core.Lexical;

using EVIL.Core.Lexical;
using Shouldly;
using static EVIL.Core.Lexical.Token;

public abstract class LexerTestBase
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

    protected void Expect(params Token[] tokens)
    {
        foreach (var token in tokens)
        {
            CurrentToken.ShouldBe(token);
            _lexer.NextToken();
        }
    }

    protected void ExpectLocated(Token token, int line, int column)
    {
        CurrentToken.ShouldBe(token);
        CurrentToken.Line.ShouldBe(line);
        CurrentToken.Column.ShouldBe(column);
        _lexer.NextToken();
    }
}