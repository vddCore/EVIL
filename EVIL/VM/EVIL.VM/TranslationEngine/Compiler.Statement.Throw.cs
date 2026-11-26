namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Grammar.AST.Statements;

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