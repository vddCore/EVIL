using System;
using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Grammar.Traversal;

namespace EVIL.Compiler
{
    public class Compiler : AstVisitor
    {
        public Stack<Prototype> PrototypeDefinitionStack { get; } = new();

        public Compiler()
        {
            PrototypeDefinitionStack.Push(new Prototype());
        }

        public override void Visit(Program program)
        {
            foreach (var node in program.Statements)
                Visit(node);
        }

        public override void Visit(BlockStatement blockStatement)
        {
        }

        public override void Visit(ConditionalExpression conditionalExpression)
        {
        }

        public override void Visit(NumberExpression numberExpression)
        {
        }

        public override void Visit(StringConstant stringConstant)
        {
        }

        public override void Visit(AssignmentExpression assignmentExpression)
        {
        }

        public override void Visit(BinaryExpression binaryExpression)
        {
        }

        public override void Visit(UnaryExpression unaryExpression)
        {
        }

        public override void Visit(VariableReference variableReference)
        {
        }

        public override void Visit(VariableDefinition variableDefinition)
        {
        }

        public override void Visit(FunctionDefinition functionDefinition)
        {
            
        }

        public override void Visit(FunctionExpression functionExpression)
        {
        }

        public override void Visit(FunctionCallExpression functionCallExpression)
        {
        }

        public override void Visit(IfStatement ifStatement)
        {
        }

        public override void Visit(ExitStatement exitStatement)
        {
        }

        public override void Visit(ForStatement forStatement)
        {
        }

        public override void Visit(DoWhileStatement doWhileStatement)
        {
        }

        public override void Visit(WhileStatement whileStatement)
        {
        }

        public override void Visit(ReturnStatement returnStatement)
        {
        }

        public override void Visit(BreakStatement breakStatement)
        {
        }

        public override void Visit(SkipStatement nextStatement)
        {
        }

        public override void Visit(TableExpression tableExpression)
        {
        }

        public override void Visit(IndexerExpression indexerExpression)
        {
        }

        public override void Visit(IncrementationExpression incrementationExpression)
        {
        }

        public override void Visit(DecrementationExpression decrementationExpression)
        {
        }

        public override void Visit(UndefStatement undefStatement)
        {
        }

        public override void Visit(EachStatement eachStatement)
        {
        }

        public override void Visit(ExpressionStatement expressionStatement)
        {
        }

        public override void Visit(ExtraArgumentsExpression extraArgumentsExpression)
        {
        }
    }
}