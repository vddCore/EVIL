namespace EVIL.Ceres.TranslationEngine;

using System.Linq;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.TranslationEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(FnTargetedStatement fnTargetedStatement)
    {
        string primaryName;
            
        if (fnTargetedStatement.PrimaryTarget is SelfExpression)
        {
            primaryName = "self";
        }
        else if (fnTargetedStatement.PrimaryTarget is IdentifierNode identifierNode)
        {
            primaryName = identifierNode.Name;
        }
        else
        {
            Log.TerminateWithInternalFailure<object>(
                $"Expected a self-expression or an identifier found an unsupported node type {fnTargetedStatement.PrimaryTarget.GetType().FullName}.",
                CurrentFileName,
                EvilMessageCode.UnexpectedSyntaxNodeFound,
                fnTargetedStatement.Line,
                fnTargetedStatement.Column
            );

            return;
        }
            
        var targetedChunkName = $"{primaryName}::{fnTargetedStatement.SecondaryIdentifier.Name}";
            
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

                    if (fnTargetedStatement.ParameterList != null)
                    {
                        Visit(fnTargetedStatement.ParameterList);
                    }

                    Visit(fnTargetedStatement.InnerStatement);
                    FinalizeChunk();

                    if (fnTargetedStatement.AttributeList != null)
                    {
                        Visit(fnTargetedStatement.AttributeList);
                    }
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

        if (fnTargetedStatement.IsSelfTargeting)
        {
            Visit(fnTargetedStatement.PrimaryTarget);
        }
        else
        {
            EmitVarGet(primaryName);
        }
            
        Chunk.CodeGenerator.Emit(
            OpCode.LDSTR,
            (int)Chunk.StringPool.FetchOrAdd(fnTargetedStatement.SecondaryIdentifier.Name)
        );
            
        Chunk.CodeGenerator.Emit(OpCode.ELSET);
    }
}