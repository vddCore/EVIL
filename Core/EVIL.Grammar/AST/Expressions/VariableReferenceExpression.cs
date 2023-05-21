using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Expressions
{
    public sealed class VariableReferenceExpression : Expression
    {
        public string Identifier { get; }

        public VariableReferenceExpression(string identifier)
        {
            Identifier = identifier;
        }
    }
}