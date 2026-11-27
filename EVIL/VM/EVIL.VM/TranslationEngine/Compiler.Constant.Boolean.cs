namespace EVIL.VM.TranslationEngine;

using EVIL.Core.Grammar.AST.Constants;
using EVIL.VM.ExecutionEngine.Diagnostics;

public partial class Compiler
{
    protected override void Visit(BooleanConstant booleanConstant)
    {
        Chunk.CodeGenerator.Emit(
            booleanConstant.Value
                ? OpCode.LDTRUE
                : OpCode.LDFALSE
        );
    }
}