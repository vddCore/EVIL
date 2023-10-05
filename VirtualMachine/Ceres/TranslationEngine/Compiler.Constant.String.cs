using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Constants;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(StringConstant stringConstant)
        {
            var id = Chunk.StringPool.FetchOrAdd(stringConstant.Value);
            Chunk.CodeGenerator.Emit(
                OpCode.LDSTR,
                (int)id
            );
        }
    }
}