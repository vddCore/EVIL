namespace EVIL.VM.TranslationEngine;

using EVIL.Core.Grammar.AST.Statements;
using EVIL.VM.ExecutionEngine.Diagnostics;

public partial class Compiler
{
    protected override void Visit(WhileStatement whileStatement)
    {
        InNewLoopDo(Loop.LoopKind.While, () =>
        {
            var bodyLabel = Chunk.CreateLabel();
            EmitConditionBranch(whileStatement.Condition, bodyLabel, Loop.EndLabel, true);

            Chunk.UpdateLabel(bodyLabel, Chunk.CodeGenerator.IP);
            Visit(whileStatement.Statement);
            if (!_isUnreachable)
            {
                Chunk.CodeGenerator.Emit(OpCode.JUMP, Loop.StartLabel);
            }

            Chunk.UpdateLabel(Loop.EndLabel, Chunk.CodeGenerator.IP);
        }, false);
    }
}