namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Grammar.AST.Constants;

public partial class Compiler
{
    protected override void Visit(NilConstant nilConstant)
    {
        Chunk.CodeGenerator.Emit(OpCode.LDNIL);
    }
}