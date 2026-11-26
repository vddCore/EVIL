namespace EVIL.Core.Grammar.AST.Expressions;

using EVIL.Core.Grammar.AST.Base;

public sealed class SymbolReferenceExpression : Expression
{
    public string Identifier { get; }

    public SymbolReferenceExpression(string identifier)
    {
        Identifier = identifier;
    }
}