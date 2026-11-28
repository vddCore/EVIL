namespace EVIL.Assessment.Tests.FE;

using EVIL.Core.Lexical;
using EVIL.FE.REPL.SyntaxHighlighting;
using Shouldly;

[TestFixture]
public class SyntaxHighlighterTests
{
    [Test]
    public void Render_UsesCustomTokenStyleRuleAndColor()
    {
        var highlighter = new SyntaxHighlighter();
        var customStyle = new TokenStyle("CustomKeyword");
        highlighter.TokenStyleColors[customStyle] = "\u001b[95m";
        highlighter.AddTokenStyleRule(token => token.Type == TokenType.If, customStyle);

        var rendered = highlighter.Render("if", 0, 2);

        rendered.ShouldBe("\u001b[95mif\u001b[0m");
    }

    [Test]
    public void Render_UsesUpdatedBuiltInStyleColor()
    {
        var highlighter = new SyntaxHighlighter();
        highlighter.TokenStyleColors[TokenStyle.Keyword] = "\u001b[91m";

        var rendered = highlighter.Render("if", 0, 2);

        rendered.ShouldBe("\u001b[91mif\u001b[0m");
    }
}