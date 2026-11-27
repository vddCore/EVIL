namespace EVIL.VM.TranslationEngine;

using EVIL.Core.Grammar.AST.Expressions;
using EVIL.VM.ExecutionEngine.Diagnostics;

public partial class Compiler
{
    protected override void Visit(ExtraArgumentsExpression extraArgumentsExpression)
    {
        var mode = extraArgumentsExpression.UnwrapOnStack ? (byte)1 : (byte)0;
        Chunk.CodeGenerator.Emit(OpCode.XARGS, mode);
    }
}