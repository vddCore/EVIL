namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(BreakStatement breakStatement)
    {
        Chunk.CodeGenerator.Emit(OpCode.JUMP, Loop.EndLabel);
    }
}