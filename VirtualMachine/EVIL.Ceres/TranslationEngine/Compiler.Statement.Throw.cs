namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Statements;

public partial class Compiler
{
    public override void Visit(ThrowStatement throwStatement)
    {
        Chunk.MarkThrowing();
            
        Visit(throwStatement.ThrownExpression);
        Chunk.CodeGenerator.Emit(OpCode.THROW);
    }
}