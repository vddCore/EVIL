using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Statements;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(RetStatement retStatement)
        {
            if (retStatement.Expression != null)
            {
                Visit(retStatement.Expression);

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

            if (_closedScopes.Count > 1)
            {
                Chunk.CodeGenerator.Emit(OpCode.CRET);
            }
            else
            {
                Chunk.CodeGenerator.Emit(OpCode.RET);
            }
        }
    }
}