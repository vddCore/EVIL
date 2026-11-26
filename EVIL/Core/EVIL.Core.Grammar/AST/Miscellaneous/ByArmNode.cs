namespace EVIL.Core.Grammar.AST.Miscellaneous;

using EVIL.Core.Grammar.AST.Base;

public sealed class ByArmNode : AstNode
{
    public Expression Selector { get; }
    public AstNode ValueArm { get; }
    public bool DeepEquality { get; }

    public ByArmNode(Expression selector, AstNode valueArm, bool deepEquality)
    {
        Selector = selector;
        ValueArm = valueArm;
        DeepEquality = deepEquality;

        Reparent(Selector, valueArm);
    }
}