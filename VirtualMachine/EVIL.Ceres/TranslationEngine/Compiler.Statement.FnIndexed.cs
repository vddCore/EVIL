using System.Linq;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.TranslationEngine.Diagnostics;
using EVIL.Grammar.AST.Statements;

namespace EVIL.Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(FnIndexedStatement fnIndexedStatement)
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
                Log.TerminateWithInternalFailure<object>(
                    $"Indexed function '{replacedChunk.Name}' defined on line {fnIndexedStatement.Line} has " +
                    $"re-defined a previously defined function of the same name in {replacedChunk.DebugDatabase.DefinedInFile} on line {fnIndexedStatement.Line}.",
                    CurrentFileName,
                    EvilMessageCode.FnIndexedStatementRedefinedExistingChunk,
                    fnIndexedStatement.Line,
                    fnIndexedStatement.Column
                );

                return;
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
}