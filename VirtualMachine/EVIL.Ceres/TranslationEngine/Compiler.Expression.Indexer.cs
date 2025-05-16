namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

public partial class Compiler
{
    protected override void Visit(IndexerExpression indexerExpression)
    {
        if (indexerExpression.IsConditional)
        {
            var endIndexerLabel = Chunk.CreateLabel();
            var indexLabel = Chunk.CreateLabel();
            
            indexerExpression.SetTag("end_label", endIndexerLabel);
            Visit(indexerExpression.Indexable);

            if (OptimizeCodeGeneration)
            {
                var parent = indexerExpression;
                while (parent.Parent is IndexerExpression { IsConditional: true } outer)
                {
                    parent = outer;
                }

                endIndexerLabel = parent.GetTag<int>("end_label");
            }

            Chunk.CodeGenerator.Emit(OpCode.DUP);
            Chunk.CodeGenerator.Emit(OpCode.VJMP, indexLabel);
            Chunk.CodeGenerator.Emit(OpCode.LDNIL);
            Chunk.CodeGenerator.Emit(OpCode.JUMP, endIndexerLabel);
            Chunk.UpdateLabel(indexLabel, Chunk.CodeGenerator.IP);
            Visit(indexerExpression.KeyExpression);
            Chunk.CodeGenerator.Emit(OpCode.INDEX);
            Chunk.UpdateLabel(endIndexerLabel, Chunk.CodeGenerator.IP);
        }
        else
        {
            Visit(indexerExpression.Indexable);
            Visit(indexerExpression.KeyExpression);
            Chunk.CodeGenerator.Emit(OpCode.INDEX);
        }
    }
}