namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Grammar.AST.Expressions;

public partial class Compiler
{
    protected override void Visit(ConditionalExpression conditionalExpression)
    {
        var trueLabel = Chunk.CreateLabel();
        var elseLabel = Chunk.CreateLabel();
        var endLabel = Chunk.CreateLabel();

        EmitConditionBranch(conditionalExpression.Condition, trueLabel, elseLabel, true);

        Chunk.UpdateLabel(trueLabel, Chunk.CodeGenerator.IP);
        Visit(conditionalExpression.TrueExpression);
        Chunk.CodeGenerator.Emit(OpCode.JUMP, endLabel);

        Chunk.UpdateLabel(elseLabel, Chunk.CodeGenerator.IP);
        Visit(conditionalExpression.FalseExpression);

        Chunk.UpdateLabel(endLabel, Chunk.CodeGenerator.IP);
    }
}