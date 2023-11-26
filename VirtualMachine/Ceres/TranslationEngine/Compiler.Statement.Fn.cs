using Ceres.ExecutionEngine.Diagnostics;
using Ceres.TranslationEngine.Diagnostics;
using Ceres.TranslationEngine.Scoping;
using EVIL.Grammar.AST.Statements.TopLevel;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(FnStatement fnStatement)
        {
            var localId = -1;
            if (fnStatement.IsLocalDefintion)
            {
                localId = Chunk.AllocateLocal();
                CurrentScope.DefineLocal(
                    fnStatement.Identifier.Name,
                    localId,
                    false,
                    fnStatement.Line,
                    fnStatement.Column
                );
            }

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
            
            if (fnStatement.IsLocalDefintion)
            {
                Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, localId);
            }
            else
            {
                Chunk.CodeGenerator.Emit(
                    OpCode.LDSTR,
                    (int)Chunk.StringPool.FetchOrAdd(fnStatement.Identifier.Name)
                );
                Chunk.CodeGenerator.Emit(OpCode.SETGLOBAL);
            }
        }
    }
}