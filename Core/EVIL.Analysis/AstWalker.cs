namespace EVIL.Analysis;

using System;
using EVIL.Grammar;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Diagnostics;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements;
using EVIL.Grammar.StringInterpolation;

public class AstWalker : AstVisitor
{
    public Action<AstNode>? NodeVisiting { get; set; }

    public void Walk(AstNode node)
    {
        Visit(node);
    }

    protected override void Visit(AstNode node)
    {
        NodeVisiting?.Invoke(node);
        base.Visit(node);
    }

    public override void Visit(ProgramNode programNode)
    {
        foreach (var stmt in programNode.Statements)
        {
            Visit(stmt);
        }
    }

    protected override void Visit(BlockStatement blockStatement)
    {
        foreach (var stmt in blockStatement.Statements)
        {
            Visit(stmt);
        }
    }

    protected override void Visit(ExpressionStatement expressionStatement)
    {
        Visit(expressionStatement.Expression);
    }

    protected override void Visit(ExpressionBodyStatement expressionBodyStatement)
    {
        Visit(expressionBodyStatement.Expression);
    }

    protected override void Visit(ValStatement valStatement)
    {
        foreach (var kvp in valStatement.Definitions)
        {
            if (kvp.Value != null)
            {
                Visit(kvp.Value);
            }
        }
    }

    protected override void Visit(FnStatement fnStatement)
    {
        if (fnStatement.ParameterList != null)
        {
            Visit(fnStatement.ParameterList);
        }

        Visit(fnStatement.Statement);

        if (fnStatement.AttributeList != null)
        {
            Visit(fnStatement.AttributeList);
        }
    }

    protected override void Visit(FnIndexedStatement fnIndexedStatement)
    {
        Visit(fnIndexedStatement.Indexer);

        if (fnIndexedStatement.ParameterList != null)
        {
            Visit(fnIndexedStatement.ParameterList);
        }

        Visit(fnIndexedStatement.InnerStatement);

        if (fnIndexedStatement.AttributeList != null)
        {
            Visit(fnIndexedStatement.AttributeList);
        }
    }

    protected override void Visit(FnTargetedStatement fnTargetedStatement)
    {
        Visit(fnTargetedStatement.PrimaryTarget);

        if (fnTargetedStatement.ParameterList != null)
        {
            Visit(fnTargetedStatement.ParameterList);
        }

        Visit(fnTargetedStatement.InnerStatement);

        if (fnTargetedStatement.AttributeList != null)
        {
            Visit(fnTargetedStatement.AttributeList);
        }
    }

    protected override void Visit(FnExpression fnExpression)
    {
        if (fnExpression.ParameterList != null)
        {
            Visit(fnExpression.ParameterList);
        }

        Visit(fnExpression.Statement);
    }

    protected override void Visit(IfStatement ifStatement)
    {
        foreach (var cond in ifStatement.Conditions)
        {
            Visit(cond);
        }

        foreach (var stmt in ifStatement.Statements)
        {
            Visit(stmt);
        }

        if (ifStatement.ElseBranch != null)
        {
            Visit(ifStatement.ElseBranch);
        }
    }

    protected override void Visit(WhileStatement whileStatement)
    {
        Visit(whileStatement.Condition);
        Visit(whileStatement.Statement);
    }

    protected override void Visit(DoWhileStatement doWhileStatement)
    {
        Visit(doWhileStatement.Statement);
        Visit(doWhileStatement.Condition);
    }

    protected override void Visit(ForStatement forStatement)
    {
        foreach (var a in forStatement.Assignments)
        {
            Visit(a);
        }

        Visit(forStatement.Condition);

        foreach (var s in forStatement.IterationStatements)
        {
            Visit(s);
        }

        Visit(forStatement.Statement);
    }

    protected override void Visit(EachStatement eachStatement)
    {
        Visit(eachStatement.Iterable);
        Visit(eachStatement.Statement);
    }

    protected override void Visit(RetStatement retStatement)
    {
        if (retStatement.Expression != null)
        {
            Visit(retStatement.Expression);
        }
    }

    protected override void Visit(ThrowStatement throwStatement)
    {
        Visit(throwStatement.ThrownExpression);
    }

    protected override void Visit(TryStatement tryStatement)
    {
        Visit(tryStatement.InnerStatement);

        if (tryStatement.HandlerStatement != null)
        {
            Visit(tryStatement.HandlerStatement);
        }
    }

    protected override void Visit(BreakStatement breakStatement)
    {
    }

    protected override void Visit(SkipStatement skipStatement)
    {
    }

    protected override void Visit(RetryStatement retryStatement)
    {
    }

    protected override void Visit(AttributeList attributeList)
    {
        foreach (var attr in attributeList.Attributes)
        {
            Visit(attr);
        }
    }

    protected override void Visit(AttributeNode attributeNode)
    {
        foreach (var val in attributeNode.Values)
        {
            Visit(val);
        }

        foreach (var val in attributeNode.Properties.Values)
        {
            Visit(val);
        }
    }

    protected override void Visit(ParameterList parameterList)
    {
        foreach (var param in parameterList.Parameters)
        {
            if (param.Initializer != null)
            {
                Visit(param.Initializer);
            }
        }
    }

    protected override void Visit(ArgumentList argumentList)
    {
        foreach (var arg in argumentList.Arguments)
        {
            Visit(arg);
        }
    }

    protected override void Visit(ConditionalExpression conditionalExpression)
    {
        Visit(conditionalExpression.Condition);
        Visit(conditionalExpression.TrueExpression);
        Visit(conditionalExpression.FalseExpression);
    }

    protected override void Visit(CoalescingExpression coalescingExpression)
    {
        Visit(coalescingExpression.Left);
        Visit(coalescingExpression.Right);
    }

    protected override void Visit(AssignmentExpression assignmentExpression)
    {
        Visit(assignmentExpression.Left);
        Visit(assignmentExpression.Right);
    }

    protected override void Visit(BinaryExpression binaryExpression)
    {
        Visit(binaryExpression.Left);
        Visit(binaryExpression.Right);
    }

    protected override void Visit(UnaryExpression unaryExpression)
    {
        Visit(unaryExpression.Right);
    }

    protected override void Visit(InvocationExpression invocationExpression)
    {
        Visit(invocationExpression.Callee);
        Visit(invocationExpression.ArgumentList);
    }

    protected override void Visit(SelfInvocationExpression selfInvocationExpression)
    {
        Visit(selfInvocationExpression.Indexable);
        Visit(selfInvocationExpression.ArgumentList);
    }

    protected override void Visit(IndexerExpression indexerExpression)
    {
        Visit(indexerExpression.Indexable);
        Visit(indexerExpression.KeyExpression);
    }

    protected override void Visit(IncrementationExpression incrementationExpression)
    {
        Visit(incrementationExpression.Target);
    }

    protected override void Visit(DecrementationExpression decrementationExpression)
    {
        Visit(decrementationExpression.Target);
    }

    protected override void Visit(NonNilAssertionExpression nonNilAssertionExpression)
    {
        Visit(nonNilAssertionExpression.Operand);
    }

    protected override void Visit(TableExpression tableExpression)
    {
        foreach (var init in tableExpression.Initializers)
        {
            Visit(init);
        }
    }

    protected override void Visit(ArrayExpression arrayExpression)
    {
        if (arrayExpression.SizeExpression != null)
        {
            Visit(arrayExpression.SizeExpression);
        }

        foreach (var init in arrayExpression.Initializers)
        {
            Visit(init);
        }
    }

    protected override void Visit(ByExpression byExpression)
    {
        Visit(byExpression.Qualifier);

        foreach (var arm in byExpression.Arms)
        {
            Visit(arm.Selector);
            Visit(arm.ValueArm);
        }

        if (byExpression.ElseArm != null)
        {
            Visit(byExpression.ElseArm);
        }
    }

    protected override void Visit(WithExpression withExpression)
    {
        Visit(withExpression.BaseExpression);
        Visit(withExpression.TableExpansionExpression);
    }

    protected override void Visit(ErrorExpression errorExpression)
    {
        if (errorExpression.ImplicitMessageConstant != null)
        {
            Visit(errorExpression.ImplicitMessageConstant);
        }
        
        if (errorExpression.UserDataTable != null)
        {
            Visit(errorExpression.UserDataTable);
        }
    }

    protected override void Visit(TypeOfExpression typeOfExpression)
    {
        Visit(typeOfExpression.Target);
    }

    protected override void Visit(YieldExpression yieldExpression)
    {
        Visit(yieldExpression.Invocation);
    }

    protected override void Visit(IsExpression isExpression)
    {
        Visit(isExpression.Target);

        if (isExpression.ManagedTypeName != null)
        {
            Visit(isExpression.ManagedTypeName);
        }
    }

    protected override void Visit(SelfExpression selfExpression)
    {
    }

    protected override void Visit(SelfFnExpression selfFnExpression)
    {
        if (selfFnExpression.ParameterList != null)
        {
            Visit(selfFnExpression.ParameterList);
        }

        Visit(selfFnExpression.Statement);
    }

    protected override void Visit(SymbolReferenceExpression symbolReferenceExpression)
    {
    }

    protected override void Visit(NumberConstant numberConstant)
    {
    }

    protected override void Visit(StringConstant stringConstant)
    {
        if (stringConstant.IsInterpolated)
        {
            var segments = InterpolatedStringParser.ParseSegments(stringConstant, throwOnError: false);
            foreach (var segment in segments)
            {
                if (segment is { Kind: InterpolatedSegmentKind.Expression, Expr: not null })
                {
                    Visit(segment.Expr);
                }
            }
        }
    }

    protected override void Visit(NilConstant nilConstant)
    {
    }

    protected override void Visit(BooleanConstant booleanConstant)
    {
    }

    protected override void Visit(TypeCodeConstant typeCodeConstant)
    {
    }

    protected override void Visit(ExtraArgumentsExpression extraArgumentsExpression)
    {
    }

    protected override void Visit(DiagnosticNode diagnosticNode)
    {
    }

    protected override void Visit(IdentifierNode identifierNode)
    {
    }

    protected override void Visit(KeyValuePairExpression keyValuePairExpression)
    {
        Visit(keyValuePairExpression.KeyNode);
        Visit(keyValuePairExpression.ValueNode);
    }

    protected override void Visit(ByArmNode byArmNode)
    {
        Visit(byArmNode.Selector);
        Visit(byArmNode.ValueArm);
    }

    protected override void Visit(ParameterNode parameterNode)
    {
        if (parameterNode.Initializer != null)
        {
            Visit(parameterNode.Initializer);
        }
    }
}