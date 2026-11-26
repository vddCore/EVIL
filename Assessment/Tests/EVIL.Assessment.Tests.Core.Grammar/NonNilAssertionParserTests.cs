namespace EVIL.Assessment.Tests.Core.Grammar;

using System.Linq;
using EVIL.Assessment.Tests.Core.Grammar.ParserTests;
using EVIL.Core.Grammar;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Grammar.AST.Statements;
using NUnit.Framework;
using Shouldly;

[TestFixture]
public class NonNilAssertionParserTests : ParserTestBase
{
    [Test]
    public void SimpleNonNilAssertion_ProducesNonNilAssertionExpression()
    {
        var program = BuildProgramTree("x!;");
        var stmt = program.Statements[0].ShouldBeOfType<ExpressionStatement>();
        var expr = stmt.Expression.ShouldBeOfType<NonNilAssertionExpression>();

        var sym = expr.Operand.ShouldBeOfType<SymbolReferenceExpression>();
        sym.Identifier.ShouldBe("x");
    }

    [Test]
    public void ChainedNonNilAssertion_ProducesNestedNonNilAssertion()
    {
        var program = BuildProgramTree("x!!;");
        var stmt = program.Statements[0].ShouldBeOfType<ExpressionStatement>();
        var outer = stmt.Expression.ShouldBeOfType<NonNilAssertionExpression>();
        var inner = outer.Operand.ShouldBeOfType<NonNilAssertionExpression>();

        inner.Operand.ShouldBeOfType<SymbolReferenceExpression>().Identifier.ShouldBe("x");
    }

    [Test]
    public void MemberAccessAfterNonNilAssertion_IndexesNonNilAssertion()
    {
        var program = BuildProgramTree("user!.name;");
        var stmt = program.Statements[0].ShouldBeOfType<ExpressionStatement>();
        var indexer = stmt.Expression.ShouldBeOfType<IndexerExpression>();

        indexer.Indexable.ShouldBeOfType<NonNilAssertionExpression>().Operand.ShouldBeOfType<SymbolReferenceExpression>().Identifier.ShouldBe("user");
        indexer.GetKeyStringRepresentation().ShouldBe("name");
    }

    [Test]
    public void InvocationResultNonNilAssertion_ProducesNonNilAssertion()
    {
        var program = BuildProgramTree("getUser()!;");
        var stmt = program.Statements[0].ShouldBeOfType<ExpressionStatement>();
        var expr = stmt.Expression.ShouldBeOfType<NonNilAssertionExpression>();

        expr.Operand.ShouldBeOfType<InvocationExpression>();
    }

    [Test]
    public void LogicalNotCombinedWithNonNilAssertion_ParsesBoth()
    {
        var program = BuildProgramTree("!x!;");
        var stmt = program.Statements[0].ShouldBeOfType<ExpressionStatement>();
        var unary = stmt.Expression.ShouldBeOfType<UnaryExpression>();
        unary.Type.ShouldBe(UnaryOperationType.LogicalNot);

        var inner = unary.Right.ShouldBeOfType<NonNilAssertionExpression>();
        inner.Operand.ShouldBeOfType<SymbolReferenceExpression>()
             .Identifier.ShouldBe("x");
    }

    [Test]
    public void NonNilAssertionInBinaryExpression()
    {
        var program = BuildProgramTree("val y = x! + 1;");
        var stmt = program.Statements[0].ShouldBeOfType<ValStatement>();
        var binary = stmt.Definitions.Values.First()!.ShouldBeOfType<BinaryExpression>();

        binary.Left.ShouldBeOfType<NonNilAssertionExpression>()
              .Operand.ShouldBeOfType<SymbolReferenceExpression>()
              .Identifier.ShouldBe("x");
    }
}
