namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(WhileStatement whileStatement)
    {
        InNewLoopDo(Loop.LoopKind.While, () =>
        {
            Visit(whileStatement.Condition);
            Chunk.CodeGenerator.Emit(OpCode.FJMP, Loop.EndLabel);

            Visit(whileStatement.Statement);
            Chunk.CodeGenerator.Emit(OpCode.JUMP, Loop.StartLabel);

            Chunk.UpdateLabel(Loop.EndLabel, Chunk.CodeGenerator.IP);
        }, false);
    }
}