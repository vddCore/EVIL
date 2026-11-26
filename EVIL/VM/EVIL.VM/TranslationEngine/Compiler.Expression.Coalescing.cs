namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Grammar.AST.Expressions;

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