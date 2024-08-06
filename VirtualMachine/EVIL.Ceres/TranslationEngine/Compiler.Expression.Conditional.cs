namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

public partial class Compiler
{
    public override void Visit(ConditionalExpression conditionalExpression)
    {
        var elseLabel = Chunk.CreateLabel();
        var endLabel = Chunk.CreateLabel();

        Visit(conditionalExpression.Condition);
        Chunk.CodeGenerator.Emit(OpCode.FJMP, elseLabel);

        Visit(conditionalExpression.TrueExpression);
        Chunk.CodeGenerator.Emit(OpCode.JUMP, endLabel);
        Chunk.UpdateLabel(elseLabel, Chunk.CodeGenerator.IP);

        Visit(conditionalExpression.FalseExpression);
        Chunk.UpdateLabel(endLabel, Chunk.CodeGenerator.IP);
    }
}