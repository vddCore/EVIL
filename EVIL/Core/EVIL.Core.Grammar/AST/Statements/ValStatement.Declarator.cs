namespace EVIL.Core.Grammar.AST.Statements;

using EVIL.Core.Grammar.AST.Base;

public abstract class ValDeclarator : AstNode
{
    public Expression? Initializer { get; }

    protected ValDeclarator(Expression? initializer)
    {
        Initializer = initializer;
        
        Reparent(Initializer);
    }
}