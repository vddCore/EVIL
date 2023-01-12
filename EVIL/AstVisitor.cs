using System;
using EVIL.Abstraction;
using EVIL.AST.Base;
using EVIL.AST.Nodes;

namespace EVIL
{
    public abstract class AstVisitor
    {
        protected abstract void ConstraintCheck(AstNode node);
        
        public DynValue Visit(AstNode node)
        {
            ConstraintCheck(node);
            
            if (node is RootNode rootNode)
                return Visit(rootNode);
            else if (node is NumberNode numberNode)
                return Visit(numberNode);
            else if (node is StringNode stringNode)
                return Visit(stringNode);
            else if (node is AssignmentNode assignmentNode)
                return Visit(assignmentNode);
            else if (node is BinaryOperationNode binaryOperationNode)
                return Visit(binaryOperationNode);
            else if (node is UnaryOperationNode unaryOperationNode)
                return Visit(unaryOperationNode);
            else if (node is VariableNode variableNode)
                return Visit(variableNode);
            else if (node is VariableDefinitionNode variableDefinitionNode)
                return Visit(variableDefinitionNode);
            else if (node is FunctionDefinitionNode scriptFunctionDefinitionNode)
                return Visit(scriptFunctionDefinitionNode);
            else if (node is FunctionCallNode functionCallNode)
                return Visit(functionCallNode);
            else if (node is ConditionNode conditionNode)
                return Visit(conditionNode);
            else if (node is ExitNode exitNode)
                return Visit(exitNode);
            else if (node is ForLoopNode forLoopNode)
                return Visit(forLoopNode);
            else if (node is WhileLoopNode whileLoopNode)
                return Visit(whileLoopNode);
            else if (node is ReturnNode returnNode)
                return Visit(returnNode);
            else if (node is BreakNode breakNode)
                return Visit(breakNode);
            else if (node is SkipNode skipNode)
                return Visit(skipNode);
            else if (node is TableNode tableNode)
                return Visit(tableNode);
            else if (node is IndexingNode tableIndexingNode)
                return Visit(tableIndexingNode);
            else if (node is MemberAccessNode memberAccessNode)
                return Visit(memberAccessNode);
            else if (node is IncrementationNode incrementationNode)
                return Visit(incrementationNode);
            else if (node is DecrementationNode decrementationNode)
                return Visit(decrementationNode);
            else if (node is UndefNode undefNode)
                return Visit(undefNode);
            else if (node is EachLoopNode eachLoopNode)
                return Visit(eachLoopNode);
            else throw new Exception("Forgot to add a node type, idiot.");
        }

        public abstract DynValue Visit(RootNode rootNode);
        public abstract DynValue Visit(NumberNode numberNode);
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
        public abstract DynValue Visit(WhileLoopNode whileLoopNode);
        public abstract DynValue Visit(ReturnNode returnNode);
        public abstract DynValue Visit(BreakNode breakNode);
        public abstract DynValue Visit(SkipNode nextNode);
        public abstract DynValue Visit(TableNode tableNode);
        public abstract DynValue Visit(IndexingNode indexingNode);
        public abstract DynValue Visit(MemberAccessNode memberAccessNode);
        public abstract DynValue Visit(IncrementationNode incrementationNode);
        public abstract DynValue Visit(DecrementationNode decrementationNode);
        public abstract DynValue Visit(UndefNode undefNode);
        public abstract DynValue Visit(EachLoopNode eachLoopNode);
    }
}