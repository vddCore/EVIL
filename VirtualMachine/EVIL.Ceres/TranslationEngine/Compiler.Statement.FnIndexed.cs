namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Diagnostics;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(FnIndexedStatement fnIndexedStatement)
    {
        var indexedChunkName = fnIndexedStatement.Indexer.BuildChainStringRepresentation();
            
        var id = InNamedSubChunkDo(
            indexedChunkName,
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
            }, out var wasExistingReplaced, 
               out var replacedChunk,
               allocateNewVersionOnRedefinition: true
        );
            
        if (wasExistingReplaced)
        {
            Log.Report(
                CompilerDiagnostics.FnIndexedStatementRedefinedExistingChunk.Create(
                    GetLocation(fnIndexedStatement),
                    [new RelatedLocation(
                        new SourceLocation(
                            replacedChunk.DebugDatabase.DefinedInFile,
                            replacedChunk.DebugDatabase.DefinedOnLine, 
                            1
                        ), 
                        "Previously defined here"
                    )],
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

        if (!fnIndexedStatement.Indexer.CreateTableIfNonExistent
            && fnIndexedStatement.Indexer.KeyExpression is StringConstant { IsInterpolated: false } sc)
        {
            var strId = (int)Chunk.StringPool.FetchOrAdd(sc.Value);
            Chunk.CodeGenerator.EmitSetProp(strId);
        }
        else
        {
            Visit(fnIndexedStatement.Indexer.KeyExpression);
            Chunk.CodeGenerator.Emit(OpCode.ELSET);
        }
    }
}