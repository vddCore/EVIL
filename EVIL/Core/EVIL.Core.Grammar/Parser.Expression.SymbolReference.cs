namespace EVIL.Core.Grammar;

using EVIL.Core.Grammar.AST.Expressions;

public partial class Parser
{
    private SymbolReferenceExpression SymbolReferenceExpression()
    {
        var identifier = Identifier();
            
        return new SymbolReferenceExpression(identifier.Name)
            { Line = identifier.Line, Column = identifier.Column };
    }
}