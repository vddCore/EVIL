using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Statements;

namespace EVIL.Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(BreakStatement breakStatement)
        {
            Chunk.CodeGenerator.Emit(OpCode.JUMP, Loop.EndLabel);
        }
    }
}