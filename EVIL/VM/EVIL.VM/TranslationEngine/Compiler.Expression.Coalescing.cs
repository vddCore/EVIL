namespace EVIL.VM.TranslationEngine;

using EVIL.Core.Grammar.AST.Expressions;
using EVIL.VM.ExecutionEngine.Diagnostics;

public partial class Compiler
{
    protected override void Visit(CoalescingExpression coalescingExpression)
    {
        var valueNotNilLabel = Chunk.CreateLabel();
            
        Visit(coalescingExpression.Left);
        Chunk.CodeGenerator.Emit(OpCode.DUP);
        Chunk.CodeGenerator.Emit(OpCode.VJMP, valueNotNilLabel);
        Chunk.CodeGenerator.Emit(OpCode.POP);
        Visit(coalescingExpression.Right);
            
        Chunk.UpdateLabel(
            valueNotNilLabel, 
            Chunk.CodeGenerator.IP
        );
    }
}