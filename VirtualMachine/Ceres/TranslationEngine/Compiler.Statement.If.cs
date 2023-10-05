using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Statements;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(IfStatement ifStatement)
        {
            var statementEnd = Chunk.CreateLabel();

            for (var i = 0; i < ifStatement.Conditions.Count; i++)
            {
                var caseEnd = Chunk.CreateLabel();

                Visit(ifStatement.Conditions[i]);
                Chunk.CodeGenerator.Emit(
                    OpCode.FJMP,
                    caseEnd
                );

                Visit(ifStatement.Statements[i]);
                Chunk.CodeGenerator.Emit(
                    OpCode.JUMP,
                    statementEnd
                );

                Chunk.UpdateLabel(caseEnd, Chunk.CodeGenerator.IP);
            }

            if (ifStatement.ElseBranch != null)
            {
                Visit(ifStatement.ElseBranch);
            }
            else
            {
                Chunk.CodeGenerator.Emit(OpCode.NOOP);
            }

            Chunk.UpdateLabel(
                statementEnd,
                Chunk.CodeGenerator.IP
            );
        }
    }
}