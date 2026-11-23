namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;

public partial class Compiler
{
    protected override void Visit(IndexerExpression indexerExpression)
    {
        if (indexerExpression.IsConditional)
        {
            var isPartOfOuterChain = false;
            int endIndexerLabel = 0;

            if (Options.OptimizeCodeGeneration 
                && indexerExpression.Parent is IndexerExpression outer 
                && outer.IsConditional 
                && outer.Indexable == indexerExpression)
            {
                var root = outer;
                while (root.Parent is IndexerExpression p 
                       && p.IsConditional 
                       && p.Indexable == root)
                {
                    root = p;
                }

                if (root.HasTag("safe_nav_end_label"))
                {
                    isPartOfOuterChain = true;
                    endIndexerLabel = root.GetTag<int>("safe_nav_end_label");
                }
            }

            if (!isPartOfOuterChain)
            {
                endIndexerLabel = Chunk.CreateLabel();
                indexerExpression.SetTag("safe_nav_end_label", endIndexerLabel);
            }

            var indexLabel = Chunk.CreateLabel();
            Visit(indexerExpression.Indexable);

            Chunk.CodeGenerator.Emit(OpCode.DUP);
            Chunk.CodeGenerator.Emit(OpCode.VJMP, indexLabel);
            Chunk.CodeGenerator.Emit(OpCode.JUMP, endIndexerLabel);
            Chunk.UpdateLabel(indexLabel, Chunk.CodeGenerator.IP);

            if (!indexerExpression.CreateTableIfNonExistent 
                && indexerExpression.KeyExpression is StringConstant { IsInterpolated: false } sc)
            {
                var strId = (int)Chunk.StringPool.FetchOrAdd(sc.Value);
                Chunk.CodeGenerator.EmitGetProp(strId);
            }
            else
            {
                Visit(indexerExpression.KeyExpression);
                Chunk.CodeGenerator.Emit(
                    indexerExpression.CreateTableIfNonExistent 
                        ? OpCode.INOCT 
                        : OpCode.INDEX
                );
            }
            
            if (!isPartOfOuterChain)
            {
                Chunk.UpdateLabel(endIndexerLabel, Chunk.CodeGenerator.IP);
            }
        }
        else
        {
            Visit(indexerExpression.Indexable);

            if (!indexerExpression.CreateTableIfNonExistent
                && indexerExpression.KeyExpression is StringConstant { IsInterpolated: false } sc)
            {
                var strId = (int)Chunk.StringPool.FetchOrAdd(sc.Value);
                Chunk.CodeGenerator.EmitGetProp(strId);
            }
            else
            {
                Visit(indexerExpression.KeyExpression);
                Chunk.CodeGenerator.Emit(
                    indexerExpression.CreateTableIfNonExistent 
                        ? OpCode.INOCT 
                        : OpCode.INDEX
                );
            }
        }
    }
}