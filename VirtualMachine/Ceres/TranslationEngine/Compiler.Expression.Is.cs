using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(IsExpression isExpression)
        {
            Visit(isExpression.Left);
            Chunk.CodeGenerator.Emit(OpCode.TYPE);
            Chunk.CodeGenerator.Emit(OpCode.LDTYPE, (int)isExpression.Right.Value);
            Chunk.CodeGenerator.Emit(OpCode.CEQ);
        }
    }
}