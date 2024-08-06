namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Statements;

public partial class Compiler
{
    public override void Visit(ForStatement forStatement)
    {
        InNewLocalScopeDo(() =>
        {
            foreach (var statement in forStatement.Assignments)
                Visit(statement);

            InNewLoopDo(Loop.LoopKind.For, () =>
            {
                Visit(forStatement.Condition);
                Chunk.CodeGenerator.Emit(OpCode.FJMP, Loop.EndLabel);

                Visit(forStatement.Statement);
                Chunk.UpdateLabel(Loop.StartLabel, Chunk.CodeGenerator.IP);

                foreach (var iterationStatement in forStatement.IterationStatements)
                    Visit(iterationStatement);

                Chunk.CodeGenerator.Emit(OpCode.JUMP, Loop.ExtraLabel);
                Chunk.UpdateLabel(Loop.EndLabel, Chunk.CodeGenerator.IP);
            }, true);
        });
    }
}