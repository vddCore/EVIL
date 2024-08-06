namespace EVIL.Grammar.AST.Miscellaneous;

using EVIL.Grammar.AST.Base;

public sealed class IdentifierNode : AstNode
{
    public string Name { get; }

    public IdentifierNode(string name)
    {
        Name = name;
    }
}