using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(CoalescingExpression coalescingExpression)
        {
            var valueNotNullLabel = Chunk.CreateLabel();
            
            Visit(coalescingExpression.Left);
            Chunk.CodeGenerator.Emit(OpCode.DUP);
            Chunk.CodeGenerator.Emit(OpCode.LDNIL);
            Chunk.CodeGenerator.Emit(OpCode.CNE);
            Chunk.CodeGenerator.Emit(OpCode.TJMP, valueNotNullLabel);
            Chunk.CodeGenerator.Emit(OpCode.POP);
            Visit(coalescingExpression.Right);
            
            Chunk.UpdateLabel(
                valueNotNullLabel, 
                Chunk.CodeGenerator.IP
            );
        }
    }
}