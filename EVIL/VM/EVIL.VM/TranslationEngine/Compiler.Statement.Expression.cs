namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(ExpressionStatement expressionStatement)
    {
        Visit(expressionStatement.Expression);

        if (expressionStatement.Expression is not (AssignmentExpression or IncrementationExpression or DecrementationExpression))
        {
            Chunk.CodeGenerator.Emit(OpCode.POP);
        }
    }
}