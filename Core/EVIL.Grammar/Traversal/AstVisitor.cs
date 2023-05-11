using System;
using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Statements;

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
                { typeof(ArgumentList), (n) => Visit((ArgumentList)n) },
                { typeof(ParameterList), (n) => Visit((ParameterList)n) },
                { typeof(BlockStatement), (n) => Visit((BlockStatement)n) },
                { typeof(ConditionalExpression), (n) => Visit((ConditionalExpression)n) },
                { typeof(NumberConstant), (n) => Visit((NumberConstant)n) },
                { typeof(StringConstant), (n) => Visit((StringConstant)n) },
                { typeof(NilConstant), (n) => Visit((NilConstant)n) },
                { typeof(BooleanConstant), (n) => Visit((BooleanConstant)n) },
                { typeof(AssignmentExpression), (n) => Visit((AssignmentExpression)n) },
                { typeof(BinaryExpression), (n) => Visit((BinaryExpression)n) },
                { typeof(UnaryExpression), (n) => Visit((UnaryExpression)n) },
                { typeof(VariableReferenceExpression), (n) => Visit((VariableReferenceExpression)n) },
                { typeof(VariableDefinition), (n) => Visit((VariableDefinition)n) },
                { typeof(FunctionDefinition), (n) => Visit((FunctionDefinition)n) },
                { typeof(FunctionCallExpression), (n) => Visit((FunctionCallExpression)n) },
                { typeof(IfStatement), (n) => Visit((IfStatement)n) },
                { typeof(ForStatement), (n) => Visit((ForStatement)n) },
                { typeof(DoWhileStatement), (n) => Visit((DoWhileStatement)n) },
                { typeof(WhileStatement), (n) => Visit((WhileStatement)n) },
                { typeof(ReturnStatement), (n) => Visit((ReturnStatement)n) },
                { typeof(BreakStatement), (n) => Visit((BreakStatement)n) },
                { typeof(SkipStatement), (n) => Visit((SkipStatement)n) },
                { typeof(TableExpression), (n) => Visit((TableExpression)n) },
                { typeof(IndexerExpression), (n) => Visit((IndexerExpression)n) },
                { typeof(IncrementationExpression), (n) => Visit((IncrementationExpression)n) },
                { typeof(DecrementationExpression), (n) => Visit((DecrementationExpression)n) },
                { typeof(ExpressionStatement), (n) => Visit((ExpressionStatement)n) },
                { typeof(AttributeStatement), (n) => Visit((AttributeStatement)n) },
                { typeof(AttributeListStatement), (n) => Visit((AttributeListStatement)n) },
                { typeof(TypeOfExpression), (n) => Visit((TypeOfExpression)n) },
                { typeof(YieldExpression), (n) => Visit((YieldExpression)n) },
                { typeof(ExpressionBodyStatement), (n) => Visit((ExpressionBodyStatement)n) },
                { typeof(ExtraArgumentsExpression), (n) => Visit((ExtraArgumentsExpression)n) },
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

        public abstract void Visit(Program program);
        public abstract void Visit(ParameterList parameterList);
        public abstract void Visit(ArgumentList argumentList);
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
        public abstract void Visit(TypeOfExpression typeOfExpression);
        public abstract void Visit(YieldExpression yieldExpression);
        public abstract void Visit(ExpressionBodyStatement expressionBodyStatement);
        public abstract void Visit(ExtraArgumentsExpression extraArgumentsExpression);
    }
}