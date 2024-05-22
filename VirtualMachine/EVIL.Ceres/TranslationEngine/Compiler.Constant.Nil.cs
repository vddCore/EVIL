using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Constants;

namespace EVIL.Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(NilConstant nilConstant)
        {
            Chunk.CodeGenerator.Emit(OpCode.LDNIL);
        }
    }
}