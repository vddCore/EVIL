namespace EVIL.Core.Grammar.AST.Statements;

using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Miscellaneous;

public sealed class IdentifierValDeclarator : ValDeclarator
{
    public IdentifierNode Identifier { get; }

    public IdentifierValDeclarator(
        IdentifierNode identifier,
        Expression? initializer
    ) : base(initializer)
    {
        Identifier = identifier;
        Reparent(Identifier);
    }
}