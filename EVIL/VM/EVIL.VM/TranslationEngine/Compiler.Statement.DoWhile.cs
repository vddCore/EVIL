namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.Core.Grammar.AST.Statements;

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
            if (!_isUnreachable)
            {
                EmitConditionBranch(doWhileStatement.Condition, bodyLabel, Loop.EndLabel, false);
            }

            Chunk.UpdateLabel(Loop.EndLabel, Chunk.CodeGenerator.IP);
        }, true);
    }
}