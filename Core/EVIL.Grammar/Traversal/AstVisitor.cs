using System;
using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements;
using EVIL.Grammar.AST.Statements.TopLevel;

namespace EVIL.Grammar.Traversal
{
    public abstract class AstVisitor
    {
        protected Dictionary<Type, NodeHandler> Handlers { get; }

        protected delegate void NodeHandler(AstNode node);

        protected AstVisitor()
        {
#nullable disable
            Handlers = new()
            {
                { typeof(IncludeStatement), (n) => Visit((IncludeStatement)n) },
                { typeof(ArgumentList), (n) => Visit((ArgumentList)n) },
                { typeof(ParameterList), (n) => Visit((ParameterList)n) },
                { typeof(BlockStatement), (n) => Visit((BlockStatement)n) },
                { typeof(ConditionalExpression), (n) => Visit((ConditionalExpression)n) },
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
                { typeof(FnStatement), (n) => Visit((FnStatement)n) },
                { typeof(InvocationExpression), (n) => Visit((InvocationExpression)n) },
                { typeof(IfStatement), (n) => Visit((IfStatement)n) },
                { typeof(ForStatement), (n) => Visit((ForStatement)n) },
                { typeof(DoWhileStatement), (n) => Visit((DoWhileStatement)n) },
                { typeof(WhileStatement), (n) => Visit((WhileStatement)n) },
                { typeof(EachStatement), (n) => Visit((EachStatement)n) },
                { typeof(RetStatement), (n) => Visit((RetStatement)n) },
                { typeof(BreakStatement), (n) => Visit((BreakStatement)n) },
                { typeof(SkipStatement), (n) => Visit((SkipStatement)n) },
                { typeof(TableExpression), (n) => Visit((TableExpression)n) },
                { typeof(ArrayExpression), (n) => Visit((ArrayExpression)n) },
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
                { typeof(IsExpression), (n) => Visit((IsExpression)n)}
            };
#nullable enable
        }

        public virtual void Visit(AstNode node)
        {
            var type = node.GetType();

            if (!Handlers.ContainsKey(type))
            {
                throw new Exception(
                    $"Unknown node type '{type.Name}'. " +
                    $"It might be that you forgor to add a handler for it."
                );
            }

            Handlers[type](node);
        }

        public abstract void Visit(ProgramNode programNode);
        public abstract void Visit(ParameterList parameterList);
        public abstract void Visit(ArgumentList argumentList);
        public abstract void Visit(IncludeStatement includeStatement);
        public abstract void Visit(BlockStatement blockStatement);
        public abstract void Visit(ConditionalExpression conditionalExpression);
        public abstract void Visit(NumberConstant numberConstant);
        public abstract void Visit(StringConstant stringConstant);
        public abstract void Visit(NilConstant nilConstant);
        public abstract void Visit(BooleanConstant booleanConstant);
        public abstract void Visit(TypeCodeConstant typeCodeConstant);
        public abstract void Visit(AssignmentExpression assignmentExpression);
        public abstract void Visit(BinaryExpression binaryExpression);
        public abstract void Visit(UnaryExpression unaryExpression);
        public abstract void Visit(SymbolReferenceExpression symbolReferenceExpression);
        public abstract void Visit(ValStatement valStatement);
        public abstract void Visit(FnStatement fnStatement);
        public abstract void Visit(InvocationExpression invocationExpression);
        public abstract void Visit(IfStatement ifStatement);
        public abstract void Visit(ForStatement forStatement);
        public abstract void Visit(DoWhileStatement doWhileStatement);
        public abstract void Visit(WhileStatement whileStatement);
        public abstract void Visit(EachStatement eachStatement);
        public abstract void Visit(RetStatement retStatement);
        public abstract void Visit(BreakStatement breakStatement);
        public abstract void Visit(SkipStatement skipStatement);
        public abstract void Visit(TableExpression tableExpression);
        public abstract void Visit(ArrayExpression arrayExpression);
        public abstract void Visit(IndexerExpression indexerExpression);
        public abstract void Visit(IncrementationExpression incrementationExpression);
        public abstract void Visit(DecrementationExpression decrementationExpression);
        public abstract void Visit(ExpressionStatement expressionStatement);
        public abstract void Visit(AttributeNode attributeNode);
        public abstract void Visit(TypeOfExpression typeOfExpression);
        public abstract void Visit(YieldExpression yieldExpression);
        public abstract void Visit(ExpressionBodyStatement expressionBodyStatement);
        public abstract void Visit(ExtraArgumentsExpression extraArgumentsExpression);
        public abstract void Visit(FnExpression fnExpression);
        public abstract void Visit(IsExpression isExpression);
    }
}