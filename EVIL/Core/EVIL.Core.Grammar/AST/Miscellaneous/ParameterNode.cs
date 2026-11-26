namespace EVIL.Core.Grammar.AST.Miscellaneous;

using EVIL.Core.Grammar.AST.Base;

public sealed class ParameterNode : AstNode
{
    public IdentifierNode Identifier { get; }
    public bool ReadWrite { get; }
        
    public ConstantExpression? Initializer { get; }

    public ParameterNode(IdentifierNode identifier, bool readWrite, ConstantExpression? initializer)
    {
        Identifier = identifier;
        ReadWrite = readWrite;
        Initializer = initializer;

        Reparent(Identifier);
        Reparent(Initializer);
    }
}