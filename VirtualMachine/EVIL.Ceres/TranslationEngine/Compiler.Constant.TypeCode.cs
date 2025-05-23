namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Constants;

public partial class Compiler
{
    protected override void Visit(TypeCodeConstant typeCodeConstant)
    {
        Chunk.CodeGenerator.Emit(
            OpCode.LDTYPE,
            (int)typeCodeConstant.Value
        );
    }
}