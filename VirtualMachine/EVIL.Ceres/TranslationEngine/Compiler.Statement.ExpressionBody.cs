namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(ExpressionBodyStatement expressionBodyStatement)
    {
        Visit(expressionBodyStatement.Expression);
        Chunk.CodeGenerator.Emit(OpCode.RET);
    }
}