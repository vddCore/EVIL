namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(DoWhileStatement doWhileStatement)
    {
        InNewLoopDo(Loop.LoopKind.DoWhile, () =>
        {
            var bodyLabel = Loop.ExtraLabel;
            Chunk.UpdateLabel(bodyLabel, Chunk.CodeGenerator.IP);

            Visit(doWhileStatement.Statement);

            Chunk.UpdateLabel(Loop.StartLabel, Chunk.CodeGenerator.IP);
            EmitConditionBranch(doWhileStatement.Condition, bodyLabel, Loop.EndLabel, false);

            Chunk.UpdateLabel(Loop.EndLabel, Chunk.CodeGenerator.IP);
        }, true);
    }
}