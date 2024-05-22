using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

namespace EVIL.Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(IsExpression isExpression)
        {
            Visit(isExpression.Target);
            Chunk.CodeGenerator.Emit(OpCode.TYPE);
            Chunk.CodeGenerator.Emit(OpCode.LDTYPE, (int)isExpression.Type.Value);
            Chunk.CodeGenerator.Emit(isExpression.Invert ? OpCode.CNE : OpCode.CEQ);
        }
    }
}