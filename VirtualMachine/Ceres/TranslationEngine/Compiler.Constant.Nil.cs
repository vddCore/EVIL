using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Constants;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(NilConstant nilConstant)
        {
            Chunk.CodeGenerator.Emit(OpCode.LDNIL);
        }
    }
}