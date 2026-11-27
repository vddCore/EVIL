namespace EVIL.VM.TranslationEngine;

using EVIL.Core.Grammar.AST.Expressions;
using EVIL.VM.ExecutionEngine.Diagnostics;

public partial class Compiler
{
    protected override void Visit(TypeOfExpression typeOfExpression)
    {
        Visit(typeOfExpression.Target);

        if (typeOfExpression.IsNative)
        {
            Chunk.CodeGenerator.Emit(OpCode.NTYPE);
        }
        else
        {
            Chunk.CodeGenerator.Emit(OpCode.TYPE);
        }
    }
}