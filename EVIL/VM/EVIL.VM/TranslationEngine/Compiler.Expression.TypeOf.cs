namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Grammar.AST.Expressions;

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