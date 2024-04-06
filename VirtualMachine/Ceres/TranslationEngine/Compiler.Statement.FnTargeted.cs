using System.Linq;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.TranslationEngine.Diagnostics;
using EVIL.Grammar.AST.Statements;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(FnTargetedStatement fnTargetedStatement)
        {
            var targetedChunkName = $"{fnTargetedStatement.PrimaryIdentifier.Name}::{fnTargetedStatement.SecondaryIdentifier.Name}";
            
            if (Chunk.SubChunks.FirstOrDefault(x => x.Name.StartsWith(targetedChunkName)) != null)
            {
                var postfix = Chunk.SubChunks.Count(x => x.Name.StartsWith(targetedChunkName));
                targetedChunkName = $"{targetedChunkName}_{postfix}";
            }
            
            var id = InNamedSubChunkDo(
                $"{targetedChunkName}",
                () =>
                {
                    InNewClosedScopeDo(() =>
                    {
                        Chunk.DebugDatabase.DefinedOnLine = fnTargetedStatement.Line;
                        Chunk.MarkSelfAware();
                        /* implicit `self' */ Chunk.AllocateParameter();

                        Visit(fnTargetedStatement.ParameterList);
                        Visit(fnTargetedStatement.Statement);
                        
                        FinalizeChunk();
                    });
                }, out var wasExistingReplaced, out var replacedChunk
            );
            
            if (wasExistingReplaced)
            {
                Log.TerminateWithInternalFailure<object>(
                    $"Targeted function '{replacedChunk.Name}' defined on line {fnTargetedStatement.Line} has " +
                    $"re-defined a previously defined function of the same name in {replacedChunk.DebugDatabase.DefinedInFile} on line {fnTargetedStatement.Line}.",
                    CurrentFileName,
                    EvilMessageCode.FnTargetedStatementRedefinedExistingChunk,
                    fnTargetedStatement.Line,
                    fnTargetedStatement.Column
                );

                return;
            }
            
            Chunk.CodeGenerator.Emit(
                OpCode.LDCNK,
                id
            );
            
            EmitVarGet(fnTargetedStatement.PrimaryIdentifier.Name);
            
            Chunk.CodeGenerator.Emit(
                OpCode.LDSTR,
                (int)Chunk.StringPool.FetchOrAdd(fnTargetedStatement.SecondaryIdentifier.Name)
            );
            
            Chunk.CodeGenerator.Emit(OpCode.ELSET);
        }
    }
}