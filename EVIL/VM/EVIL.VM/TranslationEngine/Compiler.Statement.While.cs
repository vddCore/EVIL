namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Grammar.AST.Statements;

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