namespace EVIL.Assessment.Tests.Core.Grammar;

using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Grammar.AST.Statements;
using NUnit.Framework;
using Shouldly;

[TestFixture]
public class SafeInvocationParserTests : ParserTestBase
{
    [Test]
    public void SafeMethodInvocation_ProducesConditionalSelfInvocationExpression()
    {
        var program = BuildProgramTree("obj?:method(1, 2);");
        var stmt = program.Statements[0].ShouldBeOfType<ExpressionStatement>();
        var expr = stmt.Expression.ShouldBeOfType<SelfInvocationExpression>();

        expr.IsConditional.ShouldBeTrue();
        expr.Indexable.ShouldBeOfType<SymbolReferenceExpression>().Identifier.ShouldBe("obj");
        expr.Identifier.Name.ShouldBe("method");
        expr.ArgumentList.Arguments.Count.ShouldBe(2);
    }

    [Test]
    public void NormalMethodInvocation_ProducesNonConditionalSelfInvocationExpression()
    {
        var program = BuildProgramTree("obj:method(1, 2);");
        var stmt = program.Statements[0].ShouldBeOfType<ExpressionStatement>();
        var expr = stmt.Expression.ShouldBeOfType<SelfInvocationExpression>();

        expr.IsConditional.ShouldBeFalse();
        expr.Indexable.ShouldBeOfType<SymbolReferenceExpression>().Identifier.ShouldBe("obj");
        expr.Identifier.Name.ShouldBe("method");
        expr.ArgumentList.Arguments.Count.ShouldBe(2);
    }

    [Test]
    public void SafeFunctionInvocation_ProducesConditionalInvocationExpression()
    {
        var program = BuildProgramTree("calc?(10, 20);");
        var stmt = program.Statements[0].ShouldBeOfType<ExpressionStatement>();
        var expr = stmt.Expression.ShouldBeOfType<InvocationExpression>();

        expr.IsConditional.ShouldBeTrue();
        expr.Callee.ShouldBeOfType<SymbolReferenceExpression>().Identifier.ShouldBe("calc");
        expr.ArgumentList.Arguments.Count.ShouldBe(2);
    }

    [Test]
    public void NormalFunctionInvocation_ProducesNonConditionalInvocationExpression()
    {
        var program = BuildProgramTree("calc(10, 20);");
        var stmt = program.Statements[0].ShouldBeOfType<ExpressionStatement>();
        var expr = stmt.Expression.ShouldBeOfType<InvocationExpression>();

        expr.IsConditional.ShouldBeFalse();
        expr.Callee.ShouldBeOfType<SymbolReferenceExpression>().Identifier.ShouldBe("calc");
        expr.ArgumentList.Arguments.Count.ShouldBe(2);
    }

    [Test]
    public void ChainedSafeInvocations_MaintainsCorrectAST()
    {
        var program = BuildProgramTree("factory?:create()?(arg);");
        var stmt = program.Statements[0].ShouldBeOfType<ExpressionStatement>();
        
        var outerInv = stmt.Expression.ShouldBeOfType<InvocationExpression>();
        outerInv.IsConditional.ShouldBeTrue();
        outerInv.ArgumentList.Arguments.Count.ShouldBe(1);

        var innerSelfInv = outerInv.Callee.ShouldBeOfType<SelfInvocationExpression>();
        innerSelfInv.IsConditional.ShouldBeTrue();
        innerSelfInv.Identifier.Name.ShouldBe("create");
        innerSelfInv.Indexable.ShouldBeOfType<SymbolReferenceExpression>().Identifier.ShouldBe("factory");
    }
}
