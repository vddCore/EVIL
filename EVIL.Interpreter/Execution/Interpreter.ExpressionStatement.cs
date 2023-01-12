using EVIL.Grammar.AST;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override void Visit(ExpressionStatement expressionStatement)
        {
            Visit(expressionStatement.Expression);
        }
    }
}