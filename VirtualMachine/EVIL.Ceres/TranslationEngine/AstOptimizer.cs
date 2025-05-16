namespace EVIL.Ceres.TranslationEngine;

using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements;
using EVIL.Grammar.AST.Statements.TopLevel;
using EVIL.Grammar.Traversal;

public class AstOptimizer : AstVisitor
{
    public override void Visit(ProgramNode programNode)
    {
        foreach (var node in programNode.Statements)
            Visit(node);
    }

    protected override void Visit(AttributeList attributeList)
    {
    }

    protected override void Visit(ParameterList parameterList)
    {
    }

    protected override void Visit(ArgumentList argumentList)
    {
    }

    protected override void Visit(BlockStatement blockStatement)
    {
        foreach (var statement in blockStatement.Statements)
            Visit(statement);
    }

    protected override void Visit(ConditionalExpression conditionalExpression)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(CoalescingExpression coalescingExpression)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(NumberConstant numberConstant)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(StringConstant stringConstant)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(NilConstant nilConstant)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(BooleanConstant booleanConstant)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(TypeCodeConstant typeCodeConstant)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(AssignmentExpression assignmentExpression)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(BinaryExpression binaryExpression)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(UnaryExpression unaryExpression)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(SymbolReferenceExpression symbolReferenceExpression)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(ValStatement valStatement)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(FnIndexedStatement fnIndexedStatement)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(FnStatement fnStatement)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(FnTargetedStatement fnTargetedStatement)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(InvocationExpression invocationExpression)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(IfStatement ifStatement)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(ForStatement forStatement)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(DoWhileStatement doWhileStatement)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(WhileStatement whileStatement)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(EachStatement eachStatement)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(RetStatement retStatement)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(BreakStatement breakStatement)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(SkipStatement skipStatement)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(TryStatement tryStatement)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(ThrowStatement throwStatement)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(RetryStatement retryStatement)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(TableExpression tableExpression)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(ArrayExpression arrayExpression)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(ErrorExpression errorExpression)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(SelfExpression selfExpression)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(SelfFnExpression selfFnExpression)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(SelfInvocationExpression selfInvocationExpression)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(IndexerExpression indexerExpression)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(IncrementationExpression incrementationExpression)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(DecrementationExpression decrementationExpression)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(ExpressionStatement expressionStatement)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(AttributeNode attributeNode)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(TypeOfExpression typeOfExpression)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(YieldExpression yieldExpression)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(ExpressionBodyStatement expressionBodyStatement)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(ExtraArgumentsExpression extraArgumentsExpression)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(FnExpression fnExpression)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(IsExpression isExpression)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(ByExpression byExpression)
    {
        throw new System.NotImplementedException();
    }

    protected override void Visit(WithExpression withExpression)
    {
        throw new System.NotImplementedException();
    }
}