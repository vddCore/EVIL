namespace EVIL.Grammar.Traversal;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements;
using EVIL.Grammar.AST.Statements.TopLevel;

public abstract class AstVisitor
{
    protected Dictionary<Type, NodeHandler> Handlers { get; } = new();

    protected delegate void NodeHandler(AstNode node);

    protected AstVisitor()
    {
        var visitMethods = GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Where(m => m.Name == nameof(Visit)
                        && m.GetParameters().Length == 1
                        && typeof(AstNode).IsAssignableFrom(m.GetParameters()[0].ParameterType));

        foreach (var method in visitMethods)
        {
            var paramType = method.GetParameters()[0].ParameterType;

            Handlers[paramType] = Handler;
            continue;

            void Handler(AstNode node)
                => method.Invoke(this, [node]);
        }
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