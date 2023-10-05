using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Statements;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(ReturnStatement returnStatement)
        {
            if (returnStatement.Expression != null)
            {
                Visit(returnStatement.Expression);

                if (Chunk.CodeGenerator.TryPeekOpCode(out var opCode))
                {
                    if (opCode == OpCode.TAILINVOKE)
                        return;
                }
            }
            else
            {
                Chunk.CodeGenerator.Emit(OpCode.LDNIL);
            }
            
            Chunk.CodeGenerator.Emit(OpCode.RET);
        }
    }
}