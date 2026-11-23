namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Diagnostics;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(FnTargetedStatement fnTargetedStatement)
    {
        string primaryName;
            
        switch (fnTargetedStatement.PrimaryTarget)
        {
            case SelfExpression:
            {
                primaryName = "self";
                break;
            }

            case IdentifierNode identifierNode:
            {
                primaryName = identifierNode.Name;
                break;
            }

            default:
            {
                Log.TerminateWithFatal(
                    CompilerDiagnostics.UnexpectedSyntaxNode.Create(
                        GetLocation(fnTargetedStatement.PrimaryTarget),
                        fnTargetedStatement.PrimaryTarget.GetType().Name
                    )
                );

                return;
            }
        }
            
        var targetedChunkName = $"{primaryName}::{fnTargetedStatement.SecondaryIdentifier.Name}";
            
        var id = InNamedSubChunkDo(
            targetedChunkName,
            () =>
            {
                InNewClosedScopeDo(() =>
                {
                    Chunk.DebugDatabase.DefinedOnLine = fnTargetedStatement.Line;
                    Chunk.MarkSelfAware();
                    Chunk.AllocateParameter(); /* implicit `self' */ 

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
            }, out var wasExistingReplaced, 
               out var replacedChunk,
               allocateNewVersionOnRedefinition: true
        );
            
        if (wasExistingReplaced)
        {
            Log.Report(
                CompilerDiagnostics.FnTargetedStatementRedefinedExistingChunk.Create(
                    GetLocation(fnTargetedStatement),
                    [new RelatedLocation(
                        new SourceLocation(
                            replacedChunk.DebugDatabase.DefinedInFile, 
                            replacedChunk.DebugDatabase.DefinedOnLine,
                            1
                        ), 
                        "Previously defined here"
                    )],
                    replacedChunk.Name,
                    fnTargetedStatement.Line,
                    replacedChunk.DebugDatabase.DefinedInFile,
                    replacedChunk.DebugDatabase.DefinedOnLine
                )
            );
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
            
        var identifierNameId = (int)Chunk.StringPool.FetchOrAdd(
            fnTargetedStatement.SecondaryIdentifier.Name
        );
        
        Chunk.CodeGenerator.EmitSetProp(identifierNameId);
    }
}