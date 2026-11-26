namespace EVIL.Assessment.Tests.Core.Grammar;

using System.Linq;
using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar;
using EVIL.Core.Grammar.AST.Constants;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Grammar.AST.Statements;
using EVIL.Core.Grammar.StringInterpolation;
using NUnit.Framework;
using Shouldly;

[TestFixture]
public class InterpolatedStringParserTests : ParserTestBase
{
    [Test]
    public void InterpolatedString_StoresOrderedLiteralAndExpressionSegments()
    {
        const string source = "val text = $\"Start $name ${count + 1}!\";";
        var stringConstant = GetStringConstant(source);

        stringConstant.Segments.Count.ShouldBe(5);
        stringConstant.Segments[0].Kind.ShouldBe(InterpolatedSegmentKind.Literal);
        stringConstant.Segments[0].Text.ShouldBe("Start ");
        stringConstant.Segments[1].Kind.ShouldBe(InterpolatedSegmentKind.Expression);
        stringConstant.Segments[1].Expression.ShouldBeOfType<SymbolReferenceExpression>()
            .Identifier.ShouldBe("name");
        stringConstant.Segments[2].Kind.ShouldBe(InterpolatedSegmentKind.Literal);
        stringConstant.Segments[2].Text.ShouldBe(" ");

        var expression = stringConstant.Segments[3].Expression.ShouldBeOfType<BinaryExpression>();
        expression.Left.ShouldBeOfType<SymbolReferenceExpression>().Identifier.ShouldBe("count");
        expression.Right.ShouldBeOfType<NumberConstant>().Value.ShouldBe(1);
        stringConstant.Segments[4].Kind.ShouldBe(InterpolatedSegmentKind.Literal);
        stringConstant.Segments[4].Text.ShouldBe("!");

        stringConstant.Segments[1].Expression!.Parent.ShouldBe(stringConstant);
        expression.Parent.ShouldBe(stringConstant);
        stringConstant.Segments[1].Expression!.Column.ShouldBe(source.IndexOf("name") + 1);
        expression.Left.Column.ShouldBe(source.IndexOf("count") + 1);
        expression.Column.ShouldBe(source.IndexOf("+") + 1);
    }

    [Test]
    public void InterpolatedString_ParsesNestedBracesAndQuotedClosingBrace()
    {
        const string source = "val text = $\"${{key: \"}\"}}\";";
        var stringConstant = GetStringConstant(source);

        stringConstant.Segments.Count.ShouldBe(1);
        var table = stringConstant.Segments[0].Expression.ShouldBeOfType<TableExpression>();
        table.Initializers.Count.ShouldBe(1);
        stringConstant.Segments[0].Expression!.Parent.ShouldBe(stringConstant);
    }

    [Test]
    public void InterpolatedString_ReportsEmbeddedSyntaxErrorAtOriginalSourceLocation()
    {
        const string source = "val text = $\"line\n${1 + * 2}\";";
        var diagnostics = new DiagnosticBag();
        var parser = new Parser(diagnostics, "interpolation.vil");

        _ = parser.Parse(source);

        diagnostics.ErrorCount.ShouldBe(1);
        var diagnostic = diagnostics.Diagnostics.Single();
        diagnostic.Location.FilePath.ShouldBe("interpolation.vil");
        diagnostic.Location.Line.ShouldBe(2);
        diagnostic.Location.Column.ShouldBe(7);
        diagnostic.Message.ShouldContain("Unexpected token");
    }

    private StringConstant GetStringConstant(string source)
    {
        var program = BuildProgramTree(source);
        var statement = program.Statements[0].ShouldBeOfType<ValStatement>();
        var initializer = statement.Definitions.Values.Single();
        if (initializer is UnaryExpression unaryExpression)
        {
            initializer = unaryExpression.Right;
        }

        return initializer.ShouldBeOfType<StringConstant>();
    }
}