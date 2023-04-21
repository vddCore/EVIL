using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Statements;

namespace EVIL.Grammar.Traversal
{
    public abstract class AstVisitor
    {
        protected Dictionary<Type, NodeHandler> Handlers { get; } = new();

        protected delegate void NodeHandler(AstNode node);

        protected AstVisitor()
        {
            var methods = GetType().GetMethods(
                BindingFlags.Instance
                | BindingFlags.Public
            ).Where(x => x.Name == nameof(Visit));

            foreach (var handler in methods)
            {
                var parameters = handler.GetParameters();
                if (parameters.Length != 1) continue;

                var parameterType = parameters[0].ParameterType;

                if (parameterType == typeof(AstNode)) continue;
                if (parameterType == typeof(Program)) continue;
                
                if (!parameterType.IsAssignableTo(typeof(AstNode))) continue;

                Handlers.Add(parameterType, (n) => handler?.Invoke(this, new object[] { n }));
            }
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

        public virtual void Visit(Program program)
        {
            foreach (var statement in program.Statements)
                Visit(statement);
        }

        public abstract void Visit(BlockStatement blockStatement);
        public abstract void Visit(ConditionalExpression conditionalExpression);
        public abstract void Visit(NumberConstant numberConstant);
        public abstract void Visit(StringConstant stringConstant);
        public abstract void Visit(NilConstant nilConstant);
        public abstract void Visit(BooleanConstant booleanConstant);
        public abstract void Visit(AssignmentExpression assignmentExpression);
        public abstract void Visit(BinaryExpression binaryExpression);
        public abstract void Visit(UnaryExpression unaryExpression);
        public abstract void Visit(VariableReferenceExpression variableReferenceExpression);
        public abstract void Visit(VariableDefinition variableDefinition);
        public abstract void Visit(FunctionDefinition functionDefinition);
        public abstract void Visit(FunctionCallExpression functionCallExpression);
        public abstract void Visit(IfStatement ifStatement);
        public abstract void Visit(ForStatement forStatement);
        public abstract void Visit(DoWhileStatement doWhileStatement);
        public abstract void Visit(WhileStatement whileStatement);
        public abstract void Visit(ReturnStatement returnStatement);
        public abstract void Visit(BreakStatement breakStatement);
        public abstract void Visit(SkipStatement skipStatement);
        public abstract void Visit(TableExpression tableExpression);
        public abstract void Visit(IndexerExpression indexerExpression);
        public abstract void Visit(IncrementationExpression incrementationExpression);
        public abstract void Visit(DecrementationExpression decrementationExpression);
        public abstract void Visit(ExpressionStatement expressionStatement);
        public abstract void Visit(AttributeStatement attributeStatement);
        public abstract void Visit(AttributeListStatement attributeListStatement);
    }
}