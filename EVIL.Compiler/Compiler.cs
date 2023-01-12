using System;
using System.Collections.Generic;
using EVIL.Grammar.AST.Nodes;
using EVIL.Grammar.Traversal;

namespace EVIL.Compiler
{
    public class Compiler : AstVisitor
    {
        private Stack<SymbolInfo> _definitionStack = new();
        
        public Scope GlobalScope { get; } = new();
        public Scope CurrentScope { get; private set; }

        public Compiler()
        {
            CurrentScope = GlobalScope;
        }
        
        public override void Visit(ProgramNode programNode)
        {
            for (var i = 0; i < programNode.Statements.Count; i++)
            {
                Visit(programNode.Statements[i]);
            }
        }

        public override void Visit(BlockStatementNode blockStatementNode)
        {
            CurrentScope = CurrentScope.Descend();
            {
                for (var i = 0; i < blockStatementNode.Statements.Count; i++)
                {
                    Visit(blockStatementNode.Statements[i]);
                }
            }
            CurrentScope = CurrentScope.Parent;
        }

        public override void Visit(ConditionalExpressionNode conditionalExpressionNode)
        {
            throw new NotImplementedException();
        }

        public override void Visit(NumberNode numberNode)
        {
            throw new NotImplementedException();
        }

        public override void Visit(StringNode stringNode)
        {
            throw new NotImplementedException();
        }

        public override void Visit(AssignmentNode assignmentNode)
        {
            throw new NotImplementedException();
        }

        public override void Visit(BinaryOperationNode binaryOperationNode)
        {
            throw new NotImplementedException();
        }

        public override void Visit(UnaryOperationNode unaryOperationNode)
        {
            throw new NotImplementedException();
        }

        public override void Visit(VariableNode variableNode)
        {
            var symbolInfo = CurrentScope.FindInScope(variableNode.Identifier);
        }

        public override void Visit(VariableDefinitionNode variableDefinitionNode)
        {
            CurrentScope.Define(variableDefinitionNode.Identifier, SymbolType.Number, 0);

            // todo
            
            if (variableDefinitionNode.Initializer != null)
            {
                Visit(variableDefinitionNode.Initializer);
            }
        }

        public override void Visit(FunctionDefinitionNamedNode functionDefinitionNamedNode)
        {
            CurrentScope.Define(functionDefinitionNamedNode.Identifier, SymbolType.Function);
            
            // todo
        }

        public override void Visit(FunctionDefinitionAnonymousNode functionDefinitionAnonymousNode)
        {
            throw new NotImplementedException();
        }

        public override void Visit(FunctionCallNode functionCallNode)
        {
            throw new NotImplementedException();
        }

        public override void Visit(DecisionNode decisionNode)
        {
            throw new NotImplementedException();
        }

        public override void Visit(ExitNode exitNode)
        {
            throw new NotImplementedException();
        }

        public override void Visit(ForLoopNode forLoopNode)
        {
            throw new NotImplementedException();
        }

        public override void Visit(DoWhileLoopNode doWhileLoopNode)
        {
            throw new NotImplementedException();
        }

        public override void Visit(WhileLoopNode whileLoopNode)
        {
            throw new NotImplementedException();
        }

        public override void Visit(ReturnNode returnNode)
        {
            throw new NotImplementedException();
        }

        public override void Visit(BreakNode breakNode)
        {
            throw new NotImplementedException();
        }

        public override void Visit(SkipNode nextNode)
        {
            throw new NotImplementedException();
        }

        public override void Visit(TableNode tableNode)
        {
            throw new NotImplementedException();
        }

        public override void Visit(IndexingNode indexingNode)
        {
            throw new NotImplementedException();
        }

        public override void Visit(IncrementationNode incrementationNode)
        {
            throw new NotImplementedException();
        }

        public override void Visit(DecrementationNode decrementationNode)
        {
            throw new NotImplementedException();
        }

        public override void Visit(UndefNode undefNode)
        {
            if (undefNode.Right is VariableNode vn)
            {
                CurrentScope.Undefine(vn.Identifier);
            }
            else
            {
                // todo
            }
        }

        public override void Visit(EachLoopNode eachLoopNode)
        {
            throw new NotImplementedException();
        }

        public override void Visit(ParameterListNode parameterListNode)
        {
            throw new NotImplementedException();
        }

        public override void Visit(ArgumentListNode argumentListNode)
        {
            throw new NotImplementedException();
        }
    }
}