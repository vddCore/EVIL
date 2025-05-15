namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

public partial class Compiler
{
    protected override void Visit(CoalescingExpression coalescingExpression)
    {
        var valueNotNilLabel = Chunk.CreateLabel();
            
        Visit(coalescingExpression.Left);
        Chunk.CodeGenerator.Emit(OpCode.DUP);
        Chunk.CodeGenerator.Emit(OpCode.LDNIL);
        Chunk.CodeGenerator.Emit(OpCode.CNE);
        Chunk.CodeGenerator.Emit(OpCode.TJMP, valueNotNilLabel);
        Chunk.CodeGenerator.Emit(OpCode.POP);
        Visit(coalescingExpression.Right);
            
        Chunk.UpdateLabel(
            valueNotNilLabel, 
            Chunk.CodeGenerator.IP
        );
    }
}