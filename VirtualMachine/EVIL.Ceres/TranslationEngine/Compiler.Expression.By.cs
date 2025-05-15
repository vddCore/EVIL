namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.TranslationEngine.Diagnostics;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;

public partial class Compiler
{
    protected override void Visit(ByExpression byExpression)
    {
        Visit(byExpression.Qualifier);

        var arms = byExpression.Arms;
        var elseArm = byExpression.ElseArm;

        var labels = new int[arms.Count];
        for (var i = 0; i < arms.Count; i++)
        {
            labels[i] = Chunk.CreateLabel();
        }

        var endLabel = Chunk.CreateLabel();

        for (var i = 0; i < arms.Count; i++)
        {
            if (i < arms.Count - 1)
            {
                Chunk.CodeGenerator.Emit(OpCode.DUP);
            }
                
            Visit(arms[i].Selector);

            Chunk.CodeGenerator.Emit(arms[i].DeepEquality ? OpCode.DEQ : OpCode.CEQ);
            Chunk.CodeGenerator.Emit(OpCode.FJMP, labels[i]);

            if (i < arms.Count - 1)
            {
                Chunk.CodeGenerator.Emit(OpCode.POP);
            }

            Visit(arms[i].ValueArm);
                
            if (arms[i].ValueArm is Expression)
            {
                Chunk.CodeGenerator.Emit(OpCode.JUMP, endLabel);
                Chunk.UpdateLabel(labels[i], Chunk.CodeGenerator.IP);
            }
        }

        if (elseArm != null)
        {
            Visit(elseArm);
        }
        else
        {
            Log.EmitWarning(
                "By-expression has no default arm and will always return `nil' if input matching fails. Consider adding a default arm using `else' keyword.",
                CurrentFileName,
                EvilMessageCode.NoDefaultByArm,
                byExpression.Line, 
                byExpression.Column
            );
                
            Chunk.CodeGenerator.Emit(OpCode.LDNIL);
        }
            
        Chunk.UpdateLabel(endLabel, Chunk.CodeGenerator.IP);
    }
}