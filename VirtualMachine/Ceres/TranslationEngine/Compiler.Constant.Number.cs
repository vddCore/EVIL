using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Constants;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(NumberConstant numberConstant)
        {
            if (numberConstant.Value.Equals(0))
            {
                Chunk.CodeGenerator.Emit(OpCode.LDZERO);
            }
            else if (numberConstant.Value.Equals(1))
            {
                Chunk.CodeGenerator.Emit(OpCode.LDONE);
            }
            else
            {
                Chunk.CodeGenerator.Emit(
                    OpCode.LDNUM,
                    numberConstant.Value
                );
            }
        }
    }
}