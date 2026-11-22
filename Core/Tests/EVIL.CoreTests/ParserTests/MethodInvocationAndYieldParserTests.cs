namespace EVIL.CoreTests.ParserTests;

using System.Linq;
using EVIL.Grammar;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements;
using NUnit.Framework;
using Shouldly;

[TestFixture]
public class MethodInvocationAndYieldParserTests : ParserTestBase
{
    [Test]
    public void MethodInvocation_ProducesSelfInvocationExpression()
    {
        var program = BuildProgramTree("obj:method(1, 2);");
        var stmt = program.Statements[0].ShouldBeOfType<ExpressionStatement>();
        var expr = stmt.Expression.ShouldBeOfType<SelfInvocationExpression>();
        expr.Indexable.ShouldBeOfType<SymbolReferenceExpression>().Identifier.ShouldBe("obj");
        expr.Identifier.Name.ShouldBe("method");
        expr.ArgumentList.Arguments.Count.ShouldBe(2);
    }

    [Test]
    public void ChainedMethodInvocation_ProducesNestedSelfInvocationExpression()
    {
        var program = BuildProgramTree("obj:first():second();");
        var stmt = program.Statements[0].ShouldBeOfType<ExpressionStatement>();
        var outer = stmt.Expression.ShouldBeOfType<SelfInvocationExpression>();
        outer.Identifier.Name.ShouldBe("second");
        var inner = outer.Indexable.ShouldBeOfType<SelfInvocationExpression>();
        inner.Identifier.Name.ShouldBe("first");
    }

    [Test]
    public void MethodInvocation_InsideTernaryTrueBranch()
    {
        var program = BuildProgramTree("val x = cond ? obj:method() : fallback;");
        var valStmt = program.Statements[0].ShouldBeOfType<ValStatement>();
        var valExpr = valStmt.Definitions.Values.First()!;
        var cond = valExpr.ShouldBeOfType<ConditionalExpression>();
        var trueBranch = cond.TrueExpression.ShouldBeOfType<SelfInvocationExpression>();
        trueBranch.Identifier.Name.ShouldBe("method");
        cond.FalseExpression.ShouldBeOfType<SymbolReferenceExpression>().Identifier.ShouldBe("fallback");
    }

    [Test]
    public void Ternary_WithFalseBranchStartingWithInvocation_DoesNotConfuseSelfInvocation()
    {
        var program = BuildProgramTree("val x = cond ? obj : fallback();");
        var valStmt = program.Statements[0].ShouldBeOfType<ValStatement>();
        var valExpr = valStmt.Definitions.Values.First()!;
        var cond = valExpr.ShouldBeOfType<ConditionalExpression>();
        cond.TrueExpression.ShouldBeOfType<SymbolReferenceExpression>().Identifier.ShouldBe("obj");
        var falseBranch = cond.FalseExpression.ShouldBeOfType<InvocationExpression>();
        falseBranch.Callee.ShouldBeOfType<SymbolReferenceExpression>().Identifier.ShouldBe("fallback");
    }

    [Test]
    public void TargetedFnDefinition_KeepsDoubleColonIntact()
    {
        var program = BuildProgramTree("loc fn Target::action(x) { ret x; }");
        var fnTargeted = program.Statements[0].ShouldBeOfType<FnTargetedStatement>();
        fnTargeted.PrimaryTarget.ShouldBeOfType<IdentifierNode>().Name.ShouldBe("Target");
        fnTargeted.SecondaryIdentifier.Name.ShouldBe("action");
    }

    [Test]
    public void SelfFnConstruct_KeepsDoubleColonIntact()
    {
        var program = BuildProgramTree("val t = { act: self::fn(x) -> x + 1 };");
        var valStmt = program.Statements[0].ShouldBeOfType<ValStatement>();
        var table = valStmt.Definitions.Values.First()!.ShouldBeOfType<TableExpression>();
        var kvp = table.Initializers[0].ShouldBeOfType<KeyValuePairExpression>();
        kvp.ValueNode.ShouldBeOfType<SelfFnExpression>();
    }

    [Test]
    public void OldDoubleColonMethodInvocation_ThrowsParserException()
    {
        Should.Throw<ParserException>(() => BuildProgramTree("obj::method();"));
    }

    [Test]
    public void Yield_WithFunctionCall_ProducesYieldExpression()
    {
        var program = BuildProgramTree("yield delay(100);");
        var stmt = program.Statements[0].ShouldBeOfType<ExpressionStatement>();
        var yieldExpr = stmt.Expression.ShouldBeOfType<YieldExpression>();
        var inv = yieldExpr.Invocation.ShouldBeOfType<InvocationExpression>();
        inv.Callee.ShouldBeOfType<SymbolReferenceExpression>().Identifier.ShouldBe("delay");
        inv.ArgumentList.Arguments.Count.ShouldBe(1);
    }

    [Test]
    public void Yield_WithMethodCall_ProducesYieldExpression()
    {
        var program = BuildProgramTree("yield worker:run(1, 2);");
        var stmt = program.Statements[0].ShouldBeOfType<ExpressionStatement>();
        var yieldExpr = stmt.Expression.ShouldBeOfType<YieldExpression>();
        var selfInv = yieldExpr.Invocation.ShouldBeOfType<SelfInvocationExpression>();
        selfInv.Indexable.ShouldBeOfType<SymbolReferenceExpression>().Identifier.ShouldBe("worker");
        selfInv.Identifier.Name.ShouldBe("run");
        selfInv.ArgumentList.Arguments.Count.ShouldBe(2);
    }

    [Test]
    public void Yield_WithMemberCall_ProducesYieldExpression()
    {
        var program = BuildProgramTree("yield obj.method();");
        var stmt = program.Statements[0].ShouldBeOfType<ExpressionStatement>();
        var yieldExpr = stmt.Expression.ShouldBeOfType<YieldExpression>();
        var inv = yieldExpr.Invocation.ShouldBeOfType<InvocationExpression>();
        inv.Callee.ShouldBeOfType<IndexerExpression>();
    }

    [Test]
    public void Yield_WithIndexerCall_ProducesYieldExpression()
    {
        var program = BuildProgramTree("yield funcs[0]();");
        var stmt = program.Statements[0].ShouldBeOfType<ExpressionStatement>();
        var yieldExpr = stmt.Expression.ShouldBeOfType<YieldExpression>();
        var inv = yieldExpr.Invocation.ShouldBeOfType<InvocationExpression>();
        inv.Callee.ShouldBeOfType<IndexerExpression>();
    }

    [Test]
    public void Yield_WithParenthesizedCall_ProducesYieldExpression()
    {
        var program = BuildProgramTree("yield (get_fn())(42);");
        var stmt = program.Statements[0].ShouldBeOfType<ExpressionStatement>();
        var yieldExpr = stmt.Expression.ShouldBeOfType<YieldExpression>();
        var inv = yieldExpr.Invocation.ShouldBeOfType<InvocationExpression>();
        inv.ArgumentList.Arguments.Count.ShouldBe(1);
    }

    [Test]
    public void Yield_OldAngleBracketSyntax_ThrowsParserException()
    {
        Should.Throw<ParserException>(() => BuildProgramTree("yield<worker>();"));
    }

    [Test]
    public void Yield_WithoutInvocation_ThrowsParserException()
    {
        Should.Throw<ParserException>(() => BuildProgramTree("yield 42;"));
    }
}
