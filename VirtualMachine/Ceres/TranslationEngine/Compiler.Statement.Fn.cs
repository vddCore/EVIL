using Ceres.ExecutionEngine.Diagnostics;
using Ceres.TranslationEngine.Diagnostics;
using EVIL.Grammar.AST.Statements.TopLevel;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(FnStatement fnStatement)
        {
            var id = InNamedSubChunkDo(fnStatement.Identifier.Name, () =>
            {
                InNewClosedScopeDo(() =>
                {
                    Chunk.DebugDatabase.DefinedOnLine = fnStatement.Line;

                    InNewLocalScopeDo(() =>
                    {
                        Visit(fnStatement.ParameterList);
                        Visit(fnStatement.Statement);
                    });

                    FinalizeChunk();
                    
                    foreach (var attr in fnStatement.Attributes)
                        Visit(attr);
                });
            }, out var wasExistingReplaced, out var replacedChunk);

            if (wasExistingReplaced)
            {
                Log.EmitWarning(
                    $"Named function '{replacedChunk.Name}' defined on line {fnStatement.Line} is " +
                    $"re-defining a previously defined function of the same name in {replacedChunk.DebugDatabase.DefinedInFile} on line {fnStatement.Line}.",
                    CurrentFileName,
                    EvilMessageCode.FnStatementRedefinedExistingChunk,
                    fnStatement.Line,
                    fnStatement.Column
                );
            }

            Chunk.CodeGenerator.Emit(OpCode.LDCNK, id);
            Chunk.CodeGenerator.Emit(
                OpCode.LDSTR, 
                (int)Chunk.StringPool.FetchOrAdd(fnStatement.Identifier.Name)
            );
            Chunk.CodeGenerator.Emit(OpCode.SETGLOBAL);
        }
    }
}