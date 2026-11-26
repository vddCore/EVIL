namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(IfStatement ifStatement)
    {
        var statementEnd = Chunk.CreateLabel();
            
        for (var i = 0; i < ifStatement.Conditions.Count; i++)
        {
            var statementStart = Chunk.CreateLabel();
            var caseEnd = Chunk.CreateLabel();
            var currentConditionExpression = ifStatement.Conditions[i];

            EmitConditionBranch(currentConditionExpression, statementStart, caseEnd, true);

            Chunk.UpdateLabel(statementStart, Chunk.CodeGenerator.IP);

            Visit(ifStatement.Statements[i]);
            if (!_isUnreachable)
            {
                Chunk.CodeGenerator.Emit(
                    OpCode.JUMP,
                    statementEnd
                );
            }

            Chunk.UpdateLabel(caseEnd, Chunk.CodeGenerator.IP);
        }

        if (ifStatement.ElseBranch != null)
        {
            Visit(ifStatement.ElseBranch);
        }

        Chunk.UpdateLabel(
            statementEnd,
            Chunk.CodeGenerator.IP
        );
    }
}