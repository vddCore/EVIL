using Ceres.ExecutionEngine.Diagnostics;
using Ceres.TranslationEngine.Diagnostics;
using EVIL.Grammar.AST.Statements;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(FnTargetedStatement fnTargetedStatement)
        {
            var id = InNamedSubChunkDo(
                $"{fnTargetedStatement.PrimaryIdentifier.Name}::{fnTargetedStatement.SecondaryIdentifier.Name}",
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
                Log.EmitWarning(
                    $"Targeted function '{replacedChunk.Name}' defined on line {fnTargetedStatement.Line} is " +
                    $"re-defining a previously defined function of the same name in {replacedChunk.DebugDatabase.DefinedInFile} on line {fnTargetedStatement.Line}.",
                    CurrentFileName,
                    EvilMessageCode.FnTargetedStatementRedefinedExistingChunk,
                    fnTargetedStatement.Line,
                    fnTargetedStatement.Column
                );
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