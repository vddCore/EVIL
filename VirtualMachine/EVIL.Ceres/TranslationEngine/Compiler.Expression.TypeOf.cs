namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

public partial class Compiler
{
    public override void Visit(TypeOfExpression typeOfExpression)
    {
        Visit(typeOfExpression.Target);
        Chunk.CodeGenerator.Emit(OpCode.TYPE);
    }
}