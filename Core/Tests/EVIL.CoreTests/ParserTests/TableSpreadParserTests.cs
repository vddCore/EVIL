namespace EVIL.CoreTests.ParserTests;

using System.Linq;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Statements;
using NUnit.Framework;
using Shouldly;

[TestFixture]
public class TableSpreadParserTests : ParserTestBase
{
    [Test]
    public void SafeSpread_ParsesNilTolerantTableSpreadExpression()
    {
        var program = BuildProgramTree("val t = { ...?opts };");
        var stmt = program.Statements[0].ShouldBeOfType<ValStatement>();
        var table = stmt.Definitions.Values.First()!.ShouldBeOfType<TableExpression>();

        table.Keyed.ShouldBeTrue();
        table.Initializers.Count.ShouldBe(1);

        var spread = table.Initializers[0].ShouldBeOfType<TableSpreadExpression>();
        spread.IsNilTolerant.ShouldBeTrue();
        spread.Target.ShouldBeOfType<SymbolReferenceExpression>().Identifier.ShouldBe("opts");
    }

    [Test]
    public void StrictSpread_ParsesStrictTableSpreadExpression()
    {
        var program = BuildProgramTree("val t = { ...opts };");
        var stmt = program.Statements[0].ShouldBeOfType<ValStatement>();
        var table = stmt.Definitions.Values.First()!.ShouldBeOfType<TableExpression>();

        table.Keyed.ShouldBeTrue();
        table.Initializers.Count.ShouldBe(1);

        var spread = table.Initializers[0].ShouldBeOfType<TableSpreadExpression>();
        spread.IsNilTolerant.ShouldBeFalse();
        spread.Target.ShouldBeOfType<SymbolReferenceExpression>().Identifier.ShouldBe("opts");
    }

    [Test]
    public void MixedSpreadAndProperties_ParsesCorrectly()
    {
        var program = BuildProgramTree("val t = { a: 1, ...?defaults, b: 2 };");
        var stmt = program.Statements[0].ShouldBeOfType<ValStatement>();
        var table = stmt.Definitions.Values.First()!.ShouldBeOfType<TableExpression>();

        table.Keyed.ShouldBeTrue();
        table.Initializers.Count.ShouldBe(3);

        table.Initializers[0].ShouldBeOfType<KeyValuePairExpression>();
        var spread = table.Initializers[1].ShouldBeOfType<TableSpreadExpression>();
        spread.IsNilTolerant.ShouldBeTrue();
        spread.Target.ShouldBeOfType<SymbolReferenceExpression>().Identifier.ShouldBe("defaults");
        table.Initializers[2].ShouldBeOfType<KeyValuePairExpression>();
    }

    [Test]
    public void SpreadInWithExpression_ParsesCorrectly()
    {
        var program = BuildProgramTree("val t = base with { ...?extra, active: true };");
        var stmt = program.Statements[0].ShouldBeOfType<ValStatement>();
        var withExpr = stmt.Definitions.Values.First()!.ShouldBeOfType<WithExpression>();

        withExpr.TableExpansionExpression.Keyed.ShouldBeTrue();
        withExpr.TableExpansionExpression.Initializers.Count.ShouldBe(2);

        var spread = withExpr.TableExpansionExpression.Initializers[0].ShouldBeOfType<TableSpreadExpression>();
        spread.IsNilTolerant.ShouldBeTrue();
        spread.Target.ShouldBeOfType<SymbolReferenceExpression>().Identifier.ShouldBe("extra");
        withExpr.TableExpansionExpression.Initializers[1].ShouldBeOfType<KeyValuePairExpression>();
    }
}
