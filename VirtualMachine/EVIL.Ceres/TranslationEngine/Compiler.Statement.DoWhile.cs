namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(DoWhileStatement doWhileStatement)
    {
        InNewLoopDo(Loop.LoopKind.DoWhile, () =>
        {
            Visit(doWhileStatement.Statement);

            Visit(doWhileStatement.Condition);
            Chunk.CodeGenerator.Emit(OpCode.TJMP, Loop.StartLabel);

            Chunk.UpdateLabel(Loop.EndLabel, Chunk.CodeGenerator.IP);
        }, false);
    }
}