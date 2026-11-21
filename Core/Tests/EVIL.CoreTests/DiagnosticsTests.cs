namespace EVIL.CoreTests;

using EVIL.CommonTypes.Diagnostics;
using EVIL.Grammar;
using EVIL.Grammar.AST.Diagnostics;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;
using NUnit.Framework;
using Shouldly;

public class DiagnosticsTests
{
    [Test]
    public void SourceLocation_Formatting_WithAllFields()
    {
        var loc = new SourceLocation("test.vil", 10, 5, 10, 15, 100, 10);

        loc.FilePath.ShouldBe("test.vil");
        loc.Line.ShouldBe(10);
        loc.Column.ShouldBe(5);
        loc.EndLine.ShouldBe(10);
        loc.EndColumn.ShouldBe(15);
        loc.Offset.ShouldBe(100);
        loc.Length.ShouldBe(10);
        loc.IsSpan.ShouldBeTrue();
        loc.HasLocation.ShouldBeTrue();
        loc.ToString().ShouldBe("test.vil(10,5)");
    }

    [Test]
    public void SourceLocation_Formatting_WithoutFile()
    {
        var loc = new SourceLocation(null, 42, 7);
        loc.HasLocation.ShouldBeTrue();
        loc.IsSpan.ShouldBeFalse();
        loc.ToString().ShouldBe("(42,7)");
    }

    [Test]
    public void SourceLocation_None()
    {
        SourceLocation.None.HasLocation.ShouldBeFalse();
        SourceLocation.None.ToString().ShouldBe(string.Empty);
    }

    [Test]
    public void DiagnosticDescriptor_CreatesDiagnosticWithFormatting()
    {
        var descriptor = CompilerDiagnostics.DuplicateSymbolInScope;
        var loc = new SourceLocation("main.vil", 12, 4);
        var diag = descriptor.Create(loc, "myVar", "local variable", 5, 2);

        diag.Id.ShouldBe("EV2001");
        diag.Severity.ShouldBe(DiagnosticSeverity.Error);
        diag.Location.ShouldBe(loc);
        diag.Message.ShouldBe("The symbol 'myVar' already exists in this scope and is a local variable (previously defined on line 5, column 2).");
        diag.ToString().ShouldBe("main.vil(12,4): error EV2001: The symbol 'myVar' already exists in this scope and is a local variable (previously defined on line 5, column 2).");
    }

    [Test]
    public void Diagnostic_WithRelatedLocations()
    {
        var descriptor = CompilerDiagnostics.DuplicateSymbolInScope;
        var primaryLoc = new SourceLocation("main.vil", 15, 9);
        var originalLoc = new SourceLocation("main.vil", 10, 9);

        var diag = new Diagnostic(
            descriptor,
            primaryLoc,
            DiagnosticSeverity.Error,
            "Duplicate symbol 'foo'",
            null,
            [new RelatedLocation(originalLoc, "Previously declared here")]
        );

        var output = diag.ToString();
        output.ShouldContain("main.vil(15,9): error EV2001: Duplicate symbol 'foo'");
        output.ShouldContain("-> main.vil(10,9): Previously declared here");
    }

    [Test]
    public void DiagnosticBag_CollectsAndCategorizes()
    {
        var bag = new DiagnosticBag();
        bag.HasErrors.ShouldBeFalse();
        bag.HasAnyMessages.ShouldBeFalse();

        bag.Report(CompilerDiagnostics.UnterminatedString.Create(new SourceLocation("a.vil", 1, 1)));
        bag.Report(CompilerDiagnostics.NoDefaultByArm.Create(new SourceLocation("a.vil", 5, 2)));
        bag.Report(CompilerDiagnostics.InternalCompilerError.Create(new SourceLocation("a.vil", 10, 1), "Crash"));

        bag.Count.ShouldBe(3);
        bag.ErrorCount.ShouldBe(2); // Error + InternalError
        bag.WarningCount.ShouldBe(1);
        bag.FatalCount.ShouldBe(1); // InternalError counts as fatal
        bag.HasErrors.ShouldBeTrue();
        bag.HasAnyMessages.ShouldBeTrue();

        bag.Clear();
        bag.Count.ShouldBe(0);
        bag.HasErrors.ShouldBeFalse();
    }

    [Test]
    public void LexerException_PreservesLocationStructure()
    {
        var loc = new SourceLocation("script.vil", 7, 14);
        var le = new LexerException("Invalid token", loc);

        le.Line.ShouldBe(7);
        le.Column.ShouldBe(14);
        le.Location.FilePath.ShouldBe("script.vil");
        le.Location.Line.ShouldBe(7);
        le.Location.Column.ShouldBe(14);
    }

    [Test]
    public void ParserException_PreservesLocationStructure()
    {
        var loc = new SourceLocation("script.vil", 20, 8);
        var pe = new ParserException("Syntax error", loc);

        pe.Line.ShouldBe(20);
        pe.Column.ShouldBe(8);
        pe.Location.FilePath.ShouldBe("script.vil");
        pe.Location.Line.ShouldBe(20);
        pe.Location.Column.ShouldBe(8);
    }

    [Test]
    public void Lexer_WithDiagnosticSink_RecoversFromMultipleErrors()
    {
        var bag = new DiagnosticBag();
        var lexer = new Lexer(bag, "test.vil");
        lexer.LoadSource("val a = ` + 10; val b = ` + 20;");
        while (lexer.State.CurrentToken.Type != TokenType.EOF)
        {
            lexer.NextToken();
        }

        bag.HasErrors.ShouldBeTrue();
        bag.ErrorCount.ShouldBe(2);
    }

    [Test]
    public void Parser_WithDiagnosticSink_RecoversAndProducesErrorStatementNodes()
    {
        var bag = new DiagnosticBag();
        var parser = new Parser(bag, "multi_err.vil");

        var source = "val = 1;\nval b = 2;\nval = 3;\nval c = 4;";
        var program = parser.Parse(source);

        bag.HasErrors.ShouldBeTrue();
        bag.ErrorCount.ShouldBe(2);

        program.Statements.Count.ShouldBe(4);
        program.Statements[0].ShouldBeOfType<DiagnosticStatement>();
        program.Statements[1].ShouldBeOfType<ValStatement>();
        program.Statements[2].ShouldBeOfType<DiagnosticStatement>();
        program.Statements[3].ShouldBeOfType<ValStatement>();
    }

    [Test]
    public void Parser_BlockStatement_RecoversFromSyntaxError()
    {
        var bag = new DiagnosticBag();
        var parser = new Parser(bag, "block_err.vil");

        var source = "fn test() {\nval = 1;\nval valid = 10;\n}";
        var program = parser.Parse(source);

        bag.HasErrors.ShouldBeTrue();
        program.Statements.Count.ShouldBe(1);
        var fn = (FnStatement)program.Statements[0];
        var block = (BlockStatement)fn.Statement;
        block.Statements.Count.ShouldBe(2);
        block.Statements[0].ShouldBeOfType<DiagnosticStatement>();
        block.Statements[1].ShouldBeOfType<ValStatement>();
    }

    [Test]
    public void Parser_WithoutDiagnosticSink_ThrowsParserException()
    {
        var parser = new Parser();
        Should.Throw<ParserException>(() => parser.Parse("val = 1;"));
    }

    [Test]
    public void Parser_RespectsMaxErrors()
    {
        var bag = new DiagnosticBag();
        var parser = new Parser(bag, "max_err.vil") { MaxErrors = 3 };

        var source = "val = 1;\nval = 2;\nval = 3;\nval = 4;\nval = 5;\nval = 6;";
        var program = parser.Parse(source);

        bag.ErrorCount.ShouldBe(3);
    }
}
