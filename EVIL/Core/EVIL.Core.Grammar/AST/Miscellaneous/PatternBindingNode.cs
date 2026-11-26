namespace EVIL.Core.Grammar.AST.Miscellaneous;

using EVIL.Core.Grammar.AST.Base;

public sealed class PatternBindingNode : AstNode
{
    public IdentifierNode Identifier { get; }
    public bool ReadWrite { get; }

    public PatternBindingNode(IdentifierNode identifier, bool readWrite)
    {
        Identifier = identifier;
        ReadWrite = readWrite;

        Reparent(Identifier);
    }
}