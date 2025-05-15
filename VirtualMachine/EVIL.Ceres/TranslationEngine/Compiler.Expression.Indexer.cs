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
            
            Visit(indexerExpression.Indexable);
            Chunk.CodeGenerator.Emit(OpCode.DUP);
            Chunk.CodeGenerator.Emit(OpCode.LDNIL);
            Chunk.CodeGenerator.Emit(OpCode.CEQ);
            Chunk.CodeGenerator.Emit(OpCode.FJMP, indexLabel);
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