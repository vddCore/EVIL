using System;
using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler
    {
        private readonly List<Type> _allowedExpressionStatementTypes = new()
        {
            typeof(AssignmentExpression),
            typeof(FunctionCallExpression),
            typeof(IncrementationExpression),
            typeof(DecrementationExpression),
        };
        
        public override void Visit(ExpressionStatement expressionStatement)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            if (!_allowedExpressionStatementTypes.Contains(expressionStatement.Expression.GetType()))
            {
                throw new CompilerException(
                    "Only assignment, call, increment or decrement expressions are allowed to be used as statements.",
                    expressionStatement.Line,
                    expressionStatement.Column
                );
            }
            
            Visit(expressionStatement.Expression);
            cg.Emit(OpCode.POP);
        }
    }
}