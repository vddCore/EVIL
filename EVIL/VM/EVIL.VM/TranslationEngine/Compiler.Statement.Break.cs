namespace EVIL.VM.TranslationEngine;

using EVIL.Core.Grammar.AST.Statements;
using EVIL.VM.ExecutionEngine.Diagnostics;

public partial class Compiler
{
    protected override void Visit(BreakStatement breakStatement)
    {
        Chunk.CodeGenerator.Emit(OpCode.JUMP, Loop.EndLabel);
        _isUnreachable = true;
    }
}