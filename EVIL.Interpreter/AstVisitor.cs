using System;
using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter
{
    public abstract class AstVisitor
    {
        private Dictionary<Type, NodeHandler> _handlers;

        protected delegate DynValue NodeHandler(AstNode node);

        protected AstVisitor()
        {
            _handlers = new()
            {
                {typeof(ProgramNode), (n) => Visit(n as ProgramNode)},
                {typeof(BlockStatementNode), (n) => Visit(n as BlockStatementNode)},
                {typeof(ConditionalExpressionNode), (n) => Visit(n as ConditionalExpressionNode)},
                {typeof(DecimalNode), (n) => Visit(n as DecimalNode)},
                {typeof(IntegerNode), (n) => Visit(n as IntegerNode)},
                {typeof(StringNode), (n) => Visit(n as StringNode)},
                {typeof(AssignmentNode), (n) => Visit(n as AssignmentNode)},
                {typeof(BinaryOperationNode), (n) => Visit(n as BinaryOperationNode)},
                {typeof(UnaryOperationNode), (n) => Visit(n as UnaryOperationNode)},
                {typeof(VariableNode), (n) => Visit(n as VariableNode)},
                {typeof(VariableDefinitionNode), (n) => Visit(n as VariableDefinitionNode)},
                {typeof(FunctionDefinitionNode), (n) => Visit(n as FunctionDefinitionNode)},
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
            };
        }

        public DynValue Visit(AstNode node)
        {
            var type = node.GetType();

            if (!_handlers.ContainsKey(type))
                throw new Exception("Forgot to add a node type, idiot.");

            ConstraintCheck(node);

            return _handlers[type](node);
        }

        protected abstract void ConstraintCheck(AstNode node);

        public abstract DynValue Visit(ProgramNode programNode);
        public abstract DynValue Visit(BlockStatementNode blockStatementNode);
        public abstract DynValue Visit(ConditionalExpressionNode conditionalExpressionNode);
        public abstract DynValue Visit(DecimalNode decimalNode);
        public abstract DynValue Visit(IntegerNode integerNode);
        public abstract DynValue Visit(StringNode stringNode);
        public abstract DynValue Visit(AssignmentNode assignmentNode);
        public abstract DynValue Visit(BinaryOperationNode binaryOperationNode);
        public abstract DynValue Visit(UnaryOperationNode unaryOperationNode);
        public abstract DynValue Visit(VariableNode variableNode);
        public abstract DynValue Visit(VariableDefinitionNode variableDefinitionNode);
        public abstract DynValue Visit(FunctionDefinitionNode scriptFunctionDefinitionNode);
        public abstract DynValue Visit(FunctionCallNode functionCallNode);
        public abstract DynValue Visit(ConditionNode conditionNode);
        public abstract DynValue Visit(ExitNode exitNode);
        public abstract DynValue Visit(ForLoopNode forLoopNode);
        public abstract DynValue Visit(DoWhileLoopNode doWhileLoopNode);
        public abstract DynValue Visit(WhileLoopNode whileLoopNode);
        public abstract DynValue Visit(ReturnNode returnNode);
        public abstract DynValue Visit(BreakNode breakNode);
        public abstract DynValue Visit(SkipNode nextNode);
        public abstract DynValue Visit(TableNode tableNode);
        public abstract DynValue Visit(IndexingNode indexingNode);
        public abstract DynValue Visit(IncrementationNode incrementationNode);
        public abstract DynValue Visit(DecrementationNode decrementationNode);
        public abstract DynValue Visit(UndefNode undefNode);
        public abstract DynValue Visit(EachLoopNode eachLoopNode);
    }
}