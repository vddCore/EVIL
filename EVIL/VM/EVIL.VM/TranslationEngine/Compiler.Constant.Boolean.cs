namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Grammar.AST.Constants;

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