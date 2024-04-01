using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Statements;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(ThrowStatement throwStatement)
        {
            Visit(throwStatement.ThrownExpression);
            Chunk.CodeGenerator.Emit(OpCode.THROW);
        }
    }
}