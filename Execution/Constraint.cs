using EVIL.AST.Base;

namespace EVIL.Execution
{
    public abstract class Constraint
    {
        public abstract bool Check(Interpreter interpreter, AstNode node);
    }
}