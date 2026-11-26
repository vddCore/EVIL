namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(SkipStatement skipStatement)
    {
        Chunk.CodeGenerator.Emit(OpCode.JUMP, Loop.StartLabel);
        _isUnreachable = true;
    }
}