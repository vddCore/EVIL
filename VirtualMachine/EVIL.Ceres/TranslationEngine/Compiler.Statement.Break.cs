namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Statements;

public partial class Compiler
{
    public override void Visit(BreakStatement breakStatement)
    {
        Chunk.CodeGenerator.Emit(OpCode.JUMP, Loop.EndLabel);
    }
}