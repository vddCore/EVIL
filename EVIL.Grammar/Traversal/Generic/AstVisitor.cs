using System;
using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;

namespace EVIL.Grammar.Traversal.Generic
{
    public abstract class AstVisitor<T>
    {
        protected Dictionary<Type, NodeHandler> Handlers { get; }
        
        protected delegate T NodeHandler(AstNode node);

        protected AstVisitor()
        {
            Handlers = new()
            {
                {typeof(ProgramNode), (n) => Visit(n as ProgramNode)},
                {typeof(BlockStatementNode), (n) => Visit(n as BlockStatementNode)},
                {typeof(ConditionalExpressionNode), (n) => Visit(n as ConditionalExpressionNode)},
                {typeof(NumberNode), (n) => Visit(n as NumberNode)},
                {typeof(StringNode), (n) => Visit(n as StringNode)},
                {typeof(AssignmentNode), (n) => Visit(n as AssignmentNode)},
                {typeof(BinaryOperationNode), (n) => Visit(n as BinaryOperationNode)},
                {typeof(UnaryOperationNode), (n) => Visit(n as UnaryOperationNode)},
                {typeof(VariableNode), (n) => Visit(n as VariableNode)},
                {typeof(VariableDefinitionNode), (n) => Visit(n as VariableDefinitionNode)},
                {typeof(FunctionDefinitionNamedNode), (n) => Visit(n as FunctionDefinitionNamedNode)},
                {typeof(FunctionDefinitionAnonymousNode), (n) => Visit(n as FunctionDefinitionAnonymousNode)},
                {typeof(FunctionCallNode), (n) => Visit(n as FunctionCallNode)},
                {typeof(DecisionNode), (n) => Visit(n as DecisionNode)},
                {typeof(ExitNode), (n) => Visit(n as ExitNode)},
                {typeof(ForLoopNode), (n) => Visit(n as ForLoopNode)},
                {typeof(DoWhileLoopNode), (n) => Visit(n as DoWhileLoopNode)},
                {typeof(WhileLoopNode), (n) => Visit(n as WhileLoopNode)},
                {typeof(ReturnNode), (n) => Visit(n as ReturnNode)},
                {typeof(BreakNode), (n) => Visit(n as BreakNode)},
                {typeof(SkipNode), (n) => Visit(n as SkipNode)},
                {typeof(TableNode), (n) => Visit(n as TableNode)},
                {typeof(IndexingNode), (n) => Visit(n as IndexingNode)},
                {typeof(IncrementationNode), (n) => Visit(n as IncrementationNode)},
                {typeof(DecrementationNode), (n) => Visit(n as DecrementationNode)},
                {typeof(UndefNode), (n) => Visit(n as UndefNode)},
                {typeof(EachLoopNode), (n) => Visit(n as EachLoopNode)},
                {typeof(ParameterListNode), (n) => Visit(n as ParameterListNode)},
                {typeof(ArgumentListNode), (n) => Visit(n as ArgumentListNode)},
            };
        }

        public virtual T Visit(AstNode node)
        {
            var type = node.GetType();

            if (!Handlers.ContainsKey(type))
                throw new Exception("Forgot to add a node type, idiot.");

            return Handlers[type](node);
        }

        public abstract T Visit(ProgramNode programNode);
        public abstract T Visit(BlockStatementNode blockStatementNode);
        public abstract T Visit(ConditionalExpressionNode conditionalExpressionNode);
        public abstract T Visit(NumberNode numberNode);
        public abstract T Visit(StringNode stringNode);
        public abstract T Visit(AssignmentNode assignmentNode);
        public abstract T Visit(BinaryOperationNode binaryOperationNode);
        public abstract T Visit(UnaryOperationNode unaryOperationNode);
        public abstract T Visit(VariableNode variableNode);
        public abstract T Visit(VariableDefinitionNode variableDefinitionNode);
        public abstract T Visit(FunctionDefinitionNamedNode functionDefinitionNamedNode);
        public abstract T Visit(FunctionDefinitionAnonymousNode functionDefinitionAnonymousNode);
        public abstract T Visit(FunctionCallNode functionCallNode);
        public abstract T Visit(DecisionNode decisionNode);
        public abstract T Visit(ExitNode exitNode);
        public abstract T Visit(ForLoopNode forLoopNode);
        public abstract T Visit(DoWhileLoopNode doWhileLoopNode);
        public abstract T Visit(WhileLoopNode whileLoopNode);
        public abstract T Visit(ReturnNode returnNode);
        public abstract T Visit(BreakNode breakNode);
        public abstract T Visit(SkipNode skipNode);
        public abstract T Visit(TableNode tableNode);
        public abstract T Visit(IndexingNode indexingNode);
        public abstract T Visit(IncrementationNode incrementationNode);
        public abstract T Visit(DecrementationNode decrementationNode);
        public abstract T Visit(UndefNode undefNode);
        public abstract T Visit(EachLoopNode eachLoopNode);
        public abstract T Visit(ParameterListNode parameterListNode);
        public abstract T Visit(ArgumentListNode argumentListNode);
    }
}