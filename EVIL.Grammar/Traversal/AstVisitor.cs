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
                {typeof(ConditionNode), (n) => Visit(n as ConditionNode)},
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

        public virtual void Visit(AstNode node)
        {
            var type = node.GetType();

            if (!Handlers.ContainsKey(type))
                throw new Exception("Forgot to add a node type, idiot.");

            Handlers[type](node);
        }

        public abstract void Visit(ProgramNode programNode);
        public abstract void Visit(BlockStatementNode blockStatementNode);
        public abstract void Visit(ConditionalExpressionNode conditionalExpressionNode);
        public abstract void Visit(NumberNode numberNode);
        public abstract void Visit(StringNode stringNode);
        public abstract void Visit(AssignmentNode assignmentNode);
        public abstract void Visit(BinaryOperationNode binaryOperationNode);
        public abstract void Visit(UnaryOperationNode unaryOperationNode);
        public abstract void Visit(VariableNode variableNode);
        public abstract void Visit(VariableDefinitionNode variableDefinitionNode);
        public abstract void Visit(FunctionDefinitionNamedNode functionDefinitionNamedNode);
        public abstract void Visit(FunctionDefinitionAnonymousNode functionDefinitionAnonymousNode);
        public abstract void Visit(FunctionCallNode functionCallNode);
        public abstract void Visit(ConditionNode conditionNode);
        public abstract void Visit(ExitNode exitNode);
        public abstract void Visit(ForLoopNode forLoopNode);
        public abstract void Visit(DoWhileLoopNode doWhileLoopNode);
        public abstract void Visit(WhileLoopNode whileLoopNode);
        public abstract void Visit(ReturnNode returnNode);
        public abstract void Visit(BreakNode breakNode);
        public abstract void Visit(SkipNode nextNode);
        public abstract void Visit(TableNode tableNode);
        public abstract void Visit(IndexingNode indexingNode);
        public abstract void Visit(IncrementationNode incrementationNode);
        public abstract void Visit(DecrementationNode decrementationNode);
        public abstract void Visit(UndefNode undefNode);
        public abstract void Visit(EachLoopNode eachLoopNode);
        public abstract void Visit(ParameterListNode parameterListNode);
        public abstract void Visit(ArgumentListNode argumentListNode);
    }
}