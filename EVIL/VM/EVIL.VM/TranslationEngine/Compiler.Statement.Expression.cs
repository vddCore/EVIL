namespace EVIL.VM.TranslationEngine;

using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Grammar.AST.Statements;
using EVIL.VM.ExecutionEngine.Diagnostics;

public partial class Compiler
{
    protected override void Visit(ExpressionStatement expressionStatement)
    {
        Visit(expressionStatement.Expression);

        if (expressionStatement.Expression is not (AssignmentExpression or IncrementationExpression 
                                                                        or DecrementationExpression))
        {
            if (!ReferenceEquals(expressionStatement, _replExpressionStatement))
            {
                Chunk.CodeGenerator.Emit(OpCode.POP);
            }
        }
    }
}