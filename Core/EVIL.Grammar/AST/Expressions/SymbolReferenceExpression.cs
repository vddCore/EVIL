using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Expressions
{
    public sealed class SymbolReferenceExpression : Expression
    {
        public string Identifier { get; }

        public SymbolReferenceExpression(string identifier)
        {
            Identifier = identifier;
        }
    }
}