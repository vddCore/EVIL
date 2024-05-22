using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Statements;

namespace EVIL.Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(ExpressionStatement expressionStatement)
        {
            Visit(expressionStatement.Expression);

            if (expressionStatement.Expression is not AssignmentExpression)
            {
                Chunk.CodeGenerator.Emit(OpCode.POP);
            }
        }
    }
}