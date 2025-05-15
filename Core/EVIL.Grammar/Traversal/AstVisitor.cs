namespace EVIL.Grammar.Traversal;

using System;
using System.Collections.Generic;

using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements;
using EVIL.Grammar.AST.Statements.TopLevel;

public abstract class AstVisitor
{
    protected Dictionary<Type, NodeHandler> Handlers { get; }

    protected delegate void NodeHandler(AstNode node);

    protected AstVisitor()
    {
        #nullable disable
        Handlers = new()
        {
            { typeof(ArgumentList), (n) => Visit((ArgumentList)n) },
            { typeof(AttributeList), (n) => Visit((AttributeList)n) },
            { typeof(ParameterList), (n) => Visit((ParameterList)n) },
            { typeof(BlockStatement), (n) => Visit((BlockStatement)n) },
            { typeof(ConditionalExpression), (n) => Visit((ConditionalExpression)n) },
            { typeof(CoalescingExpression), (n) => Visit((CoalescingExpression)n) },
            { typeof(NumberConstant), (n) => Visit((NumberConstant)n) },
            { typeof(StringConstant), (n) => Visit((StringConstant)n) },
            { typeof(NilConstant), (n) => Visit((NilConstant)n) },
            { typeof(BooleanConstant), (n) => Visit((BooleanConstant)n) },
            { typeof(TypeCodeConstant), (n) => Visit((TypeCodeConstant)n) },
            { typeof(AssignmentExpression), (n) => Visit((AssignmentExpression)n) },
            { typeof(BinaryExpression), (n) => Visit((BinaryExpression)n) },
            { typeof(UnaryExpression), (n) => Visit((UnaryExpression)n) },
            { typeof(SymbolReferenceExpression), (n) => Visit((SymbolReferenceExpression)n) },
            { typeof(ValStatement), (n) => Visit((ValStatement)n) },
            { typeof(FnIndexedStatement), (n) => Visit((FnIndexedStatement)n) },
            { typeof(FnStatement), (n) => Visit((FnStatement)n) },
            { typeof(FnTargetedStatement), (n) => Visit((FnTargetedStatement)n) },
            { typeof(InvocationExpression), (n) => Visit((InvocationExpression)n) },
            { typeof(IfStatement), (n) => Visit((IfStatement)n) },
            { typeof(ForStatement), (n) => Visit((ForStatement)n) },
            { typeof(DoWhileStatement), (n) => Visit((DoWhileStatement)n) },
            { typeof(WhileStatement), (n) => Visit((WhileStatement)n) },
            { typeof(EachStatement), (n) => Visit((EachStatement)n) },
            { typeof(RetStatement), (n) => Visit((RetStatement)n) },
            { typeof(BreakStatement), (n) => Visit((BreakStatement)n) },
            { typeof(SkipStatement), (n) => Visit((SkipStatement)n) },
            { typeof(TryStatement), (n) => Visit((TryStatement)n) },
            { typeof(ThrowStatement), (n) => Visit((ThrowStatement)n) },
            { typeof(RetryStatement), (n) => Visit((RetryStatement)n) },
            { typeof(TableExpression), (n) => Visit((TableExpression)n) },
            { typeof(ArrayExpression), (n) => Visit((ArrayExpression)n) },
            { typeof(ErrorExpression), (n) => Visit((ErrorExpression)n) },
            { typeof(SelfExpression), (n) => Visit((SelfExpression)n) },
            { typeof(SelfFnExpression), (n) => Visit((SelfFnExpression)n) },
            { typeof(SelfInvocationExpression), (n) => Visit((SelfInvocationExpression)n) },
            { typeof(IndexerExpression), (n) => Visit((IndexerExpression)n) },
            { typeof(IncrementationExpression), (n) => Visit((IncrementationExpression)n) },
            { typeof(DecrementationExpression), (n) => Visit((DecrementationExpression)n) },
            { typeof(ExpressionStatement), (n) => Visit((ExpressionStatement)n) },
            { typeof(AttributeNode), (n) => Visit((AttributeNode)n) },
            { typeof(TypeOfExpression), (n) => Visit((TypeOfExpression)n) },
            { typeof(YieldExpression), (n) => Visit((YieldExpression)n) },
            { typeof(ExpressionBodyStatement), (n) => Visit((ExpressionBodyStatement)n) },
            { typeof(ExtraArgumentsExpression), (n) => Visit((ExtraArgumentsExpression)n) },
            { typeof(FnExpression), (n) => Visit((FnExpression)n) },
            { typeof(IsExpression), (n) => Visit((IsExpression)n) },
            { typeof(ByExpression), (n) => Visit((ByExpression)n) },
            { typeof(WithExpression), (n) => Visit((WithExpression)n) }
        };
        #nullable enable
    }

    protected virtual void Visit(AstNode node)
    {
        var type = node.GetType();

        if (!Handlers.TryGetValue(type, out NodeHandler? value))
        {
            throw new Exception(
                $"Unknown node type '{type.Name}'. " +
                $"It might be that you forgor to add a handler for it."
            );
        }

        value(node);
    }

    public abstract void Visit(ProgramNode programNode);
    protected abstract void Visit(AttributeList attributeList);
    protected abstract void Visit(ParameterList parameterList);
    protected abstract void Visit(ArgumentList argumentList);
    protected abstract void Visit(BlockStatement blockStatement);
    protected abstract void Visit(ConditionalExpression conditionalExpression);
    protected abstract void Visit(CoalescingExpression coalescingExpression);
    protected abstract void Visit(NumberConstant numberConstant);
    protected abstract void Visit(StringConstant stringConstant);
    protected abstract void Visit(NilConstant nilConstant);
    protected abstract void Visit(BooleanConstant booleanConstant);
    protected abstract void Visit(TypeCodeConstant typeCodeConstant);
    protected abstract void Visit(AssignmentExpression assignmentExpression);
    protected abstract void Visit(BinaryExpression binaryExpression);
    protected abstract void Visit(UnaryExpression unaryExpression);
    protected abstract void Visit(SymbolReferenceExpression symbolReferenceExpression);
    protected abstract void Visit(ValStatement valStatement);
    protected abstract void Visit(FnIndexedStatement fnIndexedStatement);
    protected abstract void Visit(FnStatement fnStatement);
    protected abstract void Visit(FnTargetedStatement fnTargetedStatement);
    protected abstract void Visit(InvocationExpression invocationExpression);
    protected abstract void Visit(IfStatement ifStatement);
    protected abstract void Visit(ForStatement forStatement);
    protected abstract void Visit(DoWhileStatement doWhileStatement);
    protected abstract void Visit(WhileStatement whileStatement);
    protected abstract void Visit(EachStatement eachStatement);
    protected abstract void Visit(RetStatement retStatement);
    protected abstract void Visit(BreakStatement breakStatement);
    protected abstract void Visit(SkipStatement skipStatement);
    protected abstract void Visit(TryStatement tryStatement);
    protected abstract void Visit(ThrowStatement throwStatement);
    protected abstract void Visit(RetryStatement retryStatement);
    protected abstract void Visit(TableExpression tableExpression);
    protected abstract void Visit(ArrayExpression arrayExpression);
    protected abstract void Visit(ErrorExpression errorExpression);
    protected abstract void Visit(SelfExpression selfExpression);
    protected abstract void Visit(SelfFnExpression selfFnExpression);
    protected abstract void Visit(SelfInvocationExpression selfInvocationExpression);
    protected abstract void Visit(IndexerExpression indexerExpression);
    protected abstract void Visit(IncrementationExpression incrementationExpression);
    protected abstract void Visit(DecrementationExpression decrementationExpression);
    protected abstract void Visit(ExpressionStatement expressionStatement);
    protected abstract void Visit(AttributeNode attributeNode);
    protected abstract void Visit(TypeOfExpression typeOfExpression);
    protected abstract void Visit(YieldExpression yieldExpression);
    protected abstract void Visit(ExpressionBodyStatement expressionBodyStatement);
    protected abstract void Visit(ExtraArgumentsExpression extraArgumentsExpression);
    protected abstract void Visit(FnExpression fnExpression);
    protected abstract void Visit(IsExpression isExpression);
    protected abstract void Visit(ByExpression byExpression);
    protected abstract void Visit(WithExpression withExpression);
}