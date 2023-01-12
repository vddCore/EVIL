using EVIL.Grammar.AST;

namespace EVIL.Interpreter.Execution
{
    public abstract class Constraint
    {
        public abstract bool Check(Interpreter interpreter, AstNode node);
    }
}