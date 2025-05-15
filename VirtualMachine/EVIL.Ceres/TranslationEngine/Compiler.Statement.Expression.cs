namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(ExpressionStatement expressionStatement)
    {
        Visit(expressionStatement.Expression);

        if (expressionStatement.Expression is not AssignmentExpression)
        {
            Chunk.CodeGenerator.Emit(OpCode.POP);
        }
    }
}