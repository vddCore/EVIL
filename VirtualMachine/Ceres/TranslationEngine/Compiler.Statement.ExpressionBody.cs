using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Statements;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(ExpressionBodyStatement expressionBodyStatement)
        {
            _blockDescent++;
            Visit(expressionBodyStatement.Expression);
            Chunk.CodeGenerator.Emit(OpCode.RET);
            _blockDescent--;
        }
    }
}