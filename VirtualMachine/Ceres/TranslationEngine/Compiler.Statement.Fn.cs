using Ceres.ExecutionEngine.Diagnostics;
using Ceres.TranslationEngine.Diagnostics;
using EVIL.Grammar.AST.Statements.TopLevel;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(FnStatement fnStatement)
        {
            InTopLevelChunkDo(() =>
            {
                InNewClosedScopeDo(() =>
                {
                    Chunk.DebugDatabase.DefinedOnLine = fnStatement.Line;

                    InNewLocalScopeDo(() =>
                    {
                        Visit(fnStatement.ParameterList);
                        Visit(fnStatement.Statement);
                    });

                    if (Chunk.CodeGenerator.TryPeekOpCode(out var opCode))
                    {
                        if (opCode == OpCode.RET || opCode == OpCode.TAILINVOKE)
                        {
                            foreach (var attr in fnStatement.Attributes)
                                Visit(attr);

                            return;
                        }
                    }

                    /* Either we have no instructions in chunk, or it's not a RET. */
                    Chunk.CodeGenerator.Emit(OpCode.LDNIL);
                    Chunk.CodeGenerator.Emit(OpCode.RET);

                    foreach (var attr in fnStatement.Attributes)
                        Visit(attr);
                });
            }, fnStatement.Identifier.Name, out var wasExistingReplaced, out var replacedChunk);

            if (wasExistingReplaced)
            {
                Log.EmitWarning(
                    $"Redefining chunk '{replacedChunk.Name}' previously defined in {replacedChunk.DebugDatabase.DefinedInFile} on line {replacedChunk.DebugDatabase.DefinedOnLine}.",
                    CurrentFileName,
                    EvilMessageCode.FnStatementRedefinedExistingChunk,
                    fnStatement.Line,
                    fnStatement.Column
                );
            }
        }
    }
}