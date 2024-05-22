using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Constants;

namespace EVIL.Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(TypeCodeConstant typeCodeConstant)
        {
            Chunk.CodeGenerator.Emit(
                OpCode.LDTYPE,
                (int)typeCodeConstant.Value
            );
        }
    }
}