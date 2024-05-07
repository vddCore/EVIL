using Ceres.ExecutionEngine.Diagnostics;
using Ceres.TranslationEngine.Diagnostics;
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

                try
                {
                    CurrentScope.DefineLocal(
                        fnStatement.Identifier.Name,
                        localId,
                        false,
                        fnStatement.Line,
                        fnStatement.Column
                    );
                }
                catch (DuplicateSymbolException dse)
                {
                    Log.TerminateWithFatal(
                        $"The symbol '{fnStatement.Identifier.Name}' already exists in this scope " +
                        $"and is a {dse.ExistingSymbol.TypeName} (previously defined on line {dse.Line}, column {dse.Column}).",
                        CurrentFileName,
                        EvilMessageCode.DuplicateSymbolInScope,
                        fnStatement.Identifier.Line,
                        fnStatement.Identifier.Column,
                        dse
                    );

                    // Return just in case - TerminateWithFatal should never return, ever.
                    return;
                }
            }

            var id = InNamedSubChunkDo(fnStatement.Identifier.Name, () =>
            {
                InNewClosedScopeDo(() =>
                {
                    Chunk.DebugDatabase.DefinedOnLine = fnStatement.Line;

                    InNewLocalScopeDo(() =>
                    {
                        if (fnStatement.ParameterList != null)
                        {
                            Visit(fnStatement.ParameterList);
                        }

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