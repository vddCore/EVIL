using System;
using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;

namespace EVIL.Grammar.Traversal
{
    public abstract class AstVisitor
    {
        protected Dictionary<Type, NodeHandler> Handlers { get; }

        protected delegate void NodeHandler(AstNode node);

        protected AstVisitor()
        {
            Handlers = new()
            {
                {typeof(Program), (n) => Visit(n as Program)},
                {typeof(BlockStatement), (n) => Visit(n as BlockStatement)},
                {typeof(ConditionalExpression), (n) => Visit(n as ConditionalExpression)},
                {typeof(NumberExpression), (n) => Visit(n as NumberExpression)},
                {typeof(StringConstant), (n) => Visit(n as StringConstant)},
                {typeof(AssignmentExpression), (n) => Visit(n as AssignmentExpression)},
                {typeof(BinaryExpression), (n) => Visit(n as BinaryExpression)},
                {typeof(UnaryExpression), (n) => Visit(n as UnaryExpression)},
                {typeof(VariableReference), (n) => Visit(n as VariableReference)},
                {typeof(VariableDefinition), (n) => Visit(n as VariableDefinition)},
                {typeof(FunctionDefinition), (n) => Visit(n as FunctionDefinition)},
                {typeof(FunctionExpression), (n) => Visit(n as FunctionExpression)},
                {typeof(FunctionCallExpression), (n) => Visit(n as FunctionCallExpression)},
                {typeof(IfStatement), (n) => Visit(n as IfStatement)},
                {typeof(ExitStatement), (n) => Visit(n as ExitStatement)},
                {typeof(ForStatement), (n) => Visit(n as ForStatement)},
                {typeof(DoWhileStatement), (n) => Visit(n as DoWhileStatement)},
                {typeof(WhileStatement), (n) => Visit(n as WhileStatement)},
                {typeof(ReturnStatement), (n) => Visit(n as ReturnStatement)},
                {typeof(BreakStatement), (n) => Visit(n as BreakStatement)},
                {typeof(SkipStatement), (n) => Visit(n as SkipStatement)},
                {typeof(TableExpression), (n) => Visit(n as TableExpression)},
                {typeof(IndexerExpression), (n) => Visit(n as IndexerExpression)},
                {typeof(IncrementationExpression), (n) => Visit(n as IncrementationExpression)},
                {typeof(DecrementationExpression), (n) => Visit(n as DecrementationExpression)},
                {typeof(UndefStatement), (n) => Visit(n as UndefStatement)},
                {typeof(EachStatement), (n) => Visit(n as EachStatement)},
                {typeof(ExpressionStatement), (n) => Visit(n as ExpressionStatement)},
                {typeof(ExpressionStatement), (n) => Visit(n as ExtraArgumentsExpression)}
            };
        }

        public virtual void Visit(AstNode node)
        {
            var type = node.GetType();

            if (!Handlers.ContainsKey(type))
                throw new Exception("Forgot to add a node type, idiot.");

            Handlers[type](node);
        }

        public abstract void Visit(Program program);
        public abstract void Visit(BlockStatement blockStatement);
        public abstract void Visit(ConditionalExpression conditionalExpression);
        public abstract void Visit(NumberExpression numberExpression);
        public abstract void Visit(StringConstant stringConstant);
        public abstract void Visit(AssignmentExpression assignmentExpression);
        public abstract void Visit(BinaryExpression binaryExpression);
        public abstract void Visit(UnaryExpression unaryExpression);
        public abstract void Visit(VariableReference variableReference);
        public abstract void Visit(VariableDefinition variableDefinition);
        public abstract void Visit(FunctionDefinition functionDefinition);
        public abstract void Visit(FunctionExpression functionExpression);
        public abstract void Visit(FunctionCallExpression functionCallExpression);
        public abstract void Visit(IfStatement ifStatement);
        public abstract void Visit(ExitStatement exitStatement);
        public abstract void Visit(ForStatement forStatement);
        public abstract void Visit(DoWhileStatement doWhileStatement);
        public abstract void Visit(WhileStatement whileStatement);
        public abstract void Visit(ReturnStatement returnStatement);
        public abstract void Visit(BreakStatement breakStatement);
        public abstract void Visit(SkipStatement nextStatement);
        public abstract void Visit(TableExpression tableExpression);
        public abstract void Visit(IndexerExpression indexerExpression);
        public abstract void Visit(IncrementationExpression incrementationExpression);
        public abstract void Visit(DecrementationExpression decrementationExpression);
        public abstract void Visit(UndefStatement undefStatement);
        public abstract void Visit(EachStatement eachStatement);
        public abstract void Visit(ExpressionStatement expressionStatement);
        public abstract void Visit(ExtraArgumentsExpression extraArgumentsExpression);
    }
}