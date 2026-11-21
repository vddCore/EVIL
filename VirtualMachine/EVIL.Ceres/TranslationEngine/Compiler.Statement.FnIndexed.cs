namespace EVIL.Ceres.TranslationEngine;

using System.Linq;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.CommonTypes.Diagnostics;
using EVIL.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(FnIndexedStatement fnIndexedStatement)
    {
        var indexedChunkName = fnIndexedStatement.Indexer.BuildChainStringRepresentation();
            
        if (Chunk.SubChunks.FirstOrDefault(x => x.Name.StartsWith(indexedChunkName)) != null)
        {
            var postfix = Chunk.SubChunks.Count(x => x.Name.StartsWith(indexedChunkName));
            indexedChunkName = $"{indexedChunkName}_{postfix}";
        }
            
        var id = InNamedSubChunkDo(
            $"{indexedChunkName}",
            () =>
            {
                InNewClosedScopeDo(() =>
                {
                    Chunk.DebugDatabase.DefinedOnLine = fnIndexedStatement.Line;

                    if (fnIndexedStatement.ParameterList != null)
                    {
                        Visit(fnIndexedStatement.ParameterList);
                    }

                    Visit(fnIndexedStatement.InnerStatement);
                    FinalizeChunk();
                        
                    if (fnIndexedStatement.AttributeList != null)
                    {
                        Visit(fnIndexedStatement.AttributeList);
                    }
                });
            }, out var wasExistingReplaced, out var replacedChunk
        );
            
        if (wasExistingReplaced)
        {
            Log.Report(
                CompilerDiagnostics.FnIndexedStatementRedefinedExistingChunk.Create(
                    GetLocation(fnIndexedStatement),
                    [new RelatedLocation(new SourceLocation(replacedChunk.DebugDatabase.DefinedInFile, replacedChunk.DebugDatabase.DefinedOnLine, 1), "Previously defined here")],
                    replacedChunk.Name,
                    fnIndexedStatement.Line,
                    replacedChunk.DebugDatabase.DefinedInFile,
                    replacedChunk.DebugDatabase.DefinedOnLine
                )
            );
        }
            
        Chunk.CodeGenerator.Emit(
            OpCode.LDCNK,
            id
        );

        Visit(fnIndexedStatement.Indexer.Indexable);
        Visit(fnIndexedStatement.Indexer.KeyExpression);
            
        Chunk.CodeGenerator.Emit(OpCode.ELSET);
    }
}