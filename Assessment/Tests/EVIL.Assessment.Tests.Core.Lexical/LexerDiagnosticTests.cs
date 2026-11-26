namespace EVIL.Assessment.Tests.Core.Lexical;

using EVIL.Core.Diagnostics;
using EVIL.Core.Lexical;
using Shouldly;

public class LexerDiagnosticTests : LexerTestBase
{
    [Test]
    public void UnexpectedCharacter_WithoutDiagnosticSink_ThrowsLexerException()
    {
        var exception = Assert.Throws<LexerException>(() => _lexer.LoadSource("\u00A7"));

        exception.Message.ShouldBe("Unexpected token '\u00A7'");
        exception.Line.ShouldBe(1);
        exception.Column.ShouldBe(1);
        exception.Location.ShouldBe(new SourceLocation(null, 1, 1));
    }

    [Test]
    public void UnexpectedCharacter_IsReportedToDiagnosticSink()
    {
        var diagnostics = new DiagnosticBag();
        _lexer = new Lexer(diagnostics, "input.evil");

        _lexer.LoadSource("\u00A7");

        diagnostics.Count.ShouldBe(1);
        diagnostics.HasErrors.ShouldBeTrue();

        var diagnostic = diagnostics.Diagnostics[0];
        diagnostic.Id.ShouldBe("EV0102");
        diagnostic.Message.ShouldBe("Character '\u00A7' was unexpected at this time.");
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
        diagnostic.Location.ShouldBe(new SourceLocation("input.evil", 1, 1));
    }
}