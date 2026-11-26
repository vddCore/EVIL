namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(ForStatement forStatement)
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
                {
                    if (_isUnreachable)
                    {
                        break;
                    }
                    Visit(iterationStatement);
                }

                if (!_isUnreachable)
                {
                    Chunk.CodeGenerator.Emit(OpCode.JUMP, Loop.ExtraLabel);
                }
                Chunk.UpdateLabel(Loop.EndLabel, Chunk.CodeGenerator.IP);
            }, true);
        });
    }
}