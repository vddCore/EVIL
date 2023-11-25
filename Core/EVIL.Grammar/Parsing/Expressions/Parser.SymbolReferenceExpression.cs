using EVIL.Grammar.AST.Expressions;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private SymbolReferenceExpression SymbolReferenceExpression()
        {
            var identifier = Identifier();
            
            return new SymbolReferenceExpression(identifier.Name)
                { Line = identifier.Line, Column = identifier.Column };
        }
    }
}