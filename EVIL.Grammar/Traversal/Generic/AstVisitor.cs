using System;
using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;

namespace EVIL.Grammar.Traversal.Generic
{
    public abstract class AstVisitor<T>
    {
        protected Dictionary<Type, ExpressionHandler> ExpressionHandlers { get; }
        protected Dictionary<Type, StatementHandler> StatementHandlers { get; }
        
        protected delegate T ExpressionHandler(Expression expression);
        protected delegate void StatementHandler(Statement statement);

        protected AstVisitor()
        {
            ExpressionHandlers = new()
            {
                {typeof(ConditionalExpression), (n) => Visit(n as ConditionalExpression)},
                {typeof(NumberExpression), (n) => Visit(n as NumberExpression)},
                {typeof(StringConstant), (n) => Visit(n as StringConstant)},
                {typeof(AssignmentExpression), (n) => Visit(n as AssignmentExpression)},
                {typeof(BinaryExpression), (n) => Visit(n as BinaryExpression)},
                {typeof(UnaryExpression), (n) => Visit(n as UnaryExpression)},
                {typeof(VariableReference), (n) => Visit(n as VariableReference)},
                {typeof(FunctionExpression), (n) => Visit(n as FunctionExpression)},
                {typeof(FunctionCallExpression), (n) => Visit(n as FunctionCallExpression)},
                {typeof(TableExpression), (n) => Visit(n as TableExpression)},
                {typeof(IndexerExpression), (n) => Visit(n as IndexerExpression)},
                {typeof(IncrementationExpression), (n) => Visit(n as IncrementationExpression)},
                {typeof(DecrementationExpression), (n) => Visit(n as DecrementationExpression)},
            };

            StatementHandlers = new()
            {
                { typeof(Program), (n) => Visit(n as Program) },
                { typeof(BlockStatement), (n) => Visit(n as BlockStatement) },
                { typeof(VariableDefinition), (n) => Visit(n as VariableDefinition) },
                { typeof(FunctionDefinition), (n) => Visit(n as FunctionDefinition) },
                { typeof(IfStatement), (n) => Visit(n as IfStatement) },
                { typeof(ExitStatement), (n) => Visit(n as ExitStatement) },
                { typeof(ForStatement), (n) => Visit(n as ForStatement) },
                { typeof(DoWhileStatement), (n) => Visit(n as DoWhileStatement) },
                { typeof(WhileStatement), (n) => Visit(n as WhileStatement) },
                { typeof(ReturnStatement), (n) => Visit(n as ReturnStatement) },
                { typeof(BreakStatement), (n) => Visit(n as BreakStatement) },
                { typeof(SkipStatement), (n) => Visit(n as SkipStatement) },
                { typeof(UndefStatement), (n) => Visit(n as UndefStatement) },
                { typeof(EachStatement), (n) => Visit(n as EachStatement) },
                { typeof(ExpressionStatement), (n) => Visit(n as ExpressionStatement) }
            };
        }

        public virtual T Visit(Expression expression)
        {
            var type = expression.GetType();

            if (!ExpressionHandlers.ContainsKey(type))
                throw new Exception($"{type.Name} is not an expression node.");

            return ExpressionHandlers[type](expression);
        }

        public virtual void Visit(Statement statement)
        {
            var type = statement.GetType();

            if (!StatementHandlers.ContainsKey(type))
                throw new Exception($"{type.Name} is not a statement node.");

            StatementHandlers[type](statement);
        }

        public abstract T Visit(ConditionalExpression conditionalExpression);
        public abstract T Visit(NumberExpression numberExpression);
        public abstract T Visit(StringConstant stringConstant);
        public abstract T Visit(AssignmentExpression assignmentExpression);
        public abstract T Visit(BinaryExpression binaryExpression);
        public abstract T Visit(UnaryExpression unaryExpression);
        public abstract T Visit(VariableReference variableReference);
        public abstract T Visit(FunctionExpression functionExpression);
        public abstract T Visit(FunctionCallExpression functionCallExpression);
        public abstract T Visit(TableExpression tableExpression);
        public abstract T Visit(IndexerExpression indexerExpression);
        public abstract T Visit(IncrementationExpression incrementationExpression);
        public abstract T Visit(DecrementationExpression decrementationExpression);
        
        public abstract void Visit(Program program);
        public abstract void Visit(BlockStatement blockStatement);
        public abstract void Visit(VariableDefinition variableDefinition);
        public abstract void Visit(FunctionDefinition functionDefinition);
        public abstract void Visit(IfStatement ifStatement);
        public abstract void Visit(ExitStatement exitStatement);
        public abstract void Visit(ForStatement forStatement);
        public abstract void Visit(DoWhileStatement doWhileStatement);
        public abstract void Visit(WhileStatement whileStatement);
        public abstract void Visit(ReturnStatement returnStatement);
        public abstract void Visit(BreakStatement breakStatement);
        public abstract void Visit(SkipStatement skipStatement);
        public abstract void Visit(UndefStatement undefStatement);
        public abstract void Visit(EachStatement eachStatement);
        public abstract void Visit(ExpressionStatement expressionStatement);
    }
}