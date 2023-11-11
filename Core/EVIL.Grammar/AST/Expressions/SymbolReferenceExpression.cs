using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Expressions
{
    public sealed class SymbolReferenceExpression : Expression
    {
        public string Identifier { get; }

        public override bool CanBeNil => true;

        public SymbolReferenceExpression(string identifier)
        {
            Identifier = identifier;
        }
    }
}