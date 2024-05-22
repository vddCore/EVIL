using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Constants;

namespace EVIL.Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(BooleanConstant booleanConstant)
        {
            Chunk.CodeGenerator.Emit(
                booleanConstant.Value
                    ? OpCode.LDTRUE
                    : OpCode.LDFALSE
            );
        }
    }
}