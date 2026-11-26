namespace EVIL.Core.Grammar.AST.Miscellaneous;

using EVIL.Core.Grammar.AST.Base;

public sealed class IdentifierNode : AstNode
{
    public string Name { get; }

    public IdentifierNode(string name)
    {
        Name = name;
    }
}