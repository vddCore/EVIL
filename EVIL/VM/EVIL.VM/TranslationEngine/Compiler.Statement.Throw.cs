namespace EVIL.VM.TranslationEngine;

using EVIL.Core.Grammar.AST.Statements;
using EVIL.VM.ExecutionEngine.Diagnostics;

public partial class Compiler
{
    protected override void Visit(ThrowStatement throwStatement)
    {
        Chunk.MarkThrowing();
            
        Visit(throwStatement.ThrownExpression);
        Chunk.CodeGenerator.Emit(OpCode.THROW);
        _isUnreachable = true;
    }
}