namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Constants;

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