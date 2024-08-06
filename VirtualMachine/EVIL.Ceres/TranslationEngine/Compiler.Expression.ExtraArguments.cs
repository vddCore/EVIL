namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

public partial class Compiler
{
    public override void Visit(ExtraArgumentsExpression extraArgumentsExpression)
    {
        var mode = extraArgumentsExpression.UnwrapOnStack ? (byte)1 : (byte)0;
        Chunk.CodeGenerator.Emit(OpCode.XARGS, mode);
    }
}