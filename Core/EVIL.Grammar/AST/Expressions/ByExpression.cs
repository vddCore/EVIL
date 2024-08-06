namespace EVIL.Grammar.AST.Expressions;

using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

public sealed class ByExpression : Expression
{
    public Expression Qualifier { get; }
    public List<ByArmNode> Arms { get; }
    public AstNode? ElseArm { get; }

    public ByExpression(Expression qualifier, List<ByArmNode> arms, AstNode? elseArm)
    {
        Qualifier = qualifier;
        Arms = arms;
        ElseArm = elseArm;

        Reparent(Qualifier);
        Reparent(Arms);
        Reparent(ElseArm);
    }
}