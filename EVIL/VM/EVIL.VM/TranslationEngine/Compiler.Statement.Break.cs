namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(BreakStatement breakStatement)
    {
        Chunk.CodeGenerator.Emit(OpCode.JUMP, Loop.EndLabel);
        _isUnreachable = true;
    }
}