namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

public partial class Compiler
{
    public override void Visit(IndexerExpression indexerExpression)
    {
        Visit(indexerExpression.Indexable);
        Visit(indexerExpression.KeyExpression);
        Chunk.CodeGenerator.Emit(OpCode.INDEX);
    }
}