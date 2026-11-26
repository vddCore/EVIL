namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(FnStatement fnStatement)
    {
        var localId = -1;
        if (fnStatement.IsLocalDefinition)
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
                    CompilerDiagnostics.DuplicateSymbolInScope.Create(
                        GetLocation(fnStatement.Identifier),
                        [new RelatedLocation(new SourceLocation(CurrentFileName, dse.Line, dse.Column), "Previously defined here")],
                        fnStatement.Identifier.Name,
                        dse.ExistingSymbol.TypeName,
                        dse.Line,
                        dse.Column
                    ),
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
                    
                if (fnStatement.AttributeList != null)
                {
                    Visit(fnStatement.AttributeList);
                }
            });
        }, out var wasExistingReplaced, out var replacedChunk);

        if (wasExistingReplaced)
        {
            Log.Report(
                CompilerDiagnostics.FnStatementRedefinedExistingChunk.Create(
                    GetLocation(fnStatement),
                    [new RelatedLocation(new SourceLocation(replacedChunk.DebugDatabase.DefinedInFile, replacedChunk.DebugDatabase.DefinedOnLine, 1), "Previously defined here")],
                    replacedChunk.Name,
                    fnStatement.Line,
                    replacedChunk.DebugDatabase.DefinedInFile,
                    replacedChunk.DebugDatabase.DefinedOnLine
                )
            );
        }

        Chunk.CodeGenerator.Emit(OpCode.LDCNK, id);
            
        if (fnStatement.IsLocalDefinition)
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