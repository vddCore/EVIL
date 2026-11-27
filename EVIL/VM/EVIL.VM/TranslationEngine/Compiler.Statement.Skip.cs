namespace EVIL.VM.TranslationEngine;

using EVIL.Core.Grammar.AST.Statements;
using EVIL.VM.ExecutionEngine.Diagnostics;

public partial class Compiler
{
    protected override void Visit(SkipStatement skipStatement)
    {
        Chunk.CodeGenerator.Emit(OpCode.JUMP, Loop.StartLabel);
        _isUnreachable = true;
    }
}